using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class DocumentService : IDocumentService
{
    public byte[] GenerateTestPdf(string name)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header()
                    .Text("Animal Help Group")
                    .FontSize(24)
                    .Bold();

                page.Content()
                    .Text($"Hello {name}!");

                page.Footer()
                    .AlignCenter()
                    .Text(DateTime.Now.ToString("yyyy-MM-dd"));
            });
        }).GeneratePdf();
    }
}
