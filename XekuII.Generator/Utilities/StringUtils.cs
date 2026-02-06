namespace XekuII.Generator.Utilities;

/// <summary>
/// Shared string utilities for code generation.
/// Centralizes naming convention transformations to ensure consistency across all generators.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// Convert PascalCase to camelCase.
    /// Example: "OrderLine" → "orderLine"
    /// </summary>
    public static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLower(name[0]) + name[1..];
    }

    /// <summary>
    /// Convert PascalCase to kebab-case.
    /// Example: "OrderLine" → "order-line"
    /// </summary>
    public static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
                result.Append('-');
            result.Append(char.ToLower(name[i]));
        }
        return result.ToString();
    }

    /// <summary>
    /// Simple English pluralization.
    /// Handles common cases: -y → -ies, -s/-x/-ch/-sh → -es, default → -s
    /// </summary>
    public static string Pluralize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        if (name.EndsWith("y") && !name.EndsWith("ay") && !name.EndsWith("ey") && !name.EndsWith("oy") && !name.EndsWith("uy"))
            return name[..^1] + "ies";
        if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("ch") || name.EndsWith("sh"))
            return name + "es";
        return name + "s";
    }

    /// <summary>
    /// Escape string for C# string literals.
    /// </summary>
    public static string EscapeString(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
