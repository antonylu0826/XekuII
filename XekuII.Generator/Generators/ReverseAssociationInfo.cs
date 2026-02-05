namespace XekuII.Generator.Generators;

/// <summary>
/// Information about a reverse association to be generated.
/// </summary>
public class ReverseAssociationInfo
{
    public string TargetEntity { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string SourceEntity { get; set; } = string.Empty;
    public string AssociationName { get; set; } = string.Empty;
    public bool IsCollection { get; set; }
}
