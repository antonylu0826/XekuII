using System.Text.Json;
using System.Text.Json.Serialization;
using XekuII.CLI.Models;

namespace XekuII.CLI.Services;

/// <summary>
/// Persists and retrieves service state from a JSON file.
/// </summary>
public class ServiceStateStore
{
    private readonly string _stateFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public ServiceStateStore(string? workingDirectory = null)
    {
        var baseDir = workingDirectory ?? Directory.GetCurrentDirectory();
        var xekuiiDir = Path.Combine(baseDir, ".xekuii");
        _stateFilePath = Path.Combine(xekuiiDir, "services.json");
    }

    /// <summary>
    /// Loads all service states from the state file.
    /// </summary>
    public Dictionary<string, ServiceState> LoadStates()
    {
        if (!File.Exists(_stateFilePath))
        {
            return new Dictionary<string, ServiceState>();
        }

        try
        {
            var json = File.ReadAllText(_stateFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, ServiceState>>(json, JsonOptions)
                   ?? new Dictionary<string, ServiceState>();
        }
        catch
        {
            return new Dictionary<string, ServiceState>();
        }
    }

    /// <summary>
    /// Saves all service states to the state file.
    /// </summary>
    public void SaveStates(Dictionary<string, ServiceState> states)
    {
        var directory = Path.GetDirectoryName(_stateFilePath)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(states, JsonOptions);
        File.WriteAllText(_stateFilePath, json);
    }

    /// <summary>
    /// Updates a single service state.
    /// </summary>
    public void UpdateState(ServiceState state)
    {
        var states = LoadStates();
        states[state.Name] = state;
        SaveStates(states);
    }

    /// <summary>
    /// Removes a service state.
    /// </summary>
    public void RemoveState(string serviceName)
    {
        var states = LoadStates();
        if (states.Remove(serviceName))
        {
            SaveStates(states);
        }
    }
}
