using System.Text.RegularExpressions;

namespace ShopFloorAssistant.Core.Ingestion;

public static partial class CollectionNames
{
    /// <summary>"bge-m3" gives "chunks-bge-m3", "openai/text-embedding-3-small" gives "chunks-openai-text-embedding-3-small".</summary>
    public static string ForEmbeddingModel(string model) =>
        "chunks-" + UnsafeCharacters().Replace(model.ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex UnsafeCharacters();
}
