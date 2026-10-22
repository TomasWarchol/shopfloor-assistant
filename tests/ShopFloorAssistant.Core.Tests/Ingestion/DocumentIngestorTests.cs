using CommunityToolkit.VectorData.InMemory;
using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Extensions.VectorData;
using ShopFloorAssistant.Core.Ingestion;
using ShopFloorAssistant.Core.Tests.TestDoubles;

namespace ShopFloorAssistant.Core.Tests.Ingestion;

public class DocumentIngestorTests
{
    [Fact]
    public async Task IngestDirectoryAsync_StoresEveryChunkWithDocumentAndPage()
    {
        using var store = new InMemoryVectorStore();

        var summary = await TestIndex.Ingestor(store).IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
        var records = await ReadAllAsync(store);

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
        var ingestor = TestIndex.Ingestor(store);

        var first = await ingestor.IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
        await ingestor.IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);

        Assert.Equal(first.ChunkCount, (await ReadAllAsync(store)).Count);
    }

    [Fact]
    public async Task IngestDirectoryAsync_EmbeddingSizeMismatch_FailsWithHint()
    {
        using var store = new InMemoryVectorStore();
        var ingestor = TestIndex.Ingestor(store, configuredDimensions: 1024);

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
                summary = await TestIndex.Ingestor(store).IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
            }

            using var reopened = new SqliteVectorStore($"Data Source={databasePath};Pooling=False");
            var hits = await TestIndex.Chunks(reopened).Get()
                .SearchAsync(FakeEmbeddingGenerator.Vector("query", TestIndex.Dimensions), top: 3, cancellationToken: TestContext.Current.CancellationToken)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(summary.ChunkCount, (await ReadAllAsync(reopened)).Count);
            Assert.Equal(3, hits.Count);
            Assert.All(hits, hit => Assert.True(hit.Record.PageNumber > 0));
        }
        finally
        {
            File.Delete(databasePath);
        }
    }

    private static async Task<List<DocumentChunkRecord>> ReadAllAsync(VectorStore store) =>
        await TestIndex.Chunks(store).Get()
            .GetAsync(record => record.PageNumber > 0, top: 10_000, cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);
}
