namespace ShopFloorAssistant.Core.Retrieval;

public static class DocumentFiles
{
    /// <summary>
    /// Resolves the file name from a citation link to a PDF in the documents folder. Only a plain file name
    /// of an existing PDF is accepted, so a link cannot reach other files (for example "../appsettings.json").
    /// </summary>
    public static string? Resolve(string documentsDirectory, string fileName)
    {
        if (fileName.Length == 0
            || fileName != Path.GetFileName(fileName)
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var path = Path.Combine(Path.GetFullPath(documentsDirectory), fileName);
        return File.Exists(path) ? path : null;
    }
}
