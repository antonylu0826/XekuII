using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace XekuII.CLI.Services;

/// <summary>
/// Provides port availability and service health checks.
/// </summary>
public class PortChecker : IDisposable
{
    private readonly HttpClient _httpClient;

    public PortChecker()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    /// <summary>
    /// Checks if a port is in use by any process.
    /// </summary>
    public bool IsPortInUse(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return false;
        }
        catch (SocketException)
        {
            return true;
        }
    }

    /// <summary>
    /// Checks if a service is responding at the given URL.
    /// </summary>
    public async Task<bool> IsServiceHealthyAsync(string healthCheckUrl, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(healthCheckUrl, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finds the process ID using a specific port (Windows implementation).
    /// </summary>
    public int? GetProcessIdByPort(int port)
    {
        if (!OperatingSystem.IsWindows())
        {
            return GetProcessIdByPortUnix(port);
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                if (!line.Contains("LISTENING"))
                    continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                    continue;

                // Parse the local address (e.g., "127.0.0.1:5000" or "[::1]:5000")
                var localAddr = parts[1];
                var lastColon = localAddr.LastIndexOf(':');
                if (lastColon < 0)
                    continue;

                var portStr = localAddr[(lastColon + 1)..];
                if (int.TryParse(portStr, out var parsedPort) && parsedPort == port)
                {
                    if (int.TryParse(parts[^1], out var pid))
                    {
                        return pid;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    private int? GetProcessIdByPortUnix(int port)
    {
        try
        {
            // Try lsof first (macOS and most Linux)
            var startInfo = new ProcessStartInfo
            {
                FileName = "lsof",
                Arguments = $"-i :{port} -t",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0 && int.TryParse(output.Split('\n')[0], out var pid))
            {
                return pid;
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
