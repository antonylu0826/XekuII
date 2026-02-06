using XekuII.Generator.Models;

namespace XekuII.Generator.Generators;

/// <summary>
/// Facade for generating React pages. 
/// Delegates to specialized generators for List, Form, Detail, Routes, and Navigation.
/// </summary>
public class ReactPageGenerator
{
    private readonly ReactListPageGenerator _listGenerator = new();
    private readonly ReactFormPageGenerator _formGenerator = new();
    private readonly ReactDetailPageGenerator _detailGenerator = new();
    private readonly ReactMetadataGenerator _metadataGenerator = new();

    public string GenerateListPage(EntityDefinition entity)
    {
        return _listGenerator.Generate(entity);
    }

    public string GenerateFormPage(EntityDefinition entity, Dictionary<string, EntityDefinition>? entityMap = null)
    {
        return _formGenerator.Generate(entity, entityMap);
    }

    public string GenerateDetailPage(EntityDefinition entity, Dictionary<string, EntityDefinition>? entityMap = null)
    {
        return _detailGenerator.Generate(entity, entityMap);
    }

    public string GenerateRoutes(List<EntityDefinition> entities)
    {
        return _metadataGenerator.GenerateRoutes(entities);
    }

    public string GenerateNavigation(List<EntityDefinition> entities)
    {
        return _metadataGenerator.GenerateNavigation(entities);
    }
}
