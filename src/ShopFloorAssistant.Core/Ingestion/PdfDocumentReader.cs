using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>
/// Extracts page text from a PDF with layout analysis, so tables keep their rows and
/// running headers and footers do not end up in the chunks.
/// </summary>
public static partial class PdfDocumentReader
{
    // Running headers and footers sit in the top and bottom 8 % of the page.
    private const double MarginBand = 0.08;

    // Blocks whose tops differ by less than this many points are on the same row.
    private const double SameRowTolerance = 2.0;

    public static SourceDocument Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream, Path.GetFileNameWithoutExtension(path));
    }

    public static SourceDocument Read(Stream pdf, string documentId)
    {
        using var document = PdfDocument.Open(pdf);
        var pages = document.GetPages().Select(ReadPage).ToList();
        var title = string.IsNullOrWhiteSpace(document.Information.Title) ? documentId : document.Information.Title.Trim();
        return new SourceDocument(documentId, title, pages);
    }

    private static DocumentPage ReadPage(Page page)
    {
        var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters);
        var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        var bodyBlocks = UnsupervisedReadingOrderDetector.Instance.Get(blocks)
            .Where(block => block.BoundingBox.Bottom > page.Height * MarginBand
                && block.BoundingBox.Top < page.Height * (1 - MarginBand))
            .ToList();

        // Layout analysis puts list markers ("1.", "•") in blocks of their own, sometimes one block
        // for a whole column of markers. They are matched back to their item text by height.
        var markers = bodyBlocks.Where(IsMarkerBlock).SelectMany(block => block.TextLines).ToList();
        var segments = bodyBlocks
            .Where(block => !IsMarkerBlock(block))
            .SelectMany(block => SplitListItems(block, markers));

        // Every table cell is a block too. Segments that start at the same height are one table row.
        var rows = new List<string>();
        var row = new List<(double Top, string Text)>();
        foreach (var segment in segments)
        {
            if (row.Count > 0 && Math.Abs(row[0].Top - segment.Top) > SameRowTolerance)
            {
                rows.Add(string.Join(" | ", row.Select(cell => cell.Text)));
                row.Clear();
            }

            row.Add(segment);
        }

        if (row.Count > 0)
        {
            rows.Add(string.Join(" | ", row.Select(cell => cell.Text)));
        }

        return new DocumentPage(page.Number, rows);
    }

    private static bool IsMarkerBlock(TextBlock block) =>
        block.TextLines.All(line => ListMarker().IsMatch(line.Text.Trim()));

    /// <summary>
    /// Splits a block into list items, so "8." stays attached to the text of step 8. A line starts a new
    /// item when a marker sits just left of it at the same height (compared by vertical center, because a
    /// "•" glyph is shorter than the text next to it) or when the line itself starts with a marker.
    /// Other lines continue the current item. A block without markers is returned as one segment.
    /// </summary>
    private static IEnumerable<(double Top, string Text)> SplitListItems(TextBlock block, List<TextLine> pageMarkers)
    {
        var markerLines = block.TextLines.Where(line => ListMarker().IsMatch(line.Text.Trim())).ToList();
        var markers = pageMarkers.Concat(markerLines).ToList();
        var textLines = block.TextLines.Except(markerLines).OrderByDescending(Center).ToList();

        var items = new List<(double Top, string Marker, List<string> Lines)>();
        foreach (var line in textLines)
        {
            var marker = markers.FirstOrDefault(marker =>
                Math.Abs(Center(marker) - Center(line)) <= SameRowTolerance * 2
                && marker.BoundingBox.Right <= line.BoundingBox.Left + 1
                && line.BoundingBox.Left - marker.BoundingBox.Right < 30);
            var inlineItem = InlineListItem().Match(line.Text.Trim());

            if (marker is not null)
            {
                items.Add((line.BoundingBox.Top, marker.Text.Trim(), [line.Text]));
            }
            else if (inlineItem.Success)
            {
                items.Add((line.BoundingBox.Top, inlineItem.Groups[1].Value, [inlineItem.Groups[2].Value]));
            }
            else if (items.Count > 0)
            {
                items[^1].Lines.Add(line.Text);
            }
            else
            {
                items.Add((line.BoundingBox.Top, string.Empty, [line.Text]));
            }
        }

        if (items.All(item => item.Marker.Length == 0))
        {
            yield return (block.BoundingBox.Top, Reflow(block.TextLines.Select(line => line.Text)));
            yield break;
        }

        foreach (var (top, marker, lines) in items)
        {
            yield return (top, marker.Length == 0 ? Reflow(lines) : $"{marker} {Reflow(lines)}");
        }
    }

    private static double Center(TextLine line) => (line.BoundingBox.Top + line.BoundingBox.Bottom) / 2;

    // Joins wrapped lines into one line. A line ending with a hyphen continues the same word.
    private static string Reflow(IEnumerable<string> lines)
    {
        var result = new StringBuilder();
        foreach (var line in lines.Select(line => line.Trim()).Where(line => line.Length > 0))
        {
            if (result.Length > 0 && result[^1] != '-')
            {
                result.Append(' ');
            }

            result.Append(line);
        }

        return result.ToString();
    }

    [GeneratedRegex(@"^(\d+\.|•)$")]
    private static partial Regex ListMarker();

    [GeneratedRegex(@"^(\d+\.|•)\s+(.+)$")]
    private static partial Regex InlineListItem();
}
