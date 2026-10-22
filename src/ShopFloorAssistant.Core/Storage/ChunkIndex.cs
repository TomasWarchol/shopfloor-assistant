using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ShopFloorAssistant.Core.AI;
using ShopFloorAssistant.Core.Ingestion;

namespace ShopFloorAssistant.Core.Storage;

/// <summary>
/// Opens the chunk collection for the configured embedding model and vector store, so the ingestion
/// worker and the search always agree on the collection name and schema.
/// </summary>
public sealed class ChunkIndex(
    VectorStore vectorStore,
    IOptions<VectorStoreOptions> vectorStoreOptions,
    IOptions<IngestionOptions> ingestionOptions,
    IOptions<AiOptions> aiOptions)
{
    /// <summary>
    /// One collection per embedding model: vectors from different models are not comparable,
    /// so switching the model starts a fresh index instead of mixing them.
    /// </summary>
    public string Name { get; } = CollectionNames.ForEmbeddingModel(aiOptions.Value.Embeddings.Model);

    public VectorStoreCollection<Guid, DocumentChunkRecord> Get() =>
        vectorStore.GetCollection<Guid, DocumentChunkRecord>(
            Name,
            DocumentChunkRecord.CreateDefinition(ingestionOptions.Value.EmbeddingDimensions, vectorStoreOptions.Value.DistanceFunction));
}
