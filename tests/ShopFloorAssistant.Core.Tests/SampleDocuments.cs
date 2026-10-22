namespace ShopFloorAssistant.Core.Tests;

/// <summary>The synthetic PDFs committed in samples/documents.</summary>
internal static class SampleDocuments
{
    public static string DirectoryPath { get; } = Path.Combine(FindRepositoryRoot(), "samples", "documents");

    public static string Get(string fileName) => Path.Combine(DirectoryPath, fileName);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ShopFloorAssistant.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root with ShopFloorAssistant.slnx not found.");
    }
}
