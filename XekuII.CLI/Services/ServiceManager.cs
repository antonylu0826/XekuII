using XekuII.CLI.Models;

namespace XekuII.CLI.Services;

/// <summary>
/// Central orchestrator for managing XekuII services.
/// </summary>
public class ServiceManager : IDisposable
{
    private readonly IProcessRunner _processRunner;
    private readonly PortChecker _portChecker;
    private readonly ServiceStateStore _stateStore;
    private readonly Dictionary<string, ServiceDefinition> _serviceDefinitions;

    public ServiceManager(string workingDirectory)
    {
        _portChecker = new PortChecker();
        _processRunner = new LocalProcessRunner(_portChecker);
        _stateStore = new ServiceStateStore(workingDirectory);

        // Define service configurations
        _serviceDefinitions = new Dictionary<string, ServiceDefinition>
        {
            ["backend"] = new ServiceDefinition
            {
                Name = "backend",
                Type = ServiceType.Backend,
                WorkingDirectory = Path.Combine(workingDirectory, "XekuII.ApiHost"),
                Command = "dotnet",
                Arguments = "run --no-launch-profile",
                Port = 5000,
                HealthCheckUrl = "http://localhost:5000/swagger/index.html",
                StartTimeout = TimeSpan.FromSeconds(90),
                HealthCheckInterval = TimeSpan.FromSeconds(3)
            },
            ["frontend"] = new ServiceDefinition
            {
                Name = "frontend",
                Type = ServiceType.Frontend,
                WorkingDirectory = Path.Combine(workingDirectory, "xekuii-web"),
                Command = OperatingSystem.IsWindows() ? "cmd.exe" : "npm",
                Arguments = OperatingSystem.IsWindows() ? "/c npm run dev" : "run dev",
                Port = 5173,
                HealthCheckUrl = "http://localhost:5173",
                StartTimeout = TimeSpan.FromSeconds(60),
                HealthCheckInterval = TimeSpan.FromSeconds(2)
            }
        };
    }

    /// <summary>
    /// Starts specified services.
    /// </summary>
    public async Task<List<ServiceStartResult>> StartServicesAsync(
        bool backend = false,
        bool frontend = false,
        bool all = false,
        CancellationToken ct = default)
    {
        var results = new List<ServiceStartResult>();
        var servicesToStart = GetServicesToManage(backend, frontend, all);

        foreach (var serviceName in servicesToStart)
        {
            var result = await StartServiceAsync(serviceName, ct);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Stops specified services.
    /// </summary>
    public List<ServiceStopResult> StopServices(
        bool backend = false,
        bool frontend = false,
        bool all = false)
    {
        var results = new List<ServiceStopResult>();
        var servicesToStop = GetServicesToManage(backend, frontend, all);

        foreach (var serviceName in servicesToStop)
        {
            var result = StopService(serviceName);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Gets the status of all services.
    /// </summary>
    public List<ServiceStatusInfo> GetStatus()
    {
        var statusList = new List<ServiceStatusInfo>();
        var states = _stateStore.LoadStates();

        foreach (var (name, definition) in _serviceDefinitions)
        {
            var status = new ServiceStatusInfo
            {
                Name = name,
                Type = definition.Type,
                Port = definition.Port,
                Url = definition.HealthCheckUrl
            };

            // Check persisted state
            if (states.TryGetValue(name, out var state) && state.ProcessId.HasValue)
            {
                status.ProcessId = state.ProcessId;
                status.StartedAt = state.StartedAt;

                // Verify process is still running
                if (_processRunner.IsRunning(state.ProcessId.Value))
                {
                    // Perform health check
                    var isHealthy = _portChecker.IsServiceHealthyAsync(definition.HealthCheckUrl)
                        .GetAwaiter().GetResult();

                    status.Status = isHealthy ? ServiceStatus.Running : ServiceStatus.Unhealthy;
                }
                else
                {
                    // Process terminated, clean up state
                    status.Status = ServiceStatus.Stopped;
                    _stateStore.RemoveState(name);
                }
            }
            else
            {
                // Check if port is used by external process
                if (_portChecker.IsPortInUse(definition.Port))
                {
                    status.ProcessId = _portChecker.GetProcessIdByPort(definition.Port);
                    status.Status = ServiceStatus.Running;
                    status.Note = "由外部進程管理";
                }
                else
                {
                    status.Status = ServiceStatus.Stopped;
                }
            }

            statusList.Add(status);
        }

        return statusList;
    }

    private async Task<ServiceStartResult> StartServiceAsync(
        string serviceName,
        CancellationToken ct)
    {
        if (!_serviceDefinitions.TryGetValue(serviceName, out var definition))
        {
            return new ServiceStartResult(serviceName, false, $"未知的服務: {serviceName}");
        }

        Console.WriteLine($"正在啟動 {serviceName}...");

        var result = await _processRunner.StartAsync(definition, ct);

        if (result.ProcessId.HasValue)
        {
            // Save state
            var state = new ServiceState
            {
                Name = serviceName,
                Type = definition.Type,
                ProcessId = result.ProcessId,
                Port = definition.Port,
                StartedAt = DateTime.Now,
                Status = result.Success ? ServiceStatus.Running : ServiceStatus.Unhealthy,
                ErrorMessage = result.Message
            };
            _stateStore.UpdateState(state);
        }

        return new ServiceStartResult(serviceName, result.Success, result.Message)
        {
            ProcessId = result.ProcessId,
            Port = definition.Port,
            Url = definition.HealthCheckUrl
        };
    }

    private ServiceStopResult StopService(string serviceName)
    {
        var states = _stateStore.LoadStates();
        int? processId = null;

        if (states.TryGetValue(serviceName, out var state) && state.ProcessId.HasValue)
        {
            processId = state.ProcessId;
        }
        else if (_serviceDefinitions.TryGetValue(serviceName, out var definition))
        {
            // Try to find process by port
            processId = _portChecker.GetProcessIdByPort(definition.Port);
        }

        var result = _processRunner.Stop(serviceName, processId);
        _stateStore.RemoveState(serviceName);

        return new ServiceStopResult(serviceName, result.Success, result.Message);
    }

    private List<string> GetServicesToManage(bool backend, bool frontend, bool all)
    {
        if (all || (!backend && !frontend))
        {
            return _serviceDefinitions.Keys.ToList();
        }

        var services = new List<string>();
        if (backend) services.Add("backend");
        if (frontend) services.Add("frontend");
        return services;
    }


    public void Dispose()
    {
        _portChecker.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Result of a service start operation with additional metadata.
/// </summary>
public record ServiceStartResult(string ServiceName, bool Success, string? Message)
{
    public int? ProcessId { get; init; }
    public int? Port { get; init; }
    public string? Url { get; init; }
}

/// <summary>
/// Result of a service stop operation.
/// </summary>
public record ServiceStopResult(string ServiceName, bool Success, string? Message);

/// <summary>
/// Current status information for a service.
/// </summary>
public class ServiceStatusInfo
{
    public required string Name { get; init; }
    public required ServiceType Type { get; init; }
    public int? ProcessId { get; set; }
    public required int Port { get; init; }
    public required string Url { get; init; }
    public DateTime? StartedAt { get; set; }
    public ServiceStatus Status { get; set; }
    public string? Note { get; set; }
}
