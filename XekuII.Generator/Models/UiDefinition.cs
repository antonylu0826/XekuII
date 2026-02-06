namespace XekuII.Generator.Models;

/// <summary>
/// UI configuration for frontend code generation.
/// </summary>
public class UiDefinition
{
    public ListUiDefinition? List { get; set; }
    public FormUiDefinition? Form { get; set; }
    public DetailUiDefinition? Detail { get; set; }
}

/// <summary>
/// List page UI configuration.
/// </summary>
public class ListUiDefinition
{
    public List<string> Columns { get; set; } = new();
    public string? DefaultSort { get; set; }
    public string? DefaultSortDir { get; set; }
    public List<string> Searchable { get; set; } = new();
    public List<string> Filterable { get; set; } = new();
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Form page UI configuration.
/// </summary>
public class FormUiDefinition
{
    public List<FormRow> Layout { get; set; } = new();
}

/// <summary>
/// A row in a form layout containing one or more field names.
/// </summary>
public class FormRow
{
    public List<string> Row { get; set; } = new();
}

/// <summary>
/// Detail page UI configuration.
/// </summary>
public class DetailUiDefinition
{
    public List<DetailSection> Sections { get; set; } = new();
}

/// <summary>
/// A section in the detail page.
/// </summary>
public class DetailSection
{
    public string Title { get; set; } = string.Empty;
    public List<string>? Fields { get; set; }
    public string? Relation { get; set; }
}
