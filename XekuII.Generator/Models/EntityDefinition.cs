namespace XekuII.Generator.Models;

/// <summary>
/// Represents an entity definition parsed from YAML.
/// </summary>
public class EntityDefinition
{
    public string Entity { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? Icon { get; set; }
    public string? DbTable { get; set; }

    /// <summary>
    /// Description for AI agents to understand the entity's purpose.
    /// Will be generated as [Description("...")] attribute.
    /// </summary>
    public string? Description { get; set; }

    public List<FieldDefinition> Fields { get; set; } = new();
    public List<RelationDefinition> Relations { get; set; } = new();
    public List<RuleDefinition> Rules { get; set; } = new();

    /// <summary>
    /// Enum definitions scoped to this entity.
    /// </summary>
    public List<EnumDefinition> Enums { get; set; } = new();
}

/// <summary>
/// Represents a field definition within an entity.
/// </summary>
public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public int? Length { get; set; }
    public bool Required { get; set; }
    public bool Readonly { get; set; }
    public string? Label { get; set; }
    public string? Default { get; set; }
    public string? Validation { get; set; }

    /// <summary>
    /// Description for AI agents to understand the field's purpose.
    /// Will be generated as [Description("...")] attribute.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Calculation formula for computed properties.
    /// For persistent type: Uses XPO criteria syntax (e.g., "[Quantity] * [UnitPrice]")
    /// For getter type: Uses C# expression syntax (e.g., "Quantity * UnitPrice")
    /// </summary>
    public string? Formula { get; set; }

    /// <summary>
    /// Type of calculation: "persistent" (PersistentAlias) or "getter" (C# property getter).
    /// Defaults to "persistent" if Formula is set but CalculationType is not specified.
    /// </summary>
    public string? CalculationType { get; set; }

    /// <summary>
    /// List of validation rules for this field.
    /// </summary>
    public List<ValidationRule> Validations { get; set; } = new();
}

/// <summary>
/// Represents a validation rule for a field.
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// Range validation (e.g., ">0", ">=1", "0-100", "<=99999").
    /// Supports: ">N", ">=N", "&lt;N", "&lt;=N", "N-M" (inclusive range)
    /// </summary>
    public string? Range { get; set; }

    /// <summary>
    /// Minimum value for numeric fields.
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    /// Maximum value for numeric fields.
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    /// Regular expression pattern for string validation.
    /// </summary>
    public string? Regex { get; set; }

    /// <summary>
    /// XAF criteria expression for complex validation.
    /// </summary>
    public string? Criteria { get; set; }

    /// <summary>
    /// Custom error message for this validation rule.
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Represents a relation (reference or detail) definition.
/// </summary>
public class RelationDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "reference"; // reference or detail
    public string Target { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? Label { get; set; }
    public string? LookupField { get; set; }
    public string? Cascade { get; set; }
    public int? MinCount { get; set; }

    /// <summary>
    /// Description for AI agents to understand the relation's purpose.
    /// Will be generated as [Description("...")] attribute.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Represents a business rule trigger point.
/// </summary>
public class RuleDefinition
{
    public string Trigger { get; set; } = string.Empty; // BeforeSave, AfterSave, etc.
    public string Script { get; set; } = string.Empty;  // Partial method name
}

/// <summary>
/// Represents an enum definition for generating C# enums.
/// </summary>
public class EnumDefinition
{
    /// <summary>
    /// Enum type name (PascalCase).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description for AI agents to understand the enum's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of enum members.
    /// </summary>
    public List<EnumMember> Members { get; set; } = new();
}

/// <summary>
/// Represents a single enum member/value.
/// </summary>
public class EnumMember
{
    /// <summary>
    /// Member name (PascalCase).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional explicit integer value.
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// Description for AI agents to understand this enum value.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display label (for UI).
    /// </summary>
    public string? Label { get; set; }
}
