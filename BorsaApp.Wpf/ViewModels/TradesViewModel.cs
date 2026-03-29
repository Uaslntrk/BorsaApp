using BorsaApp.BLL.Services;
using BorsaApp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsaApp.Wpf.ViewModels
{
  
    public class TradesViewModel : BaseViewModel
    {
        private readonly TradeService _service = new();

        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<Asset> Assets { get; } = new();
        public ObservableCollection<TradeListItem> Trades { get; } = new();

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

        private Asset? _selectedAsset;
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set 
            { 
                _selectedAsset = value; 
                if (_selectedAsset != null)
                {
                    Price = _selectedAsset.CurrentPrice;
                }
                OnPropertyChanged(); 
            }
        }
        private TradeListItem? _selectedTrade;
        public TradeListItem? SelectedTrade
        {
            get => _selectedTrade;
            set { _selectedTrade = value; OnPropertyChanged(); }
        }

        public RelayCommand CancelTradeCommand { get; }
        public List<string> BuySellOptions { get; } = new() { "BUY", "SELL" };

        private string _buySell = "BUY";
        public string BuySell
        {
            get => _buySell;
            set { _buySell = value; OnPropertyChanged(); }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        private decimal _price = 0;
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand SaveTradeCommand { get; }
        public RelayCommand ClearCommand { get; }

        private readonly TradeService _tradeService = new();
        public TradesViewModel()
        {
            CancelTradeCommand = new RelayCommand(async () => await CancelSelectedAsync());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            SaveTradeCommand = new RelayCommand(async () => await SaveAsync());
            ClearCommand = new RelayCommand(ClearForm);

            _ = LoadAsync();
        }

        private async Task CancelSelectedAsync()
        {
            try
            {
                if (SelectedTrade is null)
                {
                    Message = "İptal edilecek işlem seç.";
                    return;
                }

                if (SelectedTrade.IsCancelled)
                {
                    Message = "Bu işlem zaten iptal edilmiş.";
                    return;
                }

                var result = System.Windows.MessageBox.Show(
                    $"Trade #{SelectedTrade.Id} iptal edilsin mi?\n{SelectedTrade.CustomerName} - {SelectedTrade.AssetCode} {SelectedTrade.BuySell} {SelectedTrade.Quantity}",
                    "Trade İptal",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result != System.Windows.MessageBoxResult.Yes) return;

                var reason = Microsoft.VisualBasic.Interaction.InputBox(
                    "İptal nedeni (opsiyonel):",
                    "Trade İptal",
                    "");

                await _tradeService.CancelTradeAsync(SelectedTrade.Id, reason, actor: "system");

                await LoadAsync();
                Message = "Trade iptal edildi.";
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
                Message = "Yüklendi";

                Customers.Clear();
                foreach (var c in await _service.GetCustomersAsync())
                   Customers.Add(c);

                Assets.Clear();
                foreach (var a in await _service.GetAssetsAsync())
                    Assets.Add(a);

                DateTime? fromUtc = FromDate?.Date.ToUniversalTime();
                DateTime? toUtc = ToDate?.Date.ToUniversalTime(); // ToDate gün sonu yerine ertesi gün 00:00 gibi düşün
                Trades.Clear();
                foreach (var t in await _service.GetLatestTradesAsync(fromUtc, toUtc))
                    Trades.Add(t);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ClearForm()
        {
            SelectedCustomer = null;
            SelectedAsset = null;
            BuySell = "BUY";
            Quantity = 1;
            Price = 0;
            Message = "";
        }

        private async Task SaveAsync()
        {
            try
            {
                Message = "";
                var t = new Trade
                {
                    CustomerId = SelectedCustomer?.Id ?? 0,
                    AssetId = SelectedAsset?.Id ?? 0,
                    BuySell = BuySell,
                    Quantity = Quantity,
                    Price = Price
                };

                var id = await _service.CreateTradeAsync(t);
                Message = $"İşlem kaydedildi (Id: {id})";
                await LoadAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }

}
