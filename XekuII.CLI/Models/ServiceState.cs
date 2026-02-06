namespace XekuII.CLI.Models;

/// <summary>
/// Represents the runtime state of a service.
/// </summary>
public class ServiceState
{
    public required string Name { get; set; }
    public required ServiceType Type { get; set; }
    public int? ProcessId { get; set; }
    public int Port { get; set; }
    public DateTime? StartedAt { get; set; }
    public ServiceStatus Status { get; set; } = ServiceStatus.Stopped;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Possible status values for a service.
/// </summary>
public enum ServiceStatus
{
    /// <summary>Service is not running.</summary>
    Stopped,

    /// <summary>Service is starting up.</summary>
    Starting,

    /// <summary>Service is running and healthy.</summary>
    Running,

    /// <summary>Process exists but health check failed.</summary>
    Unhealthy,

    /// <summary>Service failed to start or crashed.</summary>
    Error
}
