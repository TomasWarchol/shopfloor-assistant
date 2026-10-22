using Microsoft.Extensions.AI;
using ShopFloorAssistant.Core.Storage;

namespace ShopFloorAssistant.Core.Retrieval;

/// <summary>A chunk found for a question, with everything needed to cite it.</summary>
public sealed record RetrievedChunk(string DocumentId, string DocumentTitle, int PageNumber, string Section, string Text, double? Score);

/// <summary>Semantic search over the indexed chunks.</summary>
public sealed class DocumentSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ChunkIndex chunkIndex)
{
    /// <summary>Returns the closest chunks, or nothing when no document has been indexed yet.</summary>
    public async Task<IReadOnlyList<RetrievedChunk>> SearchAsync(string query, int top, CancellationToken cancellationToken = default)
    {
        var collection = chunkIndex.Get();
        if (!await collection.CollectionExistsAsync(cancellationToken))
        {
            return [];
        }

        var vector = await embeddingGenerator.GenerateVectorAsync(query, cancellationToken: cancellationToken);
        return await collection
            .SearchAsync(vector, top, cancellationToken: cancellationToken)
            .Select(hit => new RetrievedChunk(
                hit.Record.DocumentId,
                hit.Record.DocumentTitle,
                hit.Record.PageNumber,
                hit.Record.Section,
                hit.Record.Text,
                hit.Score))
            .ToListAsync(cancellationToken);
    }
}
