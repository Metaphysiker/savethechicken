using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ChickenHandoverDocument : IDocument
{
    private readonly ChickenHandoverModel _model;

    public ChickenHandoverDocument(ChickenHandoverModel model)
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

    }

    void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {

            col.Item().Text("Abgabevereinbarung")
                .FontSize(18)
                .Bold();
            col.Spacing(15);

            col.Item().Text("zwischen")
    .Italic();
            col.Spacing(15);

            // Parties
            col.Item().Text(text =>
            {
                text.Span("Stiftung Tiere in Not – Animal Help (Stinah)").Bold();
                text.Span("\nSophienstrasse 2, 8032 Zürich");
                text.Span("\n(im Folgenden: Stinah)");
            });

            col.Item().PaddingTop(5).Text("und");

            col.Item().Text(text =>
            {
                text.Span(_model.OvernehmerName ?? "...........................................");
                text.Span("\n(im Folgenden: Übernehmer)");
            });

            var uebernahmeString = BuildUebernahmeText(_model);

            col.Item()
               .PaddingTop(10)
               .Text($"betreffend Übernahme von {uebernahmeString}");

            col.Item().PaddingTop(10).Text("Präambel").Bold();

            col.Item().Text("""
Stinah freut sich und verdankt, dass die obgenannten Hühner beim/bei der Übernehmer(in) einen Lebensplatz finden.

Die Hühner haben ihr Leben bis zum heutigen Tag in einem Eierproduktionsbetrieb verbracht und eine gewaltige Leistung erbracht. Im Zeitpunkt der Ausstallung sind sie aufgrund dieser Leistung wie auch der unnatürlichen Haltung häufig ausgelaugt, was man u.a. am Federkleid erkennt.

Die erlittene Belastung führt dazu, dass die Tiere häufig sehr stressanfällig sind. Es ist deshalb wichtig, ihnen möglichst bald nach der Übernahme eine ruhige Rückzugsmöglichkeit zu bieten.

Stinah empfiehlt, die Hühner einem Tierarzt vorzustellen.

Stinah ist Ansprechpartner unter info@stinah.ch oder rettetdashuhn@stinah.ch.
""");

            col.Item().PaddingTop(10).Element(ComposeArticles);

            col.Item().PaddingTop(10).Element(ComposeMisc);
            col.Item().PaddingTop(20).Element(ComposeSignature);
        });
    }

    void ComposeArticles(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(12);

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 1",
                "Eigentum",
                "Der/die Übernehmer/in wird Eigentümer/in der Hühner. Die Eigentumsübertragung erfolgt unentgeltlich."
            ));

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 2",
                "Gewährleistung/Haftung",
                "Die Hühner werden wie besehen übernommen. Keine Gewährleistung für Gesundheit oder Charakter."
            ));

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 3",
                "Zweck der Übereignung",
                "Die Hühner werden als Heimtiere gehalten und nicht zu Produktionszwecken."
            ));

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 4",
                "Pflichten",
                "Der/die Übernehmer(in) verpflichtet sich zu artgerechter Haltung, insbesondere:\n" +
                "• trockener, sauberer, sicherer Stall\n" +
                "• täglicher Freilauf\n" +
                "• nicht einzeln halten\n" +
                "• ausreichend Futter und Wasser\n" +
                "• tierärztliche Versorgung bei Bedarf"
            ));

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 5",
                "Prüfung der Haltung",
                "Stinah darf die Haltung prüfen. Bei Missständen sind Korrekturen oder Rückgabe möglich."
            ));

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 6",
                "Verschiedenes",
                "Transportkisten sind zurückzugeben oder zu entsorgen."
            ));

            col.Item().ShowEntire().Element(x => Article(x,
                "Art. 7",
                "Anwendbares Recht und Gerichtsstand",
                "Schweizer Recht. Gerichtsstand Zürich."
            ));
        });
    }

    void Article(IContainer container, string articleNumber, string title, string text)
    {
        container.Row(row =>
        {
            // LEFT COLUMN: ONLY "Art. X"
            row.ConstantItem(70)
                .AlignTop()
                .Text(articleNumber)
                .Bold();

            // RIGHT COLUMN: title + body
            row.RelativeItem().Column(col =>
            {
                col.Spacing(4);

                col.Item().Text(title)
                    .Bold()
                    .FontSize(11);

                col.Item().Text(text);
            });
        });
    }

    void ComposeMisc(IContainer container)
    {
        container.Text($"Ort/Datum: Zürich, {_model.Date:dd.MM.yyyy}");
    }

    void ComposeSignature(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("_________________________");
                col.Item().Text("Unterschrift Übernehmer");
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("_________________________");
                col.Item().Text("Stiftung Tiere in Not – Animal Help");
            });
        });
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

    private string BuildUebernahmeText(ChickenHandoverModel model)
    {
        var parts = new List<string>();

        if (model.ChickenCount > 0)
            parts.Add(FormatChicken(model.ChickenCount, "Huhn", "Hühnern"));

        if (model.RoosterCount > 0)
            parts.Add(FormatChicken(model.RoosterCount, "Hahn", "Hähnen"));

        return string.Join(" und ", parts);
    }

    private static string FormatChicken(int count, string singular, string plural)
    => count == 1 ? $"1 {singular}" : $"{count} {plural}";
}
