using System.Diagnostics;
using XekuII.CLI.Models;

namespace XekuII.CLI.Services;

/// <summary>
/// Manages local process lifecycle for services.
/// </summary>
public class LocalProcessRunner : IProcessRunner
{
    private readonly PortChecker _portChecker;

    public LocalProcessRunner(PortChecker portChecker)
    {
        _portChecker = portChecker;
    }

    /// <summary>
    /// Starts a service process in the background.
    /// </summary>
    public async Task<StartResult> StartAsync(ServiceDefinition service, CancellationToken ct = default)
    {
        // Check if port is already in use
        if (_portChecker.IsPortInUse(service.Port))
        {
            var existingPid = _portChecker.GetProcessIdByPort(service.Port);
            return new StartResult(false, $"端口 {service.Port} 已被佔用 (PID: {existingPid?.ToString() ?? "未知"})")
            {
                ProcessId = existingPid
            };
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = service.Command,
                Arguments = service.Arguments,
                WorkingDirectory = service.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Set environment variables
            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
            startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";

            var process = Process.Start(startInfo);
            if (process == null)
            {
                return new StartResult(false, "無法啟動進程");
            }

            // Start reading output asynchronously to prevent blocking
            _ = ConsumeOutputAsync(process);

            // Wait for service to become healthy
            var healthCheckResult = await WaitForHealthyAsync(
                service.HealthCheckUrl,
                service.StartTimeout,
                service.HealthCheckInterval,
                ct);

            if (!healthCheckResult)
            {
                // For npm/node services, the wrapper script may exit while the actual server runs
                // Check if the port is now in use (service started successfully)
                if (_portChecker.IsPortInUse(service.Port))
                {
                    var actualPid = _portChecker.GetProcessIdByPort(service.Port);
                    return new StartResult(true, null)
                    {
                        ProcessId = actualPid ?? process.Id
                    };
                }

                if (process.HasExited)
                {
                    return new StartResult(false, "服務啟動後立即終止")
                    {
                        ProcessId = null
                    };
                }

                // Service started but health check timed out
                return new StartResult(true, "服務已啟動但健康檢查逾時，可能仍在初始化中")
                {
                    ProcessId = process.Id
                };
            }

            // Service is healthy - get the actual PID (may differ from wrapper script PID)
            var runningPid = _portChecker.GetProcessIdByPort(service.Port) ?? process.Id;
            return new StartResult(true, null)
            {
                ProcessId = runningPid
            };
        }
        catch (Exception ex)
        {
            return new StartResult(false, $"啟動失敗: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops a process by its ID.
    /// </summary>
    public StopResult Stop(string serviceName, int? processId)
    {
        if (!processId.HasValue)
        {
            return new StopResult(true, "無進程 ID");
        }

        try
        {
            var process = Process.GetProcessById(processId.Value);

            // Try graceful shutdown first
            try
            {
                process.CloseMainWindow();
            }
            catch
            {
                // Ignore if CloseMainWindow fails
            }

            // Wait up to 5 seconds for graceful exit
            if (!process.WaitForExit(5000))
            {
                // Force kill
                process.Kill(entireProcessTree: true);
                process.WaitForExit(3000);
            }

            return new StopResult(true, "已停止");
        }
        catch (ArgumentException)
        {
            // Process doesn't exist - consider it a success
            return new StopResult(true, "進程已不存在");
        }
        catch (Exception ex)
        {
            return new StopResult(false, $"停止進程失敗: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a process is still running.
    /// </summary>
    public bool IsRunning(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> WaitForHealthyAsync(
        string healthCheckUrl,
        TimeSpan timeout,
        TimeSpan interval,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (ct.IsCancellationRequested)
                return false;

            if (await _portChecker.IsServiceHealthyAsync(healthCheckUrl, ct))
                return true;

            try
            {
                await Task.Delay(interval, ct);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        return false;
    }

    private static async Task ConsumeOutputAsync(Process process)
    {
        // Consume stdout and stderr to prevent buffer blocking
        // We don't need to do anything with the output
        try
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await Task.WhenAll(stdoutTask, stderrTask);
        }
        catch
        {
            // Ignore errors
        }
    }
}
