using XekuII.Generator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XekuII.Generator.Parsers;

/// <summary>
/// Parses .XekuII.yaml files into EntityDefinition objects.
/// </summary>
public class YamlEntityParser
{
    private readonly IDeserializer _deserializer;

    public YamlEntityParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Parse a single YAML file.
    /// </summary>
    public EntityDefinition Parse(string yamlContent)
    {
        return _deserializer.Deserialize<EntityDefinition>(yamlContent);
    }

    /// <summary>
    /// Parse a YAML file from path.
    /// </summary>
    public EntityDefinition ParseFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        return Parse(content);
    }

    /// <summary>
    /// Parse all .XekuII.yaml files in a directory.
    /// </summary>
    public IEnumerable<EntityDefinition> ParseDirectory(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.xeku.yaml", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            yield return ParseFile(file);
        }
    }
}
