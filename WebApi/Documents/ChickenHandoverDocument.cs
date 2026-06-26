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

Die Hühner haben ihr Leben bis zum heutigen Tag in einem Eierproduktionsbetrieb verbracht und eine gewaltige Leistung erbracht. Im Zeitpunkt der Ausstallung sind sie aufgrund dieser Leistung wie auch der unnatürlichen Haltung wegen häufig ausgelaugt, was man u.a. am Federkleid erkennt.

Die erlittene Belastung führt dazu, dass die Tiere häufig sehr stressanfällig sind. Es ist deshalb wichtig, ihnen möglichst bald nach der Übernahme eine ruhige Rückzugsmöglichkeit zu bieten, wo sie sich vor den Umwelteinflüssen schützen können.

Stinah empfiehlt, die Hühner nach der Übernahme einem Tierarzt vorzustellen, um einerseits den Gesundheitszustand zu erheben und andererseits einen Ansprechpartner für den Krankheitsfall zu haben.

Stinah ist gerne Ansprechpartner bei Fragen (info@stinah.ch oder rettetdashuhn@stinah.ch). Wichtige Infos finden sich auch unter www.rettetdashuhn.ch. Und selbstverständlich lesen wir auch gerne Berichte oder sehen Bilder „unserer“ Geretteten.

Die folgenden Bestimmungen regeln und informieren über die grundsätzlichen Parameter der Übernahme.
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
            "Der/die Übernehmer/in wird Eigentümer/in und damit Halter/in der vorgenannten Hühner. Die Eigentumsübertragung erfolgt unentgeltlich."
        ));

        col.Item().ShowEntire().Element(x => Article(x,
            "Art. 2",
            "Gewährleistung/Haftung",
            "Die Hühner werden vom/von der Übernehmer(in) in Besitz genommen wie besehen. Es sind keinerlei Krankheiten oder Verhaltensstörungen bekannt. Stinah übernimmt keinerlei Gewährleistung für die Gesundheit oder den Charakter der Tiere."
        ));

        col.Item().ShowEntire().Element(x => Article(x,
            "Art. 3",
            "Zweck der Übereignung",
            "Die Hühner werden nicht zu Produktionszwecken, sondern als Heimtiere abgegeben. Der/die Übernehmerin bietet den Tieren einen artgerechten Lebensplatz."
        ));

        col.Item().ShowEntire().Element(x => ArticleWithBullets(x,
            "Art. 4",
            "Pflichten",
            "Als Halter der Hühner verpflichtet sich der/die Übernehmer(in), die gestützt auf diese Vereinbarung übernommenen Hühner bis zu ihrem natürlichen Tod artgerecht zu halten, sie insbesondere",
            new[]
            {
                "in einem trockenen, sauberen, zugluftfreien, fuchs- und mardersicheren Stall unterzubringen, der im Winter mit einer Wärmelampe oder auf andere Weise bei Bedarf geheizt werden kann, und ihnen täglich grosszügigen Freilauf zu bieten, wo sie Gras und Steinchen aufnehmen, im Sand und in der Sonne baden etc. können;",
                "nicht einzeln zu halten;",
                "abwechslungsreich und ausreichend zu füttern und ständigen Zugang zu frischem Wasser zu gewährleisten;",
                "sie bei Bedarf tierärztlich zu versorgen."
            },
            "Falls die Tiere vom Übernehmer nicht mehr in der obgenannten Form gehalten werden können, setzt der Eigentümer Stinah rechtzeitig darüber in Kenntnis, so dass gemeinsam eine gute neue Lösung für die Tiere gefunden werden kann. Der/die Übernehmer(in) verpflichtet sich, die Hühner entsprechend den jeweiligen Vorgaben im Kanton, in welchem die Tiere gehalten werden, bei den Behörden zu melden."
        ));

        col.Item().ShowEntire().Element(x => Article(x,
            "Art. 5",
            "Prüfung der Haltung",
            "Stinah ist berechtigt, die Tierhaltung selbst oder unter Beizug eines Bevollmächtigten nach Vorankündigung zu überprüfen. Sollten dabei Missstände festgestellt werden, mahnt sie den/die Übernehmer/in ab. Bei Vorliegen krasser Missstände bzw. trotz Abmahnung anhaltender mangelhafter Bedingungen ist der/die Übernehmer(in) verpflichtet, die Tiere an Stinah abzugeben. Die Rücknahme der Tiere erfolgt unentgeltlich. Dem/der Eigentümer(in) werden keinerlei Kosten erstattet."
        ));

        col.Item().ShowEntire().Element(x => Article(x,
            "Art. 6",
            "Verschiedenes",
            "Die zur Verfügung gestellten Hühner-Transportkisten sind an Stinah zurückzugeben oder umgehend zu entsorgen (Hygiene!)"
        ));

        col.Item().ShowEntire().Element(x => Article(x,
            "Art. 7",
            "Anwendbares Recht und Gerichtsstand",
            "Die vorliegende Vereinbarung untersteht Schweizer Recht. Die Parteien vereinbaren Zürich als Gerichtsstand."
        ));
    });
}

void ArticleWithBullets(IContainer container, string articleNumber, string title, string intro, string[] bullets, string outro)
{
    container.Row(row =>
    {
        row.ConstantItem(70)
            .AlignTop()
            .Text(articleNumber)
            .Bold();

        row.RelativeItem().Column(col =>
        {
            col.Spacing(4);

            col.Item().Text(title).Bold().FontSize(11);

            col.Item().Text(intro);

            foreach (var bullet in bullets)
            {
                col.Item().Row(r =>
                {
                    r.ConstantItem(16).Text("•");
                    r.RelativeItem().Text(bullet);
                });
            }

            col.Item().Text(outro);
        });
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
