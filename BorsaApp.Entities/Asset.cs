using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BorsaApp.Entities
{
    public class Asset : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _code = "";
        public string Code { get => _code; set { _code = value; OnPropertyChanged(); } }

        private string _name = "";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string? _sector;
        public string? Sector { get => _sector; set { _sector = value; OnPropertyChanged(); } }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
