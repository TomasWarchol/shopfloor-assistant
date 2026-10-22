// Generates the synthetic PDF documents in samples/documents from the Markdown sources in samples/source.
// Run from anywhere inside the repository: dotnet run --project tools/ShopFloorAssistant.SampleDocuments
using QuestPDF.Infrastructure;
using ShopFloorAssistant.SampleDocuments;

QuestPDF.Settings.License = LicenseType.Community;

var root = new DirectoryInfo(AppContext.BaseDirectory);
while (root is not null && !File.Exists(Path.Combine(root.FullName, "ShopFloorAssistant.slnx")))
{
    root = root.Parent;
}

if (root is null)
{
    Console.Error.WriteLine("Run this tool from inside the repository.");
    return 1;
}

var sourceDirectory = Path.Combine(root.FullName, "samples", "source");
var outputDirectory = Path.Combine(root.FullName, "samples", "documents");
Directory.CreateDirectory(outputDirectory);

foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory, "*.md").Order(StringComparer.Ordinal))
{
    var document = MarkdownDocument.Parse(File.ReadAllText(sourceFile));
    var outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourceFile) + ".pdf");
    PdfRenderer.Render(document, outputFile);
    Console.WriteLine($"{Path.GetFileName(outputFile)} ({document.Blocks.Count} blocks)");
}

return 0;
