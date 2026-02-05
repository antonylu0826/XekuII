using XekuII.Generator.Models;

namespace XekuII.Generator.Analyzers;

/// <summary>
/// Analyzes all entities to detect and generate reverse associations.
/// </summary>
public class AssociationAnalyzer
{
    /// <summary>
    /// Represents a reverse association that needs to be added to an entity.
    /// </summary>
    public class ReverseAssociation
    {
        public string TargetEntity { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string SourceEntity { get; set; } = string.Empty;
        public string AssociationName { get; set; } = string.Empty;
        public bool IsCollection { get; set; } // true for One-to-Many reverse
    }

    /// <summary>
    /// Analyze all entities and find reverse associations that need to be generated.
    /// </summary>
    public Dictionary<string, List<ReverseAssociation>> AnalyzeReverseAssociations(IEnumerable<EntityDefinition> entities)
    {
        var result = new Dictionary<string, List<ReverseAssociation>>();
        var entityList = entities.ToList();
        var entityNames = entityList.Select(e => e.Entity).ToHashSet();

        foreach (var entity in entityList)
        {
            foreach (var relation in entity.Relations)
            {
                // Skip if target entity is not in our list
                if (!entityNames.Contains(relation.Target))
                    continue;

                var associationName = $"{entity.Entity}-{relation.Name}";

                if (relation.Type == "reference")
                {
                    // Many-to-One: Target needs a One-to-Many reverse
                    // e.g., Order.Customer (reference) ??Customer needs Orders collection
                    if (!result.ContainsKey(relation.Target))
                        result[relation.Target] = new List<ReverseAssociation>();

                    result[relation.Target].Add(new ReverseAssociation
                    {
                        TargetEntity = relation.Target,
                        PropertyName = entity.Entity + "s", // Pluralize
                        SourceEntity = entity.Entity,
                        AssociationName = associationName,
                        IsCollection = true
                    });
                }
                else if (relation.Type == "detail")
                {
                    // One-to-Many: Target (detail item) needs a Many-to-One reverse
                    // e.g., Order.Items (detail) ??OrderItem needs Order reference
                    if (!result.ContainsKey(relation.Target))
                        result[relation.Target] = new List<ReverseAssociation>();

                    result[relation.Target].Add(new ReverseAssociation
                    {
                        TargetEntity = relation.Target,
                        PropertyName = entity.Entity, // Singular
                        SourceEntity = entity.Entity,
                        AssociationName = associationName,
                        IsCollection = false
                    });
                }
            }
        }

        return result;
    }
}
