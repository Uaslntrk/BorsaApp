using BorsaApp.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyApp.Wpf.Pdf;

public class PortfolioPdfDocument : IDocument
{
    private readonly string _title;
    private readonly DateTime _createdAt;
    private readonly IReadOnlyList<PortfolioPosition> _rows;

    private readonly decimal _totalCost;
    private readonly decimal _totalMarket;
    private readonly decimal _totalPL;
    private readonly decimal _totalPLPct;

    // ✅ yeni alanlar
    private readonly byte[] _logoBytes;
    private readonly string _firmName;
    private readonly DateTime? _fromLocal;
    private readonly DateTime? _toLocal;
    private readonly int _activeCustomerCount;
    private readonly decimal _totalRealized;

    public PortfolioPdfDocument(
        string title,
        DateTime createdAt,
        IReadOnlyList<PortfolioPosition> rows,
        decimal totalCost,
        decimal totalMarket,
        decimal totalPL,
        decimal totalPLPct,
        byte[] logoBytes,
        string firmName,
        DateTime? fromLocal,
        DateTime? toLocal,
        int activeCustomerCount,
        decimal totalRealized)
    {
        _title = title;
        _createdAt = createdAt;
        _rows = rows;

        _totalCost = totalCost;
        _totalMarket = totalMarket;
        _totalPL = totalPL;
        _totalPLPct = totalPLPct;

        _logoBytes = logoBytes;
        _firmName = firmName;
        _fromLocal = fromLocal;
        _toLocal = toLocal;
        _activeCustomerCount = activeCustomerCount;
        _totalRealized = totalRealized;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    [Obsolete]
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().ShowOnce().Row(row =>
            {
                row.ConstantColumn(80)
    .Height(32)
    .AlignMiddle()
    .Image(_logoBytes, ImageScaling.FitHeight);



                row.RelativeColumn().Stack(s =>
                {
                    s.Item().Text(_firmName).FontSize(12).SemiBold();
                    s.Item().Text(_title).FontSize(16).SemiBold();

                    var rangeText =
                        (_fromLocal is null && _toLocal is null)
                            ? "Tarih Aralığı: Tümü"
                            : $"Tarih Aralığı: {_fromLocal:yyyy-MM-dd} - {_toLocal:yyyy-MM-dd}";

                    s.Item().Text(rangeText).FontColor(Colors.Grey.Darken1).FontSize(9);
                    s.Item().Text($"Oluşturma: {_createdAt:yyyy-MM-dd HH:mm}")
                        .FontColor(Colors.Grey.Darken1).FontSize(9);
                });
            });


            page.Content().PaddingTop(10).Column(col =>
            {
                // ✅ Dashboard kartları artık Content'te
                col.Item().Row(r =>
                {
                    r.RelativeColumn().Element(e => Card(e, "Aktif Müşteri", _activeCustomerCount.ToString()));
                    r.RelativeColumn().Element(e => Card(e, "Portföy Değeri", _totalMarket.ToString("#,##0.00")));
                    r.RelativeColumn().Element(e => Card(e, "Unrealized P/L", $"{_totalPL:#,##0.00} (%{_totalPLPct:#,##0.00})"));
                    r.RelativeColumn().Element(e => Card(e, "Realized P/L", _totalRealized.ToString("#,##0.00")));
                });

                col.Item().PaddingTop(10).Table(table =>
                {
                    // ✅ burası senin mevcut tablo kodun (aynen kalsın)
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    table.Header(h =>
                    {
                        H(h, "Müşteri"); H(h, "Kod"); H(h, "Net Lot"); H(h, "Ort.Mal");
                        H(h, "Güncel"); H(h, "Maliyet"); H(h, "Piyasa"); H(h, "P/L"); H(h, "P/L %");
                    });

                    int i = 0;
                    foreach (var r in _rows)
                    {
                        var zebra = (i++ % 2 == 0) ? Colors.White : Colors.Grey.Lighten5;
                        B(table, r.CustomerName ?? "", zebra);
                        B(table, r.AssetCode ?? "", zebra);
                        B(table, r.NetQty.ToString(), zebra);
                        B(table, r.AvgCost.ToString("#,##0.0000"), zebra);
                        B(table, r.CurrentPrice.ToString("#,##0.0000"), zebra);
                        B(table, r.CostValue.ToString("#,##0.00"), zebra);
                        B(table, r.MarketValue.ToString("#,##0.00"), zebra);
                        B(table, r.UnrealizedPL.ToString("#,##0.00"), zebra);
                        B(table, r.UnrealizedPLPct.ToString("#,##0.00"), zebra);
                    }
                });
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("BorsaApp • Sayfa ");
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    private static void Card(IContainer c, string title, string value)
    {
        c.Border(1).BorderColor(Colors.Grey.Lighten2)
         .Padding(6)
         .Column(col =>
         {
             col.Item().Text(title).FontColor(Colors.Grey.Darken1).FontSize(8);
             col.Item().Text(value).FontSize(11).SemiBold();
         });
    }



    private static void H(TableCellDescriptor h, string text) =>
        h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(text).SemiBold();

    private static void B(TableDescriptor t, string text, string bg) =>
        t.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(text);
}
