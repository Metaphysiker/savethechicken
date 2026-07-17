using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Classes;

public class MeetingLocationScheduleDocument : IDocument
{
    private readonly MeetingLocationScheduleModel _model;

    public MeetingLocationScheduleDocument(MeetingLocationScheduleModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.Size(PageSizes.A4);

            page.DefaultTextStyle(x => x.FontSize(11));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Text("Treffpunkte")
            .FontSize(18)
            .Bold();
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingTop(15).Column(col =>
        {
            col.Spacing(20);

            foreach (var day in _model.Days.OrderBy(d => d.Date))
            {
                col.Item().Element(x => ComposeDay(x, day));
            }
        });
    }

    void ComposeDay(IContainer container, MeetingLocationScheduleDayGroup day)
    {
        container.Column(col =>
        {
            col.Spacing(10);

            col.Item().Text(day.Date.ToString("dd.MM.yyyy")).FontSize(15).Bold();

            foreach (var location in day.Locations.OrderBy(l => l.DateTime))
            {
                col.Item().Element(x => ComposeLocation(x, location));
            }
        });
    }

    void ComposeLocation(IContainer container, MeetingLocationScheduleEntry location)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2); // blank / spacer
                columns.RelativeColumn(3); // chicken/rooster count
                columns.RelativeColumn(4); // name
                columns.RelativeColumn(3); // phone
            });

            table.Cell().ColumnSpan(4).Text($"Treffpunkt {location.SequenceInDay}: {location.DateTime:HH:mm} Uhr {location.Name}")
                .Bold()
                .FontSize(13);

            if (location.AssignedRequests.Count == 0)
            {
                table.Cell().ColumnSpan(4).PaddingTop(4).PaddingLeft(10).Text("Keine Zuordnungen").Italic();
            }
            else
            {
                foreach (var request in location.AssignedRequests)
                {
                    table.Cell().PaddingTop(2).Text("");
                    table.Cell().PaddingTop(2).Text(FormatCounts(request));
                    table.Cell().PaddingTop(2).Text(request.Name);
                    table.Cell().PaddingTop(2).Text(request.Phone);
                }
            }
        });
    }

    private static string FormatCounts(AssignedRequestEntry request)
    {
        if (request.RoosterCount > 0)
            return $"{request.ChickenCount} Hühner - {request.RoosterCount} Hähne";

        return $"{request.ChickenCount} Hühner";
    }

    void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Darken2));
                text.Span("© Stiftung Tiere in Not – Animal Help, CH-8032 Zürich");
            });

            row.ConstantItem(80).AlignRight().Text(text =>
            {
                text.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Darken2));
                text.Span("Seite ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }
}
