namespace XekuII.CLI.Models;

/// <summary>
/// Defines a service that can be managed by the CLI.
/// </summary>
public class ServiceDefinition
{
    public required string Name { get; init; }
    public required ServiceType Type { get; init; }
    public required string WorkingDirectory { get; init; }
    public required string Command { get; init; }
    public required string Arguments { get; init; }
    public required int Port { get; init; }
    public required string HealthCheckUrl { get; init; }
    public TimeSpan StartTimeout { get; init; } = TimeSpan.FromSeconds(90);
    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(3);
}
