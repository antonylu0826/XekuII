using XekuII.Generator.Models;
using static XekuII.Generator.Utilities.StringUtils;

namespace XekuII.Generator.Utilities;

public static class ReactGeneratorUtils
{
    public const string Indent = "  ";

    public static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BO_Product"] = "Package",
        ["BO_Customer"] = "Users",
        ["BO_Order"] = "ShoppingCart",
        ["BO_Invoice"] = "FileText",
        ["BO_Category"] = "Folder",
        ["BO_Report"] = "BarChart3",
        ["BO_Settings"] = "Settings",
        ["BO_Calendar"] = "Calendar",
        ["BO_Task"] = "CheckSquare",
        ["BO_Note"] = "StickyNote",
    };

    public static string MapIcon(string entityName)
    {
        if (IconMap.TryGetValue($"BO_{entityName}", out var icon))
            return icon;
        return "Circle";
    }

    public static string MapFieldType(FieldDefinition field, List<EnumDefinition>? enums)
    {
        var enumNames = enums?.Select(e => e.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
                        ?? new HashSet<string>();
        if (enumNames.Contains(field.Type))
            return "select";

        return field.Type.ToLower() switch
        {
            "string" when field.Length is > 200 => "textarea",
            "string" => "text",
            "int" => "number",
            "decimal" => "number",
            "double" => "number",
            "bool" => "boolean",
            "datetime" => "date",
            _ => "text",
        };
    }

    public static string GetDefaultForType(FieldDefinition field, List<EnumDefinition>? enums)
    {
        var enumDef = enums?.FirstOrDefault(e => e.Name.Equals(field.Type, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(field.Default))
        {
            if (enumDef != null)
            {
                var member = enumDef.Members.FirstOrDefault(m => m.Name.Equals(field.Default, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                    return $"{enumDef.Name}.{member.Name}";
                return "0";
            }

            return field.Type.ToLower() switch
            {
                "string" => $"\"{field.Default}\"",
                "bool" => field.Default.ToLower(),
                "datetime" when field.Default.Equals("now", StringComparison.OrdinalIgnoreCase)
                    => "new Date().toISOString()",
                "datetime" when field.Default.Equals("today", StringComparison.OrdinalIgnoreCase)
                    => "new Date().toISOString().split('T')[0]",
                "datetime" => $"\"{field.Default}\"",
                _ => field.Default,
            };
        }

        if (enumDef != null)
            return "0";

        return field.Type.ToLower() switch
        {
            "string" => "\"\"",
            "int" => "0",
            "decimal" => "0",
            "double" => "0",
            "bool" => "false",
            "datetime" => "\"\"",
            "guid" => "\"\"",
            _ => "\"\"",
        };
    }

    public static List<string> ResolveListColumns(EntityDefinition entity)
    {
        if (entity.Ui?.List?.Columns is { Count: > 0 } cols)
            return cols;

        // Default: all non-calculated, non-detail fields + references
        var columns = entity.Fields
            .Where(f => string.IsNullOrEmpty(f.Formula))
            .Select(f => f.Name)
            .ToList();

        columns.AddRange(entity.Relations
            .Where(r => r.Type == "reference")
            .Select(r => r.Name));

        return columns;
    }

    public static List<string> ResolveSearchable(EntityDefinition entity)
    {
        if (entity.Ui?.List?.Searchable is { Count: > 0 } s)
            return s;

        return entity.Fields
            .Where(f => f.Type.ToLower() == "string" && string.IsNullOrEmpty(f.Formula))
            .Select(f => f.Name)
            .Take(2)
            .ToList();
    }
}
