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
    public class AssetsViewModel : BaseViewModel
    {
        private readonly AssetService _service = new();

        public ObservableCollection<Asset> Assets { get; } = new();

        private Asset _form = new() { CurrentPrice = 0 };
        public Asset Form
        {
            get => _form;
            set { _form = value; OnPropertyChanged(); }
        }

        private Asset? _selected;
        public Asset? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();

                if (_selected is not null)
                {
                    Form = new Asset
                    {
                        Id = _selected.Id,
                        Code = _selected.Code,
                        Name = _selected.Name,
                        Sector = _selected.Sector,
                        CurrentPrice = _selected.CurrentPrice
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

        public AssetsViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ClearCommand = new RelayCommand(ClearForm);
            CreateCommand = new RelayCommand(async () => await CreateAsync());
            UpdateCommand = new RelayCommand(async () => await UpdateAsync(), () => Selected is not null);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => Selected is not null);

            _ = LoadAsync();

            LiveMarketService.Instance.MarketDataUpdated += async (s, e) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadAsync();
                });
            };
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
                var list = await _service.GetAllAsync();
                var toRemove = Assets.Where(a => !list.Any(l => l.Id == a.Id)).ToList();
                foreach (var rm in toRemove) Assets.Remove(rm);

                foreach (var a in list)
                {
                    var existing = Assets.FirstOrDefault(x => x.Id == a.Id);
                    if (existing != null)
                    {
                        existing.Code = a.Code;
                        existing.Name = a.Name;
                        existing.Sector = a.Sector;
                        existing.CurrentPrice = a.CurrentPrice;
                    }
                    else
                    {
                        Assets.Add(a);
                    }
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ClearForm()
        {
            Selected = null;
            Form = new Asset { CurrentPrice = 0 };
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
                await _service.DeleteAsync(Selected.Id);
                Message = "Silindi";
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
