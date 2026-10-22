using System.Text.RegularExpressions;

namespace ShopFloorAssistant.SampleDocuments;

internal abstract record Block;

internal sealed record Heading(int Level, string Text) : Block;

internal sealed record Paragraph(string Text) : Block;

internal sealed record ListBlock(IReadOnlyList<(string Marker, string Text)> Items) : Block;

internal sealed record Table(IReadOnlyList<string> Header, IReadOnlyList<IReadOnlyList<string>> Rows) : Block;

/// <summary>
/// The small Markdown subset used by the sample sources: a title, a metadata line, headings,
/// paragraphs, bullet and numbered lists, and pipe tables.
/// </summary>
internal sealed partial record MarkdownDocument(string Title, string Metadata, string Code, bool IsPolish, IReadOnlyList<Block> Blocks)
{
    public static MarkdownDocument Parse(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var title = lines[0].TrimStart('#', ' ');
        var metadata = lines.Skip(1).First(line => line.Length > 0);
        var blocks = new List<Block>();

        var paragraph = new List<string>();
        var listItems = new List<(string, string)>();
        var tableRows = new List<IReadOnlyList<string>>();

        void Flush()
        {
            if (paragraph.Count > 0)
            {
                blocks.Add(new Paragraph(string.Join(' ', paragraph)));
                paragraph.Clear();
            }

            if (listItems.Count > 0)
            {
                blocks.Add(new ListBlock([.. listItems]));
                listItems.Clear();
            }

            if (tableRows.Count > 0)
            {
                blocks.Add(new Table(tableRows[0], [.. tableRows.Skip(1)]));
                tableRows.Clear();
            }
        }

        foreach (var line in lines.SkipWhile(line => line != metadata).Skip(1))
        {
            if (line.Length == 0)
            {
                Flush();
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal) || line.StartsWith("### ", StringComparison.Ordinal))
            {
                Flush();
                var level = line.IndexOf(' ', StringComparison.Ordinal);
                blocks.Add(new Heading(level, line[(level + 1)..]));
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                listItems.Add(("•", line[2..]));
            }
            else if (NumberedItem().Match(line) is { Success: true } item)
            {
                listItems.Add((item.Groups[1].Value, item.Groups[2].Value));
            }
            else if (line.StartsWith('|'))
            {
                if (!TableSeparator().IsMatch(line))
                {
                    tableRows.Add([.. line.Trim('|').Split('|').Select(cell => cell.Trim())]);
                }
            }
            else
            {
                paragraph.Add(line.Trim());
            }
        }

        Flush();

        var code = metadata.Split('|')[0].Split(':', 2)[1].Trim();
        var isPolish = metadata.StartsWith("Dokument:", StringComparison.Ordinal);
        return new MarkdownDocument(title, metadata, code, isPolish, blocks);
    }

    [GeneratedRegex(@"^(\d+\.)\s+(.*)$")]
    private static partial Regex NumberedItem();

    [GeneratedRegex(@"^\|[\s\-|]+\|$")]
    private static partial Regex TableSeparator();
}
