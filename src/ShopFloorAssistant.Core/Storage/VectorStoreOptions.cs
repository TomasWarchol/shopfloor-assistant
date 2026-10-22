namespace ShopFloorAssistant.Core.Storage;

public enum VectorStoreProvider
{
    /// <summary>A local file with the sqlite-vec extension. No Docker needed.</summary>
    Sqlite,

    /// <summary>Qdrant, run by Aspire as a container. Needs Docker.</summary>
    Qdrant,
}

/// <summary>Configuration bound from the <c>VectorStore</c> section.</summary>
public sealed class VectorStoreOptions
{
    public const string SectionName = "VectorStore";

    public VectorStoreProvider Provider { get; set; } = VectorStoreProvider.Sqlite;

    /// <summary>Database file used by the SQLite provider.</summary>
    public string SqlitePath { get; set; } = Path.Combine(".data", "shopfloor-assistant.db");

    /// <summary>
    /// Both measure the angle between vectors; the providers support different variants.
    /// Similarity: higher is closer. Distance: lower is closer.
    /// </summary>
    public string DistanceFunction => Provider == VectorStoreProvider.Qdrant
        ? Microsoft.Extensions.VectorData.DistanceFunction.CosineSimilarity
        : Microsoft.Extensions.VectorData.DistanceFunction.CosineDistance;
}
