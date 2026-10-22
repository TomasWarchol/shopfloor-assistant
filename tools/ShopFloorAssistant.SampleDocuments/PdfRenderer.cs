using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ShopFloorAssistant.SampleDocuments;

/// <summary>Renders a sample document as an A4 PDF with the header and footer a real plant document would have.</summary>
internal static class PdfRenderer
{
    // Fixed dates keep regenerated files identical when the sources have not changed.
    private static readonly DateTimeOffset GeneratedOn = new(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);

    public static void Render(MarkdownDocument document, string outputFile)
    {
        Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(style => style.FontSize(10.5f).LineHeight(1.3f));

            page.Header().BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingBottom(6).Row(row =>
            {
                row.RelativeItem().Text("Contoso Metal Stamping – Plant 2").FontSize(8).FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignRight().Text(document.Code).FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            page.Content().PaddingVertical(12).Column(column =>
            {
                column.Spacing(6);
                column.Item().Text(document.Title).FontSize(18).Bold();
                column.Item().PaddingBottom(6).Text(document.Metadata).FontSize(9).FontColor(Colors.Grey.Darken2);

                var blocks = document.Blocks;
                for (var i = 0; i < blocks.Count; i++)
                {
                    if (blocks[i] is not Heading)
                    {
                        RenderBlock(column, blocks[i]);
                        continue;
                    }

                    // Keep headings on the same page as the paragraph that follows them.
                    var group = new List<Block>();
                    while (i < blocks.Count && blocks[i] is Heading)
                    {
                        group.Add(blocks[i++]);
                    }

                    if (i < blocks.Count && blocks[i] is Paragraph)
                    {
                        group.Add(blocks[i]);
                    }
                    else
                    {
                        i--;
                    }

                    column.Item().ShowEntire().Column(inner =>
                    {
                        inner.Spacing(6);
                        foreach (var block in group)
                        {
                            RenderBlock(inner, block);
                        }
                    });
                }
            });

            page.Footer().BorderTop(0.5f).BorderColor(Colors.Grey.Medium).PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text(document.IsPolish
                        ? "Dokument syntetyczny wyłącznie do celów demonstracyjnych. Contoso jest firmą fikcyjną."
                        : "Synthetic document for demonstration only. Contoso is a fictional company.")
                    .FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                row.ConstantItem(90).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span(document.IsPolish ? "Strona " : "Page ");
                    text.CurrentPageNumber();
                    text.Span(document.IsPolish ? " z " : " of ");
                    text.TotalPages();
                });
            });
        }))
        .WithMetadata(new DocumentMetadata
        {
            Title = document.Title,
            Subject = document.Code,
            Author = "Contoso Metal Stamping (fictional)",
            CreationDate = GeneratedOn,
            ModifiedDate = GeneratedOn,
        })
        .GeneratePdf(outputFile);
    }

    private static void RenderBlock(ColumnDescriptor column, Block block)
    {
        switch (block)
        {
            // EnsureSpace keeps a heading followed by a list or table from being the last line on a page.
            case Heading { Level: 2 } heading:
                column.Item().EnsureSpace(90).PaddingTop(10).Text(heading.Text).FontSize(13).Bold();
                break;

            case Heading heading:
                column.Item().EnsureSpace(70).PaddingTop(4).Text(heading.Text).FontSize(11.5f).SemiBold();
                break;

            case Paragraph paragraph:
                column.Item().Text(paragraph.Text);
                break;

            case ListBlock list:
                column.Item().Column(items =>
                {
                    items.Spacing(3);
                    foreach (var (marker, text) in list.Items)
                    {
                        items.Item().Row(row =>
                        {
                            row.ConstantItem(20).Text(marker);
                            row.RelativeItem().Text(text);
                        });
                    }
                });
                break;

            case Table table:
                column.Item().PaddingVertical(4).Table(grid =>
                {
                    grid.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in table.Header)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    grid.Header(header =>
                    {
                        foreach (var cell in table.Header)
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(cell).FontSize(9.5f).Bold();
                        }
                    });

                    foreach (var cell in table.Rows.SelectMany(row => row))
                    {
                        grid.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(cell).FontSize(9.5f);
                    }
                });
                break;
        }
    }
}
