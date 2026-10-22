using System.Text;
using System.Text.RegularExpressions;

namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>
/// Splits a document into chunks that never cross a page boundary (so a citation is always one page)
/// and start a new chunk at every numbered heading (so a chunk stays on one topic).
/// </summary>
public sealed partial class DocumentChunker(int maxChunkLength = DocumentChunker.DefaultMaxChunkLength)
{
    /// <summary>
    /// About 125 tokens. Measured on the sample documents with bge-m3: 500 characters put the right chunk
    /// first for 7 of 8 questions (MRR 0.92), 1200 characters only for 4 of 8 (MRR 0.68), because one
    /// fact gets diluted in a long chunk. See docs/adr/0004-page-aware-chunking.md.
    /// </summary>
    public const int DefaultMaxChunkLength = 500;

    public IReadOnlyList<DocumentChunk> Chunk(SourceDocument document)
    {
        var chunks = new List<DocumentChunk>();
        var headings = new List<(int Level, string Text)>();
        var section = string.Empty;
        var text = new StringBuilder();

        foreach (var page in document.Pages)
        {
            foreach (var block in page.Blocks)
            {
                if (HeadingLevel(block) is { } level)
                {
                    Flush(page.Number);
                    headings.RemoveAll(heading => heading.Level >= level);
                    headings.Add((level, block));
                    section = string.Join(" > ", headings.Select(heading => heading.Text));
                    continue;
                }

                foreach (var piece in SplitToFit(block))
                {
                    if (text.Length > 0 && text.Length + 1 + piece.Length > maxChunkLength)
                    {
                        Flush(page.Number);
                    }

                    if (text.Length > 0)
                    {
                        text.Append('\n');
                    }

                    text.Append(piece);
                }
            }

            Flush(page.Number);
        }

        return chunks;

        void Flush(int pageNumber)
        {
            if (text.Length > 0)
            {
                chunks.Add(new DocumentChunk(document.Id, document.Title, pageNumber, section, chunks.Count, text.ToString()));
                text.Clear();
            }
        }
    }

    /// <summary>
    /// "4.3 Shutdown" is a level 2 heading. Values such as "6 bar" or list items such as "1. Check..."
    /// are not headings: a heading has a short section number followed by a capitalized title.
    /// </summary>
    internal static int? HeadingLevel(string block)
    {
        var match = Heading().Match(block);
        return match.Success ? match.Groups[1].Value.Count(c => c == '.') + 1 : null;
    }

    private IEnumerable<string> SplitToFit(string block)
    {
        if (block.Length <= maxChunkLength)
        {
            yield return block;
            yield break;
        }

        // Rare: one paragraph longer than a chunk. Split at sentence ends, and hard-split what is still too long.
        var piece = new StringBuilder();
        foreach (var sentence in SentenceEnd().Split(block))
        {
            if (piece.Length > 0 && piece.Length + 1 + sentence.Length > maxChunkLength)
            {
                yield return piece.ToString();
                piece.Clear();
            }

            if (piece.Length > 0)
            {
                piece.Append(' ');
            }

            piece.Append(sentence);
            while (piece.Length > maxChunkLength)
            {
                yield return piece.ToString(0, maxChunkLength);
                piece.Remove(0, maxChunkLength);
            }
        }

        if (piece.Length > 0)
        {
            yield return piece.ToString();
        }
    }

    [GeneratedRegex(@"^(\d{1,2}(?:\.\d{1,2})*)\s+\p{Lu}[^|]{0,80}$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceEnd();
}
