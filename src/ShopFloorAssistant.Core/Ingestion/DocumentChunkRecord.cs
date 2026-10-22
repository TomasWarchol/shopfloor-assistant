using Microsoft.Extensions.VectorData;

namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>How a chunk is stored in the vector database.</summary>
public sealed class DocumentChunkRecord
{
    public Guid Key { get; set; }

    public string DocumentId { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public int PageNumber { get; set; }

    public string Section { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public ReadOnlyMemory<float> Embedding { get; set; }

    /// <summary>
    /// The schema is built at runtime instead of with attributes, because the vector size depends on
    /// the embedding model (1024 for bge-m3, 1536 for text-embedding-3-small) and the distance
    /// function on the vector store.
    /// </summary>
    public static VectorStoreCollectionDefinition CreateDefinition(int dimensions, string distanceFunction) => new()
    {
        Properties =
        [
            new VectorStoreKeyProperty(nameof(Key), typeof(Guid)),
            new VectorStoreDataProperty(nameof(DocumentId), typeof(string)) { IsIndexed = true },
            new VectorStoreDataProperty(nameof(DocumentTitle), typeof(string)),
            new VectorStoreDataProperty(nameof(PageNumber), typeof(int)),
            new VectorStoreDataProperty(nameof(Section), typeof(string)),
            new VectorStoreDataProperty(nameof(Text), typeof(string)) { IsFullTextIndexed = true },
            new VectorStoreVectorProperty(nameof(Embedding), typeof(ReadOnlyMemory<float>), dimensions)
            {
                DistanceFunction = distanceFunction,
            },
        ],
    };
}
