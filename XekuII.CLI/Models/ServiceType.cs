namespace XekuII.CLI.Models;

/// <summary>
/// Defines the type of service that can be managed.
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// .NET backend API service.
    /// </summary>
    Backend,

    /// <summary>
    /// Node.js/Vite frontend service.
    /// </summary>
    Frontend
}
