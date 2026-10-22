using System.Security.Cryptography;
using System.Text;

namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>A piece of one page that is embedded and retrieved as a unit.</summary>
/// <param name="Section">Heading path, for example "1 Safety > 1.2 Safety devices". Empty before the first heading.</param>
/// <param name="Index">Position of the chunk within the document.</param>
public sealed record DocumentChunk(string DocumentId, string DocumentTitle, int PageNumber, string Section, int Index, string Text)
{
    /// <summary>Same document, page and position always give the same id, so re-ingestion overwrites instead of duplicating.</summary>
    public Guid Id { get; } = CreateId(DocumentId, PageNumber, Index);

    /// <summary>
    /// Text sent to the embedding model. The title and section are prepended because a chunk such as
    /// "E214 | Oil temperature high" means little without knowing which machine and chapter it belongs to.
    /// </summary>
    public string EmbeddingText => Section.Length == 0
        ? $"{DocumentTitle}\n\n{Text}"
        : $"{DocumentTitle}\n{Section}\n\n{Text}";

    private static Guid CreateId(string documentId, int pageNumber, int index)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{documentId}|{pageNumber}|{index}"));
        return new Guid(hash.AsSpan(0, 16));
    }
}
