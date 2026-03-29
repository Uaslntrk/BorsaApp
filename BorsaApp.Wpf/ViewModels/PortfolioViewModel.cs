using BorsaApp.BLL.Services;
using BorsaApp.Entities;
using BorsaApp.Wpf.Helpers;
using BorsaApp.Wpf.Pdf;
using ClosedXML.Excel;
using MyApp.Wpf.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BorsaApp.Wpf.ViewModels
{
    public class PortfolioViewModel : BaseViewModel
    {
        private readonly PortfolioService _service = new();
        private readonly CustomerService _customers = new();

        public ObservableCollection<Customer> CustomerOptions { get; } = new();
        public ObservableCollection<PortfolioPosition> Positions { get; } = new();

        private decimal _totalCost;
        public decimal TotalCost { get => _totalCost; set { _totalCost = value; OnPropertyChanged(); } }
        public int ActiveCustomerCount { get; set; } // istersen OnPropertyChanged ekle
        public decimal TotalRealizedPL { get; set; }

        private decimal _totalMarket;
        public decimal TotalMarket { get => _totalMarket; set { _totalMarket = value; OnPropertyChanged(); } }

        private decimal _totalPL;
        public decimal TotalPL { get => _totalPL; set { _totalPL = value; OnPropertyChanged(); } }

        private decimal _totalPLPct;
        public decimal TotalPLPct { get => _totalPLPct; set { _totalPLPct = value; OnPropertyChanged(); } }

        private DateTime? _fromDate = null;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }


        private DateTime? _toDate = null;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }


        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportExcelCommand { get; }
        public RelayCommand ExportPdfCommand { get; }
        public PortfolioViewModel()
        {
            ExportPdfCommand = new RelayCommand(ExportPdf);
            ExportExcelCommand = new RelayCommand(ExportExcel);
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            _ = InitAsync();

            LiveMarketService.Instance.MarketDataUpdated += async (s, e) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadAsync();
                });
            };
        }
        private void ExportPdf()
        {
            try
            {
                var path = PdfExporter.AskSavePath("portfoy.pdf");
                if (path is null) return;

                // Logo burada patlarsa artık yakalanacak
                var logo = LogoLoader.LoadResourceBytes("Assets/logo.png");

                var firmName = "Global Menkul Değerler A.Ş.";
                var fromLocal = FromDate?.Date;
                var toLocal = ToDate?.Date;

                var title = SelectedCustomer is null || SelectedCustomer.Id == 0
                    ? "Portföy Raporu (Tüm Müşteriler)"
                    : $"Portföy Raporu ({SelectedCustomer.Name})";

                var doc = new PortfolioPdfDocument(
                    title,
                    DateTime.Now,
                    Positions.ToList(),
                    TotalCost,
                    TotalMarket,
                    TotalPL,
                    TotalPLPct,
                    logo,
                    firmName,
                    fromLocal,
                    toLocal,
                    ActiveCustomerCount,
                    TotalRealizedPL
                );

                PdfExporter.Save(path, doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "PDF Hatası",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void ExportExcel()
        {
            var path = ExcelExporter.AskSavePath("portfoy.xlsx");
            if (path is null) return;

            ExcelExporter.SaveWorkbook(path, wb =>
            {
                var ws = wb.Worksheets.Add("Portföy");

                // Header
                ws.Cell(1, 1).Value = "Müşteri";
                ws.Cell(1, 2).Value = "Kod";
                ws.Cell(1, 3).Value = "Enstrüman";
                ws.Cell(1, 4).Value = "Net Lot";
                ws.Cell(1, 5).Value = "Ort. Maliyet";
                ws.Cell(1, 6).Value = "Güncel";
                ws.Cell(1, 7).Value = "Maliyet Tutar";
                ws.Cell(1, 8).Value = "Piyasa Tutar";
                ws.Cell(1, 9).Value = "Unrealized P/L";
                ws.Cell(1, 10).Value = "P/L %";

                ExcelExporter.StyleHeader(ws.Range(1, 1, 1, 10));

                // Rows
                int r = 2;
                foreach (var p in Positions)
                {
                    ws.Cell(r, 1).Value = p.CustomerName;
                    ws.Cell(r, 2).Value = p.AssetCode;
                    ws.Cell(r, 3).Value = p.AssetName;
                    ws.Cell(r, 4).Value = p.NetQty;
                    ws.Cell(r, 5).Value = p.AvgCost;
                    ws.Cell(r, 6).Value = p.CurrentPrice;
                    ws.Cell(r, 7).Value = p.CostValue;
                    ws.Cell(r, 8).Value = p.MarketValue;
                    ws.Cell(r, 9).Value = p.UnrealizedPL;
                    ws.Cell(r, 10).Value = p.UnrealizedPLPct;
                    r++;
                }

                // Totals row
                ws.Cell(r + 1, 6).Value = "TOPLAM";
                ws.Cell(r + 1, 7).Value = TotalCost;
                ws.Cell(r + 1, 8).Value = TotalMarket;
                ws.Cell(r + 1, 9).Value = TotalPL;
                ws.Cell(r + 1, 10).Value = TotalPLPct;

                ws.Range(r + 1, 6, r + 1, 10).Style.Font.Bold = true;

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                // Format
                ws.Column(5).Style.NumberFormat.Format = "#,##0.0000";
                ws.Column(6).Style.NumberFormat.Format = "#,##0.0000";
                ws.Column(7).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(8).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(9).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(10).Style.NumberFormat.Format = "0.00";
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

                DateTime? fromUtc = FromDate?.Date.ToUniversalTime();
                DateTime? toUtc = ToDate?.Date.ToUniversalTime();

                int? cid = (SelectedCustomer is null || SelectedCustomer.Id == 0) ? null : SelectedCustomer.Id;
                var items = await _service.GetPortfolioAsync(cid, fromUtc, toUtc);

                var toRemove = Positions.Where(p => !items.Any(i => i.AssetId == p.AssetId && i.CustomerId == p.CustomerId)).ToList();
                foreach (var rm in toRemove) Positions.Remove(rm);

                foreach (var p in items) 
                {
                    var existing = Positions.FirstOrDefault(x => x.AssetId == p.AssetId && x.CustomerId == p.CustomerId);
                    if (existing != null)
                    {
                        existing.NetQty = p.NetQty;
                        existing.AvgCost = p.AvgCost;
                        existing.CurrentPrice = p.CurrentPrice;
                        existing.CostValue = p.CostValue;
                        existing.MarketValue = p.MarketValue;
                        existing.UnrealizedPL = p.UnrealizedPL;
                        existing.UnrealizedPLPct = p.UnrealizedPLPct;
                    }
                    else
                    {
                        Positions.Add(p);
                    }
                }
               
                // ✅ totals
                TotalCost = Decimal.Round(Positions.Sum(x => x.CostValue), 2);
                TotalMarket = Decimal.Round(Positions.Sum(x => x.MarketValue), 2);
                TotalPL = Decimal.Round(Positions.Sum(x => x.UnrealizedPL), 2);
                TotalPLPct = TotalCost > 0 ? Decimal.Round((TotalPL / TotalCost) * 100m, 2) : 0m;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }
}
