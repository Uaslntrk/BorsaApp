using BorsaApp.BLL.Services;
using BorsaApp.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BorsaApp.Wpf.ViewModels
{
    public class AlarmsViewModel : BaseViewModel
    {
        private readonly PriceAlarmService _service = new();
        private readonly CustomerService _customerService = new();

        public ObservableCollection<PriceAlarm> Alarms { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private PriceAlarm _form = new() { Direction = "Above", IsActive = true, IsAutoSell = false };
        public PriceAlarm Form
        {
            get => _form;
            set { _form = value; OnPropertyChanged(); }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand CreateCommand { get; }
        public RelayCommand DeleteCommand { get; }

        private PriceAlarm? _selected;
        public PriceAlarm? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public AlarmsViewModel()
        {
            LoadCommand = new RelayCommand(async () => await LoadAsync());
            CreateCommand = new RelayCommand(async () => await CreateAsync());
            DeleteCommand = new RelayCommand(async () => await DeleteAsync());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                Message = "";
                var list = await _service.GetAllAlarmsAsync();
                Alarms.Clear();
                foreach (var a in list) Alarms.Add(a);

                var customers = await _customerService.GetAllAsync();
                Customers.Clear();
                foreach (var c in customers) if (c.IsActive) Customers.Add(c);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private async Task CreateAsync()
        {
            try
            {
                Message = "";
                if (string.IsNullOrWhiteSpace(Form.AssetCode) || Form.TargetPrice <= 0)
                {
                    Message = "Lütfen geçerli bir hisse kodu ve hedef fiyat giriniz.";
                    return;
                }

                if (Form.IsAutoSell && SelectedCustomer == null)
                {
                    Message = "Otomatik işlem için bir müşteri seçmelisiniz.";
                    return;
                }

                await _service.AddAlarmAsync(new PriceAlarm
                {
                    AssetCode = Form.AssetCode.ToUpper(),
                    TargetPrice = Form.TargetPrice,
                    Direction = Form.Direction,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    IsAutoSell = Form.IsAutoSell,
                    CustomerId = SelectedCustomer?.Id ?? 0
                });

                Message = "Alarm eklendi!";
                Form = new PriceAlarm { Direction = "Above", IsActive = true, IsAutoSell = false };
                SelectedCustomer = null;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private async Task DeleteAsync()
        {
            if (Selected == null) return;
            try
            {
                Message = "";
                await _service.DeleteAlarmAsync(Selected.Id);
                Message = "Alarm silindi.";
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
    }
}
