using BorsaApp.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BorsaApp.Wpf.Pdf;

public class RealizedPdfDocument : IDocument
{
    private readonly string _title;
    private readonly DateTime _createdAt;
    private readonly IReadOnlyList<RealizedPnLRow> _rows;

    private readonly byte[] _logoBytes;
    private readonly string _firmName;
    private readonly DateTime? _fromLocal;
    private readonly DateTime? _toLocal;

    private readonly int _activeCustomerCount;
    private readonly decimal _totalMarket;      // istersen 0 geç
    private readonly decimal _totalUnrealized;  // istersen 0 geç
    private readonly decimal _totalUnrealizedPct; // istersen 0 geç
    private readonly decimal _totalRealized;

    public RealizedPdfDocument(
        string title,
        DateTime createdAt,
        IReadOnlyList<RealizedPnLRow> rows,
        byte[] logoBytes,
        string firmName,
        DateTime? fromLocal,
        DateTime? toLocal,
        int activeCustomerCount,
        decimal totalMarket,
        decimal totalUnrealized,
        decimal totalUnrealizedPct,
        decimal totalRealized)
    {
        _title = title;
        _createdAt = createdAt;
        _rows = rows;

        _logoBytes = logoBytes;
        _firmName = firmName;
        _fromLocal = fromLocal;
        _toLocal = toLocal;

        _activeCustomerCount = activeCustomerCount;
        _totalMarket = totalMarket;
        _totalUnrealized = totalUnrealized;
        _totalUnrealizedPct = totalUnrealizedPct;
        _totalRealized = totalRealized;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(10));

            // ✅ Header: logo + firma + başlık + tarih aralığı
            page.Header().ShowOnce().Row(row =>
            {
                row.ConstantItem(80).AlignMiddle().Height(32).Image(_logoBytes).FitHeight();

                row.RelativeItem().Column(s =>
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

            // ✅ Content: dashboard kartları + tablo
            page.Content().PaddingTop(10).Column(col =>
            {
                col.Item().Row(r =>
                {
                    r.RelativeItem().Element(e => Card(e, "Aktif Müşteri", _activeCustomerCount.ToString()));
                    r.RelativeItem().Element(e => Card(e, "Portföy Değeri", _totalMarket.ToString("#,##0.00")));
                    r.RelativeItem().Element(e => Card(e, "Unrealized P/L", $"{_totalUnrealized:#,##0.00} (%{_totalUnrealizedPct:#,##0.00})"));
                    r.RelativeItem().Element(e => Card(e, "Realized P/L", _totalRealized.ToString("#,##0.00")));
                });

                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1.4f); // tarih
                        c.RelativeColumn(2);    // müşteri
                        c.RelativeColumn(1);    // kod
                        c.RelativeColumn(1);    // lot
                        c.RelativeColumn(1);    // satış
                        c.RelativeColumn(1);    // ort mal
                        c.RelativeColumn(1);    // realized
                        c.RelativeColumn(1);    // %
                    });

                    table.Header(h =>
                    {
                        H(h, "Tarih");
                        H(h, "Müşteri");
                        H(h, "Kod");
                        H(h, "Lot");
                        H(h, "Satış");
                        H(h, "Ort.Mal");
                        H(h, "Realized");
                        H(h, "P/L %");
                    });

                    int i = 0;
                    foreach (var r in _rows)
                    {
                        var zebra = (i++ % 2 == 0) ? Colors.White : Colors.Grey.Lighten5;

                        B(table, r.TradeDate.ToString("yyyy-MM-dd HH:mm"), zebra);
                        B(table, r.CustomerName ?? "", zebra);
                        B(table, r.AssetCode ?? "", zebra);
                        B(table, r.SellQty.ToString(), zebra);
                        B(table, r.SellPrice.ToString("#,##0.0000"), zebra);
                        B(table, r.AvgCostAtSell.ToString("#,##0.0000"), zebra);
                        B(table, r.RealizedPL.ToString("#,##0.00"), zebra);
                        B(table, r.RealizedPLPct.ToString("#,##0.00"), zebra);
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
