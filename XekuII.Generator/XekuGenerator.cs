using XekuII.Generator.Generators;
using XekuII.Generator.Models;
using XekuII.Generator.Parsers;

namespace XekuII.Generator;

/// <summary>
/// Main entry point for XekuII code generation.
/// </summary>
public class XekuGenerator
{
    private readonly YamlEntityParser _parser;
    private readonly CSharpCodeGenerator _codeGenerator;
    private readonly ControllerCodeGenerator _controllerGenerator;
    private readonly TypeScriptTypeGenerator _tsTypeGenerator;
    private readonly ZodSchemaGenerator _zodSchemaGenerator;
    private readonly ApiClientGenerator _apiClientGenerator;
    private readonly ReactPageGenerator _reactPageGenerator;

    private readonly string _outputDirectory;

    public XekuGenerator(string outputDirectory)
    {
        _parser = new YamlEntityParser();
        _codeGenerator = new CSharpCodeGenerator();
        _controllerGenerator = new ControllerCodeGenerator();
        _tsTypeGenerator = new TypeScriptTypeGenerator();
        _zodSchemaGenerator = new ZodSchemaGenerator();
        _apiClientGenerator = new ApiClientGenerator();
        _reactPageGenerator = new ReactPageGenerator();

        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Generate C# files for all YAML entities in the specified directory.
    /// </summary>
    public void GenerateAll(
        string entitiesDirectory,
        string targetNamespace = "XekuII.Generated",
        bool generateControllers = false,
        string? controllersOutputDir = null,
        bool generateFrontend = false,
        string? frontendOutputDir = null)
    {
        Console.WriteLine($"🔍 Scanning entities from: {entitiesDirectory}");
        Console.WriteLine($"📁 Output directory: {_outputDirectory}");
        if (generateFrontend && frontendOutputDir != null)
            Console.WriteLine($"🌐 Frontend output: {frontendOutputDir}");
        Console.WriteLine();

        Directory.CreateDirectory(_outputDirectory);

        // Parse all entities first
        var entities = _parser.ParseDirectory(entitiesDirectory).ToList();

        // Build entity map for cross-entity lookups (detail relations need target entity fields)
        var entityMap = entities.ToDictionary(e => e.Entity, StringComparer.OrdinalIgnoreCase);

        // Analyze reverse associations
        var (reverseAssociations, associationOverrides) = AnalyzeRelationships(entities);

        var count = 0;

        foreach (var entity in entities)
        {
            // Get reverse associations for this entity
            reverseAssociations.TryGetValue(entity.Entity, out var reverseList);
            // Get overrides
            associationOverrides.TryGetValue(entity.Entity, out var overrides);

            var code = _codeGenerator.Generate(entity, targetNamespace, reverseList, overrides);
            var outputPath = Path.Combine(_outputDirectory, $"{entity.Entity}.Generated.cs");

            File.WriteAllText(outputPath, code);
            Console.WriteLine($"✅Generated BO: {entity.Entity}.Generated.cs");

            // Generate controller if requested
            if (generateControllers && controllersOutputDir != null)
            {
                Directory.CreateDirectory(controllersOutputDir);
                var controllerCode = _controllerGenerator.Generate(entity, targetNamespace, entityMap: entityMap);
                var controllerPath = Path.Combine(controllersOutputDir, $"{Pluralize(entity.Entity)}Controller.Generated.cs");
                File.WriteAllText(controllerPath, controllerCode);
                Console.WriteLine($"✅Generated API: {Pluralize(entity.Entity)}Controller.Generated.cs");
            }

            // Generate frontend code if requested
            if (generateFrontend && frontendOutputDir != null)
            {
                GenerateFrontendForEntity(entity, frontendOutputDir, entityMap);
            }

            count++;
        }

        // Generate cross-entity frontend files (routes, navigation)
        if (generateFrontend && frontendOutputDir != null && entities.Count > 0)
        {
            var routesCode = _reactPageGenerator.GenerateRoutes(entities);
            File.WriteAllText(Path.Combine(frontendOutputDir, "routes.generated.tsx"), routesCode);
            Console.WriteLine("✅Generated: routes.generated.tsx");

            var navCode = _reactPageGenerator.GenerateNavigation(entities);
            File.WriteAllText(Path.Combine(frontendOutputDir, "navigation.generated.ts"), navCode);
            Console.WriteLine("✅Generated: navigation.generated.ts");
        }

        Console.WriteLine();
        Console.WriteLine($"📊 Total: {count} entities generated.");
    }

    private void GenerateFrontendForEntity(EntityDefinition entity, string frontendOutputDir, Dictionary<string, EntityDefinition> entityMap)
    {
        var kebabName = ToKebabCase(entity.Entity);

        // Types
        var typesDir = Path.Combine(frontendOutputDir, "types");
        Directory.CreateDirectory(typesDir);
        var tsCode = _tsTypeGenerator.Generate(entity, entityMap);
        File.WriteAllText(Path.Combine(typesDir, $"{kebabName}.types.generated.ts"), tsCode);
        Console.WriteLine($"  🌐 types/{kebabName}.types.generated.ts");

        // Schemas
        var schemasDir = Path.Combine(frontendOutputDir, "schemas");
        Directory.CreateDirectory(schemasDir);
        var zodCode = _zodSchemaGenerator.Generate(entity, entityMap);
        File.WriteAllText(Path.Combine(schemasDir, $"{kebabName}.schema.generated.ts"), zodCode);
        Console.WriteLine($"  🌐 schemas/{kebabName}.schema.generated.ts");

        // API client
        var apiDir = Path.Combine(frontendOutputDir, "api");
        Directory.CreateDirectory(apiDir);
        var apiCode = _apiClientGenerator.Generate(entity, entityMap);
        File.WriteAllText(Path.Combine(apiDir, $"{kebabName}.api.generated.ts"), apiCode);
        Console.WriteLine($"  🌐 api/{kebabName}.api.generated.ts");

        // Pages
        var pagesDir = Path.Combine(frontendOutputDir, "pages", kebabName);
        Directory.CreateDirectory(pagesDir);

        var listCode = _reactPageGenerator.GenerateListPage(entity);
        File.WriteAllText(Path.Combine(pagesDir, $"{entity.Entity}ListPage.generated.tsx"), listCode);
        Console.WriteLine($"  🌐 pages/{kebabName}/{entity.Entity}ListPage.generated.tsx");

        var formCode = _reactPageGenerator.GenerateFormPage(entity);
        File.WriteAllText(Path.Combine(pagesDir, $"{entity.Entity}FormPage.generated.tsx"), formCode);
        Console.WriteLine($"  🌐 pages/{kebabName}/{entity.Entity}FormPage.generated.tsx");

        var detailCode = _reactPageGenerator.GenerateDetailPage(entity, entityMap);
        File.WriteAllText(Path.Combine(pagesDir, $"{entity.Entity}DetailPage.generated.tsx"), detailCode);
        Console.WriteLine($"  🌐 pages/{kebabName}/{entity.Entity}DetailPage.generated.tsx");
    }

    private static string ToKebabCase(string name)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
                result.Append('-');
            result.Append(char.ToLower(name[i]));
        }
        return result.ToString();
    }

    private string Pluralize(string name)
    {
        if (name.EndsWith("y") && !name.EndsWith("ay") && !name.EndsWith("ey") && !name.EndsWith("oy") && !name.EndsWith("uy"))
            return name.Substring(0, name.Length - 1) + "ies";
        if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("ch") || name.EndsWith("sh"))
            return name + "es";
        return name + "s";
    }

    /// <summary>
    /// Analyze all entities to find relationship pairs and determine reverse associations.
    /// Returns:
    /// 1. ReverseAssociationInfo list for each entity (missing properties to generate)
    /// 2. AssociationName overrides for explicit properties (to ensure pairs match)
    /// </summary>
    private (Dictionary<string, List<ReverseAssociationInfo>>, Dictionary<string, Dictionary<string, string>>) AnalyzeRelationships(List<EntityDefinition> entities)
    {
        var reverseMap = new Dictionary<string, List<ReverseAssociationInfo>>();
        var associationOverrides = new Dictionary<string, Dictionary<string, string>>();

        var entityMap = entities.ToDictionary(e => e.Entity, StringComparer.OrdinalIgnoreCase);

        foreach (var entity in entities)
        {
            foreach (var relation in entity.Relations)
            {
                // Skip if target entity is unknown
                if (!entityMap.TryGetValue(relation.Target, out var targetEntityDef))
                    continue;

                // 1. Check if the target has a matching reverse relation explicitly defined
                /*
                 * Logic for matching:
                 * If Current is Reference (Many-To-One) -> Look for Detail (One-To-Many) in Target that points back to Current
                 * If Current is Detail (One-To-Many) -> Look for Reference (Many-To-One) in Target that points back to Current
                 */

                RelationDefinition? matchingReverse = null;
                if (relation.Type == "reference")
                {
                    matchingReverse = targetEntityDef.Relations.FirstOrDefault(r =>
                        r.Type == "detail" && r.Target == entity.Entity);
                }
                else if (relation.Type == "detail")
                {
                    matchingReverse = targetEntityDef.Relations.FirstOrDefault(r =>
                        r.Type == "reference" && r.Target == entity.Entity);
                }

                if (matchingReverse != null)
                {
                    // PAIR FOUND! 
                    // We must ensure they share the same Association Name.
                    // Convention: Use the name from the Detail side usually, or just "{OneSideEntity}-{ManySideRelationName}"
                    // Let's stick to the Generator's default convention: "{EntityWithDetail}-{DetailPropertyName}"

                    string commonAssociationName;

                    if (relation.Type == "detail")
                    {
                        // I am the Detail side (The "One" side of the relation, holding the collection)
                        // My default association is "{MyName}-{MyRelationName}"
                        commonAssociationName = $"{entity.Entity}-{relation.Name}";

                        // I don't need an override, my default logic works. 
                        // But the OTHER side (Reference) needs to know this name.
                    }
                    else
                    {
                        // I am the Reference side.
                        // I need to match the Detail side's name.
                        // The matching reverse is the Detail side.
                        commonAssociationName = $"{targetEntityDef.Entity}-{matchingReverse.Name}";

                        // I need an override!
                        if (!associationOverrides.ContainsKey(entity.Entity))
                            associationOverrides[entity.Entity] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        associationOverrides[entity.Entity][relation.Name] = commonAssociationName;
                    }

                    // Since a matching explicit relation exists, we DO NOT generate a reverse association.
                    continue;
                }

                // NO MATCH FOUND - Generate Implicit Reverse Association
                // The association name will be based on THIS side since it's the only definer.
                var associationName = $"{entity.Entity}-{relation.Name}";

                if (relation.Type == "reference")
                {
                    // Many-to-One: Target needs a One-to-Many reverse collection
                    if (!reverseMap.ContainsKey(relation.Target))
                        reverseMap[relation.Target] = new List<ReverseAssociationInfo>();

                    reverseMap[relation.Target].Add(new ReverseAssociationInfo
                    {
                        TargetEntity = relation.Target,
                        PropertyName = Pluralize(entity.Entity), // Use pluralizer
                        SourceEntity = entity.Entity,
                        AssociationName = associationName,
                        IsCollection = true
                    });
                }
                else if (relation.Type == "detail")
                {
                    // One-to-Many: Target needs a Many-to-One reverse reference
                    if (!reverseMap.ContainsKey(relation.Target))
                        reverseMap[relation.Target] = new List<ReverseAssociationInfo>();

                    // Check if property name already exists to avoid conflict? 
                    // The CSharpGenerator handles checking if property exists, but better to be safe.
                    // Default name is the Entity name.

                    reverseMap[relation.Target].Add(new ReverseAssociationInfo
                    {
                        TargetEntity = relation.Target,
                        PropertyName = entity.Entity,
                        SourceEntity = entity.Entity,
                        AssociationName = associationName,
                        IsCollection = false
                    });
                }
            }
        }

        return (reverseMap, associationOverrides);
    }

    /// <summary>
    /// Generate C# for a single YAML file.
    /// </summary>
    public string GenerateSingle(string yamlFilePath, string targetNamespace = "XekuII.Generated")
    {
        var entity = _parser.ParseFile(yamlFilePath);
        var code = _codeGenerator.Generate(entity, targetNamespace);

        var outputPath = Path.Combine(_outputDirectory, $"{entity.Entity}.Generated.cs");
        File.WriteAllText(outputPath, code);

        return outputPath;
    }
}
