using ShopFloorAssistant.Core.Ingestion;

namespace ShopFloorAssistant.Core.Tests.Ingestion;

public class PdfDocumentReaderTests
{
    private static readonly SourceDocument Manual =
        PdfDocumentReader.Read(SampleDocuments.Get("HP-250-operation-and-maintenance-manual.pdf"));

    [Fact]
    public void Read_Manual_UsesPdfTitleAndKeepsAllPages()
    {
        Assert.Equal("HP-250-operation-and-maintenance-manual", Manual.Id);
        Assert.Equal("HP-250 Hydraulic Press – Operation and Maintenance Manual", Manual.Title);
        Assert.Equal([1, 2, 3, 4], Manual.Pages.Select(page => page.Number));
    }

    [Fact]
    public void Read_Manual_RemovesRunningHeadersAndFooters()
    {
        var blocks = Manual.Pages.SelectMany(page => page.Blocks).ToList();

        Assert.DoesNotContain(blocks, block => block.Contains("Plant 2", StringComparison.Ordinal));
        Assert.DoesNotContain(blocks, block => block.Contains("Synthetic document", StringComparison.Ordinal));
        Assert.DoesNotContain(blocks, block => block.StartsWith("Page ", StringComparison.Ordinal));
    }

    [Fact]
    public void Read_Manual_KeepsTableCellsOfOneRowTogether()
    {
        var alarmsPage = Manual.Pages[3].Blocks;

        Assert.Contains("Code | Message | Probable cause | Action", alarmsPage);
        Assert.Contains(alarmsPage, block => block.StartsWith(
            "E214 | Oil temperature high (above 60 °C) | Cooler fault, low oil level or clogged cooler | Stop production;",
            StringComparison.Ordinal));
    }

    [Fact]
    public void Read_Manual_JoinsListMarkersWithTheirText()
    {
        Assert.Contains("6. Bleed the system, then run 10 strokes in setup mode.", Manual.Pages[3].Blocks);
        Assert.Contains("• Two-hand control on the operator console for single stroke mode.", Manual.Pages[0].Blocks);
    }

    // Layout analysis merges some markers of this list into the text block ("9.", "10. Run ...").
    [Fact]
    public void Read_Procedure_KeepsEveryStepNumberWithItsWrappedText()
    {
        var procedure = PdfDocumentReader.Read(SampleDocuments.Get("SOP-014-die-change-line-P12.pdf"));
        var firstPage = procedure.Pages[0].Blocks;

        Assert.Contains(firstPage, block =>
            block.StartsWith("8. Clamp the upper and lower die.", StringComparison.Ordinal)
            && block.EndsWith("tighten them to 180 Nm in a cross pattern.", StringComparison.Ordinal));
        Assert.Contains("9. Enter the die shut height from the die tag on the HMI. Tolerance is ±0.1 mm.", firstPage);
        Assert.Contains("10. Run one single stroke without a blank and check that no alarm is shown.", firstPage);
    }

    [Fact]
    public void Read_PolishInstruction_KeepsDiacriticsAndSymbols()
    {
        var instruction = PdfDocumentReader.Read(SampleDocuments.Get("WI-203-kontrola-jakosci-wytloczek.pdf"));
        var text = string.Join('\n', instruction.Pages.SelectMany(page => page.Blocks));

        Assert.EndsWith("kontrola jakości wytłoczek", instruction.Title, StringComparison.Ordinal);
        Assert.Contains("Ø 8,2 mm (+0,1 / 0)", text, StringComparison.Ordinal);
        Assert.Contains("czerwoną etykietą „Blokada”", text, StringComparison.Ordinal);
    }
}
