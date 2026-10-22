using ShopFloorAssistant.Core.Chat;
using ShopFloorAssistant.Core.Retrieval;

namespace ShopFloorAssistant.Core.Tests.Chat;

public class CitationsTests
{
    private static readonly Source[] Sources = [Source(1, "SOP-014"), Source(2, "HP-250"), Source(3, "WI-203")];

    [Fact]
    public void FindCited_ReturnsSourcesInOrderOfFirstMention()
    {
        var cited = Citations.FindCited("Tighten to 180 Nm [3]. Check the clamps [1]. Again [3].", Sources);

        Assert.Equal([3, 1], cited.Select(source => source.Number));
    }

    [Theory]
    [InlineData("Values [1][2].")]
    [InlineData("Values [1, 2].")]
    [InlineData("Values [1; 2].")]
    public void FindCited_AcceptsCommonMultiCitationForms(string answer)
    {
        Assert.Equal([1, 2], Citations.FindCited(answer, Sources).Select(source => source.Number));
    }

    [Fact]
    public void FindCited_IgnoresNumbersWithoutASource()
    {
        Assert.Empty(Citations.FindCited("As stated in [7] and [0].", Sources));
    }

    [Fact]
    public void FindCited_AnswerWithoutMarkers_CitesNothing()
    {
        Assert.Empty(Citations.FindCited("I could not find this in the documents.", Sources));
    }

    private static Source Source(int number, string documentId) =>
        new(number, new RetrievedChunk(documentId, documentId, 1, "1 Section", "Text", 0.1));
}
