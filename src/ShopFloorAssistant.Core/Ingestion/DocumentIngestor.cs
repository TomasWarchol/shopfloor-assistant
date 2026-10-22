using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using ShopFloorAssistant.Core.AI;
using ShopFloorAssistant.Core.Storage;

namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>Reads PDFs, splits them into chunks, embeds the chunks and stores them in the vector store.</summary>
public sealed partial class DocumentIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    IOptions<IngestionOptions> options,
    IOptions<VectorStoreOptions> vectorStoreOptions,
    IOptions<AiOptions> aiOptions,
    ILogger<DocumentIngestor> logger)
{
    private readonly IngestionOptions _options = options.Value;
    private readonly DocumentChunker _chunker = new(options.Value.MaxChunkLength);

    /// <summary>
    /// One collection per embedding model: vectors from different models are not comparable,
    /// so switching the model starts a fresh index instead of mixing them.
    /// </summary>
    public string CollectionName { get; } = CollectionNames.ForEmbeddingModel(aiOptions.Value.Embeddings.Model);

    public async Task<IngestionSummary> IngestDirectoryAsync(string directory, CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(directory, "*.pdf").Order(StringComparer.Ordinal).ToList();
        var collection = vectorStore.GetCollection<Guid, DocumentChunkRecord>(
            CollectionName,
            DocumentChunkRecord.CreateDefinition(_options.EmbeddingDimensions, vectorStoreOptions.Value.DistanceFunction));
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var totalChunks = 0;
        foreach (var file in files)
        {
            var document = PdfDocumentReader.Read(file);
            var chunks = _chunker.Chunk(document);
            await ReplaceDocumentAsync(collection, document.Id, chunks, cancellationToken);

            LogDocumentIngested(document.Id, document.Pages.Count, chunks.Count);
            totalChunks += chunks.Count;
        }

        return new IngestionSummary(CollectionName, files.Count, totalChunks);
    }

    // Deleting the document's old chunks first keeps the index correct when a new revision has fewer chunks.
    private async Task ReplaceDocumentAsync(
        VectorStoreCollection<Guid, DocumentChunkRecord> collection,
        string documentId,
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken)
    {
        var records = new List<DocumentChunkRecord>(chunks.Count);
        foreach (var batch in chunks.Chunk(_options.EmbeddingBatchSize))
        {
            var embeddings = await embeddingGenerator.GenerateAsync(
                batch.Select(chunk => chunk.EmbeddingText), cancellationToken: cancellationToken);
            records.AddRange(batch.Zip(embeddings, ToRecord));
        }

        var oldKeys = await collection
            .GetAsync(record => record.DocumentId == documentId, top: 10_000, cancellationToken: cancellationToken)
            .Select(record => record.Key)
            .ToListAsync(cancellationToken);
        if (oldKeys.Count > 0)
        {
            await collection.DeleteAsync(oldKeys, cancellationToken);
        }

        await collection.UpsertAsync(records, cancellationToken);
    }

    private DocumentChunkRecord ToRecord(DocumentChunk chunk, Embedding<float> embedding)
    {
        if (embedding.Vector.Length != _options.EmbeddingDimensions)
        {
            throw new InvalidOperationException(
                $"The embedding model returned {embedding.Vector.Length} dimensions, but Ingestion:EmbeddingDimensions " +
                $"is {_options.EmbeddingDimensions}. Set it to match the model in AI:Embeddings:Model.");
        }

        return new DocumentChunkRecord
        {
            Key = chunk.Id,
            DocumentId = chunk.DocumentId,
            DocumentTitle = chunk.DocumentTitle,
            PageNumber = chunk.PageNumber,
            Section = chunk.Section,
            Text = chunk.Text,
            Embedding = embedding.Vector,
        };
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Ingested {DocumentId}: {PageCount} pages, {ChunkCount} chunks")]
    private partial void LogDocumentIngested(string documentId, int pageCount, int chunkCount);
}

public sealed record IngestionSummary(string CollectionName, int DocumentCount, int ChunkCount);
