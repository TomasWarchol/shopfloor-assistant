using ShopFloorAssistant.Core.Ingestion;

namespace ShopFloorAssistant.Core.Tests.Ingestion;

public class DocumentChunkerTests
{
    [Fact]
    public void Chunk_StartsNewChunkAtEachHeadingAndTracksTheSectionPath()
    {
        var document = Document(Page(1, "1 Safety", "General rules.", "1.1 PPE", "Wear safety glasses.", "2 Technical data", "Force 2500 kN."));

        var chunks = new DocumentChunker().Chunk(document);

        Assert.Collection(
            chunks,
            chunk => Assert.Equal(("1 Safety", "General rules."), (chunk.Section, chunk.Text)),
            chunk => Assert.Equal(("1 Safety > 1.1 PPE", "Wear safety glasses."), (chunk.Section, chunk.Text)),
            chunk => Assert.Equal(("2 Technical data", "Force 2500 kN."), (chunk.Section, chunk.Text)));
    }

    [Fact]
    public void Chunk_NeverCrossesAPageButCarriesTheSectionOver()
    {
        var document = Document(
            Page(1, "8 Hydraulic oil change", "1. Drain the oil."),
            Page(2, "2. Refill the oil."));

        var chunks = new DocumentChunker().Chunk(document);

        Assert.Equal([1, 2], chunks.Select(chunk => chunk.PageNumber));
        Assert.All(chunks, chunk => Assert.Equal("8 Hydraulic oil change", chunk.Section));
    }

    [Fact]
    public void Chunk_SplitsLongContentAtSentencesBelowTheMaximumLength()
    {
        var paragraph = string.Join(' ', Enumerable.Range(1, 40).Select(i => $"Sentence number {i} about the press."));
        var document = Document(Page(1, paragraph, "Another paragraph."));

        var chunks = new DocumentChunker(maxChunkLength: 200).Chunk(document);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, chunk => Assert.InRange(chunk.Text.Length, 1, 200));
        Assert.Equal($"{paragraph} Another paragraph.", string.Join(' ', chunks.Select(chunk => chunk.Text.Replace('\n', ' '))));
    }

    [Fact]
    public void Chunk_GivesStableUniqueIdsAndAddsContextToTheEmbeddingText()
    {
        var document = Document(Page(1, "1 Safety", "General rules.", "2 Data", "Force 2500 kN."));
        var chunker = new DocumentChunker();

        var first = chunker.Chunk(document);
        var second = chunker.Chunk(document);

        Assert.Equal(first.Select(chunk => chunk.Id), second.Select(chunk => chunk.Id));
        Assert.Equal(first.Count, first.Select(chunk => chunk.Id).Distinct().Count());
        Assert.Equal("Press manual\n1 Safety\n\nGeneral rules.", first[0].EmbeddingText);
    }

    [Theory]
    [InlineData("4.3 Shutdown", 2)]
    [InlineData("10 Contacts", 1)]
    [InlineData("2 Częstotliwość kontroli", 1)]
    public void HeadingLevel_RecognizesNumberedHeadings(string block, int expectedLevel)
    {
        Assert.Equal(expectedLevel, DocumentChunker.HeadingLevel(block));
    }

    [Theory]
    [InlineData("6 bar")]
    [InlineData("1. Check the oil level.")]
    [InlineData("12 | Value")]
    [InlineData("E214 | Oil temperature high")]
    [InlineData("Setup mode is used only for die change.")]
    public void HeadingLevel_IgnoresValuesListItemsAndTableRows(string block)
    {
        Assert.Null(DocumentChunker.HeadingLevel(block));
    }

    [Fact]
    public void Chunk_SampleManual_PutsTheAlarmTableInItsSectionAndPage()
    {
        var manual = PdfDocumentReader.Read(SampleDocuments.Get("HP-250-operation-and-maintenance-manual.pdf"));

        var chunks = new DocumentChunker().Chunk(manual);

        var alarm = Assert.Single(chunks, chunk => chunk.Text.Contains("E214 |", StringComparison.Ordinal));
        Assert.Equal(4, alarm.PageNumber);
        Assert.Equal("9 Alarms and troubleshooting", alarm.Section);
    }

    private static SourceDocument Document(params DocumentPage[] pages) => new("press-manual", "Press manual", pages);

    private static DocumentPage Page(int number, params string[] blocks) => new(number, blocks);
}
