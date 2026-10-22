using CommunityToolkit.VectorData.InMemory;
using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ShopFloorAssistant.Core.AI;
using ShopFloorAssistant.Core.Ingestion;
using ShopFloorAssistant.Core.Storage;

namespace ShopFloorAssistant.Core.Tests.Ingestion;

public class DocumentIngestorTests
{
    private const int Dimensions = 8;

    [Fact]
    public async Task IngestDirectoryAsync_StoresEveryChunkWithDocumentAndPage()
    {
        using var store = new InMemoryVectorStore();
        var ingestor = CreateIngestor(store);

        var summary = await ingestor.IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
        var records = await ReadAllAsync(store, summary.CollectionName);

        Assert.Equal("chunks-bge-m3", summary.CollectionName);
        Assert.Equal(4, summary.DocumentCount);
        Assert.Equal(summary.ChunkCount, records.Count);
        Assert.Contains(records, record =>
            record.DocumentId == "SOP-014-die-change-line-P12"
            && record.Text.Contains("180 Nm", StringComparison.Ordinal)
            && record.Section == "5 Internal setup (press stopped)");
    }

    [Fact]
    public async Task IngestDirectoryAsync_RunTwice_ReplacesInsteadOfDuplicating()
    {
        using var store = new InMemoryVectorStore();
        var ingestor = CreateIngestor(store);

        var first = await ingestor.IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
        await ingestor.IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);

        Assert.Equal(first.ChunkCount, (await ReadAllAsync(store, first.CollectionName)).Count);
    }

    [Fact]
    public async Task IngestDirectoryAsync_EmbeddingSizeMismatch_FailsWithHint()
    {
        using var store = new InMemoryVectorStore();
        var ingestor = CreateIngestor(store, configuredDimensions: 1024);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ingestor.IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken));

        Assert.Contains("Ingestion:EmbeddingDimensions", exception.Message, StringComparison.Ordinal);
    }

    // The default local store: a real SQLite file, reopened to prove the index is persisted and searchable.
    [Fact]
    public async Task IngestDirectoryAsync_WithSqliteStore_PersistsSearchableChunks()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"shopfloor-{Guid.NewGuid():N}.db");
        try
        {
            IngestionSummary summary;
            using (var store = new SqliteVectorStore($"Data Source={databasePath};Pooling=False"))
            {
                summary = await CreateIngestor(store).IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
            }

            using var reopened = new SqliteVectorStore($"Data Source={databasePath};Pooling=False");
            var collection = reopened.GetCollection<Guid, DocumentChunkRecord>(
                summary.CollectionName, DocumentChunkRecord.CreateDefinition(Dimensions, DistanceFunction.CosineDistance));
            var hits = await collection
                .SearchAsync(FakeEmbeddingGenerator.Vector("query", Dimensions), top: 3, cancellationToken: TestContext.Current.CancellationToken)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(summary.ChunkCount, (await ReadAllAsync(reopened, summary.CollectionName)).Count);
            Assert.Equal(3, hits.Count);
            Assert.All(hits, hit => Assert.True(hit.Record.PageNumber > 0));
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    private static DocumentIngestor CreateIngestor(VectorStore store, int configuredDimensions = Dimensions) => new(
        new FakeEmbeddingGenerator(Dimensions),
        store,
        Options.Create(new IngestionOptions { EmbeddingDimensions = configuredDimensions }),
        Options.Create(new VectorStoreOptions()),
        Options.Create(new AiOptions()),
        NullLogger<DocumentIngestor>.Instance);

    private static async Task<List<DocumentChunkRecord>> ReadAllAsync(VectorStore store, string collectionName) =>
        await store.GetCollection<Guid, DocumentChunkRecord>(
                collectionName, DocumentChunkRecord.CreateDefinition(Dimensions, DistanceFunction.CosineDistance))
            .GetAsync(record => record.PageNumber > 0, top: 10_000, cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

    /// <summary>Deterministic vectors, so the pipeline can be tested without a model.</summary>
    private sealed class FakeEmbeddingGenerator(int dimensions) : IEmbeddingGenerator<string, Embedding<float>>
    {
        public static float[] Vector(string value, int dimensions) =>
            [.. Enumerable.Range(1, dimensions).Select(i => (float)((value.Length % 13) + i))];

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var embeddings = values.Select(value => new Embedding<float>(Vector(value, dimensions)));
            return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
