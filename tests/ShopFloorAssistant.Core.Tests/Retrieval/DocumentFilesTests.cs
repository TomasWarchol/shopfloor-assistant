using ShopFloorAssistant.Core.Retrieval;

namespace ShopFloorAssistant.Core.Tests.Retrieval;

public class DocumentFilesTests
{
    [Fact]
    public void Resolve_ExistingPdf_ReturnsItsFullPath()
    {
        var path = DocumentFiles.Resolve(SampleDocuments.DirectoryPath, "SOP-014-die-change-line-P12.pdf");

        Assert.Equal(SampleDocuments.Get("SOP-014-die-change-line-P12.pdf"), path);
    }

    [Theory]
    [InlineData("../README.md")]
    [InlineData("..\\README.md")]
    [InlineData("../source/SOP-014-die-change-line-P12.md")]
    [InlineData("../../ShopFloorAssistant.slnx")]
    [InlineData("missing.pdf")]
    [InlineData("SOP-014-die-change-line-P12")]
    [InlineData("")]
    public void Resolve_AnythingElse_ReturnsNull(string fileName)
    {
        Assert.Null(DocumentFiles.Resolve(SampleDocuments.DirectoryPath, fileName));
    }
}
