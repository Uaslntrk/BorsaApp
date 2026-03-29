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
    public class CustomersViewModel : BaseViewModel
    {
        private readonly CustomerService _service = new();

        public ObservableCollection<Customer> Customers { get; } = new();

        private Customer _form = new() { RiskLevel = "Orta", IsActive = true };
        public Customer Form
        {
            get => _form;
            set { _form = value; OnPropertyChanged(); }
        }

        private Customer? _selected;
        public Customer? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
                if (_selected is not null)
                {
                    // seçileni forma kopyala
                    Form = new Customer
                    {
                        Id = _selected.Id,
                        Name = _selected.Name,
                        TcNo = _selected.TcNo,
                        RiskLevel = _selected.RiskLevel,
                        IsActive = _selected.IsActive,
                        CashBalance = _selected.CashBalance
                    };
                }
                UpdateCommands();
            }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand CreateCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public CustomersViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ClearCommand = new RelayCommand(ClearForm);
            CreateCommand = new RelayCommand(async () => await CreateAsync());
            UpdateCommand = new RelayCommand(async () => await UpdateAsync(), () => Selected is not null);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => Selected is not null);

            _ = LoadAsync();
        }

        private void UpdateCommands()
        {
            UpdateCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        private async Task LoadAsync()
        {
            try
            {
                Message = "";
                Customers.Clear();
                var list = await _service.GetAllAsync();
                foreach (var c in list) Customers.Add(c);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ClearForm()
        {
            Selected = null;
            Form = new Customer { RiskLevel = "Orta", IsActive = true, CashBalance = 100000m };
            Message = "";
            UpdateCommands();
        }

        private async Task CreateAsync()
        {
            try
            {
                Message = "";
                var id = await _service.CreateAsync(Form);
                Message = $"Eklendi (Id: {id})";
                await LoadAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private async Task UpdateAsync()
        {
            if (Selected is null) return;
            try
            {
                Message = "";
                await _service.UpdateAsync(Form);
                Message = "Güncellendi";
                await LoadAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private async Task DeleteAsync()
        {
            if (Selected is null) return;
            try
            {
                Message = "";
                await _service.SoftDeleteAsync(Selected.Id);
                Message = "Pasif yapıldı";
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
