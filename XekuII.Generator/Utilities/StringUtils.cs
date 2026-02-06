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
        
        if (!char.IsUpper(name[0])) return name;

        char[] chars = name.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (i > 0 && !char.IsUpper(chars[i])) break;
            
            // Check if the next character is lower case.
            // If so, we found the end of the acronym.
            // We should stop lowercasing, unless we are at the first character.
            // Example: "IOStream" -> "ioStream" (i=1 is 'O', i+1='S', wait.. S is upper)
            
            // Example "Power" -> 'P', next 'o'. i=0. Lower 'P'.
            
            // Example "SKU" -> S(next K), K(next U), U(next end). All lower.
            
            // Correct logic based on System.Text.Json:
            // "If the first two characters are uppercase, we assume it is an acronym and lowercase valid uppercase characters."
            
            bool hasNext = (i + 1 < chars.Length);
            
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                // Current is Upper, Next is Lower.
                // This is the last character of the acronym (e.g. 'T' in "IPTree").
                // We should keep this uppercase, UNLESS it is the very first character.
                break;
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }
        
        return new string(chars);
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
