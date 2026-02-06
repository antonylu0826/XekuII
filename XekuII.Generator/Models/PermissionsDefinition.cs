namespace XekuII.Generator.Models;

/// <summary>
/// Permission configuration for frontend/backend access control.
/// </summary>
public class PermissionsDefinition
{
    public string Read { get; set; } = "Default";
    public string Create { get; set; } = "Default";
    public string Update { get; set; } = "Default";
    public string Delete { get; set; } = "Administrators";
}
