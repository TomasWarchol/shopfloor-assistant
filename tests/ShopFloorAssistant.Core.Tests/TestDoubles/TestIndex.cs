using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ShopFloorAssistant.Core.AI;
using ShopFloorAssistant.Core.Ingestion;
using ShopFloorAssistant.Core.Storage;

namespace ShopFloorAssistant.Core.Tests.TestDoubles;

/// <summary>Builds the ingestion and search objects the way dependency injection would.</summary>
internal static class TestIndex
{
    public const int Dimensions = 8;

    public static ChunkIndex Chunks(VectorStore store, int dimensions = Dimensions) => new(
        store,
        Options.Create(new VectorStoreOptions()),
        Options.Create(new IngestionOptions { EmbeddingDimensions = dimensions }),
        Options.Create(new AiOptions()));

    public static DocumentIngestor Ingestor(
        VectorStore store,
        IEmbeddingGenerator<string, Embedding<float>>? embeddings = null,
        int configuredDimensions = Dimensions) => new(
        embeddings ?? new FakeEmbeddingGenerator(Dimensions),
        Chunks(store, configuredDimensions),
        Options.Create(new IngestionOptions { EmbeddingDimensions = configuredDimensions }),
        NullLogger<DocumentIngestor>.Instance);
}
