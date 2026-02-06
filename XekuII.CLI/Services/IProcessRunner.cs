using XekuII.CLI.Models;

namespace XekuII.CLI.Services;

/// <summary>
/// Abstraction for process management to support different execution modes (local, Docker, etc.).
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Starts a service process.
    /// </summary>
    Task<StartResult> StartAsync(ServiceDefinition service, CancellationToken ct = default);

    /// <summary>
    /// Stops a service process.
    /// </summary>
    StopResult Stop(string serviceName, int? processId);

    /// <summary>
    /// Checks if a process is still running.
    /// </summary>
    bool IsRunning(int processId);
}

/// <summary>
/// Result of a service start operation.
/// </summary>
public record StartResult(bool Success, string? Message)
{
    public int? ProcessId { get; init; }
}

/// <summary>
/// Result of a service stop operation.
/// </summary>
public record StopResult(bool Success, string? Message);
