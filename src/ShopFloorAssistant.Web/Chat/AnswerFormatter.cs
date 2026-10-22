using System.Text.RegularExpressions;
using Markdig;

namespace ShopFloorAssistant.Web.Chat;

/// <summary>
/// Renders the model's Markdown answer as HTML. The answer can repeat text from a document, so it is
/// treated as untrusted: raw HTML is escaped, links are limited to http(s) and images are dropped.
/// </summary>
public static partial class AnswerFormatter
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().DisableHtml().Build();

    public static string ToHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown, Pipeline);
        html = Image().Replace(html, string.Empty);
        html = UnsafeLink().Replace(html, "href=\"#\"");
        return Citation().Replace(html, "<sup class=\"cite\">$0</sup>");
    }

    [GeneratedRegex(@"<img\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex Image();

    [GeneratedRegex(@"href=""(?!https?://)[^""]*""", RegexOptions.IgnoreCase)]
    private static partial Regex UnsafeLink();

    [GeneratedRegex(@"\[\d{1,2}(?:\s*[,;]\s*\d{1,2})*\]")]
    private static partial Regex Citation();
}
