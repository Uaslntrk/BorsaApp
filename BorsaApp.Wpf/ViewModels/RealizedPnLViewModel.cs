using BorsaApp.BLL.Services;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorsaApp.Wpf.Helpers;
using ClosedXML.Excel;
using BorsaApp.Wpf.Pdf;

namespace BorsaApp.Wpf.ViewModels
{
    public class RealizedPnLViewModel : BaseViewModel
    {
        private readonly RealizedPnLService _service = new();
        private readonly CustomerService _customers = new();

        public ObservableCollection<Customer> CustomerOptions { get; } = new();
        public ObservableCollection<RealizedPnLRow> Rows { get; } = new();

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }
        private DateTime? _fromDate = null;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime? _toDate = null;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private decimal _totalRealized;
        public decimal TotalRealized { get => _totalRealized; set { _totalRealized = value; OnPropertyChanged(); } }

        private string _message = "";
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }

        private int _activeCustomerCount;
        public int ActiveCustomerCount { get => _activeCustomerCount; set { _activeCustomerCount = value; OnPropertyChanged(); } }

        private decimal _totalMarketValue;
        public decimal TotalMarketValue { get => _totalMarketValue; set { _totalMarketValue = value; OnPropertyChanged(); } }

        private decimal _totalUnrealizedPL;
        public decimal TotalUnrealizedPL { get => _totalUnrealizedPL; set { _totalUnrealizedPL = value; OnPropertyChanged(); } }

        private decimal _totalUnrealizedPLPct;
        public decimal TotalUnrealizedPLPct { get => _totalUnrealizedPLPct; set { _totalUnrealizedPLPct = value; OnPropertyChanged(); } }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportExcelCommand { get; }

        public RelayCommand ExportPdfCommand { get; }

        private readonly DashboardService _dashboard = new();

        public RealizedPnLViewModel()
        {
            ExportPdfCommand = new RelayCommand(ExportPdf);
            ExportExcelCommand = new RelayCommand(ExportExcel);
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            _ = InitAsync();
        }
        private void ExportPdf()
        {
            try
            {
                var path = PdfExporter.AskSavePath("realized_pnl.pdf");
                if (path is null) return;

                var logo = LogoLoader.LoadResourceBytes("Assets/logo.png");
                var firmName = "Global Menkul Değerler A.Ş.";

                var fromLocal = FromDate?.Date;
                var toLocal = ToDate?.Date;

                var title = SelectedCustomer is null || SelectedCustomer.Id == 0
                    ? "Realized P/L Raporu (Tüm Müşteriler)"
                    : $"Realized P/L Raporu ({SelectedCustomer.Name})";

                var doc = new RealizedPdfDocument(
                    title,
                    DateTime.Now,
                    Rows.ToList(),
                    logo,
                    firmName,
                    fromLocal,
                    toLocal,
                    ActiveCustomerCount,
                    TotalMarketValue,
                    TotalUnrealizedPL,
                    TotalUnrealizedPLPct,
                    TotalRealized
                );

                PdfExporter.Save(path, doc);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "PDF Export Hatası",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ExportExcel()
        {
            var path = ExcelExporter.AskSavePath("realized_pnl.xlsx");
            if (path is null) return;

            ExcelExporter.SaveWorkbook(path, wb =>
            {
                var ws = wb.Worksheets.Add("Realized");

                ws.Cell(1, 1).Value = "Tarih";
                ws.Cell(1, 2).Value = "Müşteri";
                ws.Cell(1, 3).Value = "Kod";
                ws.Cell(1, 4).Value = "Enstrüman";
                ws.Cell(1, 5).Value = "Lot";
                ws.Cell(1, 6).Value = "Satış";
                ws.Cell(1, 7).Value = "Ort. Maliyet";
                ws.Cell(1, 8).Value = "Realized P/L";
                ws.Cell(1, 9).Value = "P/L %";
                ws.Cell(1, 10).Value = "TradeId";

                ExcelExporter.StyleHeader(ws.Range(1, 1, 1, 10));

                int r = 2;
                foreach (var x in Rows)
                {
                    ws.Cell(r, 1).Value = x.TradeDate;
                    ws.Cell(r, 2).Value = x.CustomerName;
                    ws.Cell(r, 3).Value = x.AssetCode;
                    ws.Cell(r, 4).Value = x.AssetName;
                    ws.Cell(r, 5).Value = x.SellQty;
                    ws.Cell(r, 6).Value = x.SellPrice;
                    ws.Cell(r, 7).Value = x.AvgCostAtSell;
                    ws.Cell(r, 8).Value = x.RealizedPL;
                    ws.Cell(r, 9).Value = x.RealizedPLPct;
                    ws.Cell(r, 10).Value = x.TradeId;
                    r++;
                }

                ws.Cell(r + 1, 7).Value = "TOPLAM";
                ws.Cell(r + 1, 8).Value = TotalRealized;
                ws.Range(r + 1, 7, r + 1, 8).Style.Font.Bold = true;

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                ws.Column(1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                ws.Column(6).Style.NumberFormat.Format = "#,##0.0000";
                ws.Column(7).Style.NumberFormat.Format = "#,##0.0000";
                ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(9).Style.NumberFormat.Format = "0.00";
            });
        }

        private async Task InitAsync()
        {
            try
            {
                CustomerOptions.Clear();
                CustomerOptions.Add(new Customer { Id = 0, Name = "Tüm Müşteriler" });

                var list = await _customers.GetAllAsync();
                foreach (var c in list.Where(x => x.IsActive))
                    CustomerOptions.Add(c);

                SelectedCustomer = CustomerOptions.FirstOrDefault();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                Message = "";
                Rows.Clear();
                DateTime? fromUtc = FromDate?.Date.ToUniversalTime();
                DateTime? toUtc = ToDate?.Date.ToUniversalTime();

                var dash = await _dashboard.GetSummaryAsync();
                ActiveCustomerCount = dash.ActiveCustomerCount;
                TotalMarketValue = dash.TotalMarketValue;
                TotalUnrealizedPL = dash.TotalUnrealizedPL;
                TotalUnrealizedPLPct = dash.TotalUnrealizedPLPct;

                int? cid = (SelectedCustomer is null || SelectedCustomer.Id == 0) ? null : SelectedCustomer.Id;
                var (items, warnings) = await _service.GetRealizedAsync(cid, fromUtc, toUtc);

                foreach (var r in items) Rows.Add(r);

                TotalRealized = Decimal.Round(Rows.Sum(x => x.RealizedPL), 2);

                if (warnings.Count > 0)
                {
                    Message = string.Join(" | ", warnings.Take(3));
                    if (warnings.Count > 3) Message += $" (+{warnings.Count - 3} uyarı)";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }
}
