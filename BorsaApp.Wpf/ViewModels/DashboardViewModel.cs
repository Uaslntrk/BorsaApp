using BorsaApp.BLL.Services;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BorsaApp.Wpf.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DashboardService _service = new();

        public ObservableCollection<PortfolioPosition> Winners { get; } = new();
        public ObservableCollection<PortfolioPosition> Losers { get; } = new();

        public ObservableCollection<ISeries> AssetDistributionSeries { get; set; } = new();
        public ObservableCollection<ISeries> PLSeries { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new()
        {
            new Axis
            {
                Labels = new List<string>(),
                LabelsRotation = 15
            }
        };

        private int _activeCustomerCount;
        public int ActiveCustomerCount { get => _activeCustomerCount; set { _activeCustomerCount = value; OnPropertyChanged(); } }

        private decimal _totalCashBalance;
        public decimal TotalCashBalance { get => _totalCashBalance; set { _totalCashBalance = value; OnPropertyChanged(); } }

        private decimal _totalMarketValue;
        public decimal TotalMarketValue { get => _totalMarketValue; set { _totalMarketValue = value; OnPropertyChanged(); } }

        private decimal _totalUnrealizedPL;
        public decimal TotalUnrealizedPL { get => _totalUnrealizedPL; set { _totalUnrealizedPL = value; OnPropertyChanged(); } }

        private decimal _totalUnrealizedPLPct;
        public decimal TotalUnrealizedPLPct { get => _totalUnrealizedPLPct; set { _totalUnrealizedPLPct = value; OnPropertyChanged(); } }

        private decimal _totalRealizedPL;
        public decimal TotalRealizedPL { get => _totalRealizedPL; set { _totalRealizedPL = value; OnPropertyChanged(); } }

        private string _message = "";
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }

        private DateTime? _fromDate = null;
        public DateTime? FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime? _toDate = null;
        public DateTime? ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private bool _isMarketLive = false;
        public bool IsMarketLive
        {
            get => _isMarketLive;
            set { _isMarketLive = value; OnPropertyChanged(); }
        }

        private string _currentNewsText = "Piyasalara dair güncel haber akışı bekleniyor...";
        public string CurrentNewsText
        {
            get => _currentNewsText;
            set { _currentNewsText = value; OnPropertyChanged(); }
        }

        private readonly List<string> _mockNews = new()
        {
            "📢 TCMB Faiz Kararını Açıkladı: Politika faizi sabit bırakıldı.",
            "🚀 BIST 100 endeksi günü rekor seviyeden tamamladı.",
            "🌍 Küresel piyasalarda gözler ABD tarım dışı istihdam verisine odaklandı.",
            "🔋 Teknoloji hisselerinde ralli devam ediyor.",
            "📉 Altın fiyatları ons başına 2.050 dolar seviyelerinde dengelendi.",
            "🏢 AŞ hisseleri güçlü bilanço sonrası %5 prim yaptı.",
            "🚢 Demir çelik sektöründe ihracat artışı beklentileri yükseltti."
        };
        private int _newsIndex = 0;
        private System.Timers.Timer _newsTimer;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ToggleMarketCommand { get; }

        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ToggleMarketCommand = new RelayCommand(ToggleMarket);
            _ = LoadAsync();

            LiveMarketService.Instance.MarketDataUpdated += async (s, e) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadAsync();
                });
            };

            // Setup News Feed Rotation Timer (Rotate every 5 seconds)
            _newsTimer = new System.Timers.Timer(5000);
            _newsTimer.Elapsed += (s, e) =>
            {
                _newsIndex = (_newsIndex + 1) % _mockNews.Count;
                CurrentNewsText = _mockNews[_newsIndex];
            };
            _newsTimer.Start();
        }

        private void ToggleMarket()
        {
            // The IsChecked two-way binding already flips IsMarketLive, so we don't need to flip it again manually.
            // But let's make sure the background service accurately maps to the property state.
            if (IsMarketLive)
            {
                LiveMarketService.Instance.Start();
                Message = "Simülatör: Çalışıyor (Fiyatlar Canlı Güncelleniyor)";
            }
            else
            {
                LiveMarketService.Instance.Stop();
                Message = "Simülatör: Durduruldu (Fiyatlar Sabit)";
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                Message = "";
                DateTime? fromUtc = FromDate?.Date.ToUniversalTime();
                DateTime? toUtc = ToDate?.Date.ToUniversalTime();
                
                var s = await _service.GetSummaryAsync(fromUtc, toUtc);

                ActiveCustomerCount = s.ActiveCustomerCount;
                TotalMarketValue = s.TotalMarketValue;
                TotalCashBalance = s.TotalCashBalance;
                TotalUnrealizedPL = s.TotalUnrealizedPL;
                TotalUnrealizedPLPct = s.TotalUnrealizedPLPct;
                TotalRealizedPL = s.TotalRealizedPL;

                var oldWinners = Winners.ToList();
                Winners.Clear();
                foreach (var p in s.TopWinners) 
                {
                    var old = oldWinners.FirstOrDefault(x => x.AssetId == p.AssetId && x.CustomerId == p.CustomerId);
                    if (old != null) p.PreviousPrice = old.CurrentPrice;
                    Winners.Add(p);
                }

                var oldLosers = Losers.ToList();
                Losers.Clear();
                foreach (var p in s.TopLosers) 
                {
                    var old = oldLosers.FirstOrDefault(x => x.AssetId == p.AssetId && x.CustomerId == p.CustomerId);
                    if (old != null) p.PreviousPrice = old.CurrentPrice;
                    Losers.Add(p);
                }

                // 1. Asset Distribution Pie Chart (Group by Asset Code)
                var distribution = s.AllPositions
                    .GroupBy(p => p.AssetCode)
                    .Select(g => new { Code = g.Key, TotalVal = g.Sum(x => x.MarketValue) })
                    .Where(x => x.TotalVal > 0)
                    .OrderByDescending(x => x.TotalVal)
                    .ToList();

                var newPieSeries = new List<ISeries>();
                foreach (var item in distribution)
                {
                    newPieSeries.Add(new PieSeries<decimal>
                    {
                        Values = new[] { item.TotalVal },
                        Name = item.Code,
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        DataLabelsFormatter = point => $"{item.Code}: {point.Coordinate.PrimaryValue:N0}"
                    });
                }
                
                // Avoid UI flickering by resetting the collection
                AssetDistributionSeries.Clear();
                foreach (var ser in newPieSeries) AssetDistributionSeries.Add(ser);

                // 2. PL Series (Top Winners + Top Losers as Columns)
                var plItems = s.TopWinners.Concat(s.TopLosers).ToList();
                var plValues = plItems.Select(x => (double)x.UnrealizedPL).ToArray();
                var plLabels = plItems.Select(x => $"{x.CustomerName}\n{x.AssetCode}").ToList();

                var newColSeries = new ColumnSeries<double>
                {
                    Values = plValues,
                    Name = "Unrealized P/L",
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue)
                };

                PLSeries.Clear();
                PLSeries.Add(newColSeries);
                XAxes[0].Labels = plLabels;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }
}
