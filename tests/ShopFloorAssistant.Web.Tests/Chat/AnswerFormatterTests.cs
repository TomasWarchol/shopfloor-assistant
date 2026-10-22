using ShopFloorAssistant.Web.Chat;

namespace ShopFloorAssistant.Web.Tests.Chat;

public class AnswerFormatterTests
{
    [Fact]
    public void ToHtml_RendersMarkdownListsAndMarksCitations()
    {
        var html = AnswerFormatter.ToHtml("1. Lower the slide [1]\n2. Tighten to 180 Nm [2, 3]");

        Assert.Contains("<ol>", html, StringComparison.Ordinal);
        Assert.Contains("<sup class=\"cite\">[1]</sup>", html, StringComparison.Ordinal);
        Assert.Contains("<sup class=\"cite\">[2, 3]</sup>", html, StringComparison.Ordinal);
    }

    // A document can contain text that the model repeats in its answer.
    [Fact]
    public void ToHtml_EscapesRawHtml()
    {
        var html = AnswerFormatter.ToHtml("Done. <script>alert('x')</script><b onclick=\"steal()\">bold</b>");

        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<b ", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[open](javascript:alert(1))")]
    [InlineData("[open](data:text/html,payload)")]
    [InlineData("[open](vbscript:x)")]
    public void ToHtml_NeutralizesLinksThatAreNotWebAddresses(string markdown)
    {
        var html = AnswerFormatter.ToHtml(markdown);

        Assert.Contains("href=\"#\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(":alert", html, StringComparison.Ordinal);
    }

    [Fact]
    public void ToHtml_KeepsWebLinks()
    {
        Assert.Contains("href=\"https://example.com/manual\"", AnswerFormatter.ToHtml("[manual](https://example.com/manual)"), StringComparison.Ordinal);
    }

    [Fact]
    public void ToHtml_DropsImages()
    {
        Assert.DoesNotContain("<img", AnswerFormatter.ToHtml("![pixel](https://tracker.example/pixel.png)"), StringComparison.OrdinalIgnoreCase);
    }
}
