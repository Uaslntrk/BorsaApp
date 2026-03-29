using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BorsaApp.Entities
{
    public class PortfolioPosition : INotifyPropertyChanged
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        public int AssetId { get; set; }
        public string AssetCode { get; set; } = "";
        public string AssetName { get; set; } = "";

        public int NetQty { get; set; }
        public decimal AvgCost { get; set; }          // ortalama maliyet
        
        private decimal _currentPrice;
        public decimal CurrentPrice 
        { 
            get => _currentPrice; 
            set 
            { 
                if (_currentPrice != 0) 
                    PreviousPrice = _currentPrice;
                _currentPrice = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsPriceUp));
                OnPropertyChanged(nameof(IsPriceDown));
            } 
        }
        
        public decimal PreviousPrice { get; set; }
        public bool IsPriceUp => CurrentPrice > PreviousPrice && PreviousPrice > 0;
        public bool IsPriceDown => CurrentPrice < PreviousPrice && PreviousPrice > 0;

        private decimal _costValue;
        public decimal CostValue { get => _costValue; set { _costValue = value; OnPropertyChanged(); } }        // NetQty * AvgCost
        
        private decimal _marketValue;
        public decimal MarketValue { get => _marketValue; set { _marketValue = value; OnPropertyChanged(); } }      // NetQty * CurrentPrice
        
        private decimal _unrealizedPL;
        public decimal UnrealizedPL { get => _unrealizedPL; set { _unrealizedPL = value; OnPropertyChanged(); } }     // MarketValue - CostValue
        
        private decimal _unrealizedPLPct;
        public decimal UnrealizedPLPct { get => _unrealizedPLPct; set { _unrealizedPLPct = value; OnPropertyChanged(); } }  // % (CostValue>0)

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
