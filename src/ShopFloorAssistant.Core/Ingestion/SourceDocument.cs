namespace ShopFloorAssistant.Core.Ingestion;

/// <summary>A document read from a file, split into pages so every answer can cite a page number.</summary>
/// <param name="Id">Stable identifier, the file name without extension.</param>
/// <param name="Title">Title from the PDF metadata, or the file name if the metadata is empty.</param>
/// <param name="Pages">Pages in order, without running headers and footers.</param>
public sealed record SourceDocument(string Id, string Title, IReadOnlyList<DocumentPage> Pages);

/// <summary>One page as text blocks in reading order: a paragraph, a heading, a list item or a table row.</summary>
public sealed record DocumentPage(int Number, IReadOnlyList<string> Blocks);
