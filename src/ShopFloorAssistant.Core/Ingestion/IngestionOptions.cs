namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>Configuration bound from the <c>Ingestion</c> section.</summary>
public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Folder with the PDF files to ingest.</summary>
    public string DocumentsPath { get; set; } = "samples/documents";

    /// <summary>Must match the embedding model: 1024 for bge-m3.</summary>
    public int EmbeddingDimensions { get; set; } = 1024;

    public int MaxChunkLength { get; set; } = DocumentChunker.DefaultMaxChunkLength;

    /// <summary>Chunks sent to the embedding model in one request.</summary>
    public int EmbeddingBatchSize { get; set; } = 16;
}
