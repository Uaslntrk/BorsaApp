using BorsaApp.BLL.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BorsaApp.Wpf
{
    public partial class MainWindow : Window
    {
        private static readonly string[] PageTitles =
        {
            "Müşteri Yönetimi",
            "Enstrüman Yönetimi",
            "İşlem Yönetimi",
            "Portföy Görünümü",
            "Realized P/L Raporu",
            "Fiyat Alarmları",
            "Dashboard"
        };

        public MainWindow()
        {
            InitializeComponent();
            LiveMarketService.Instance.PriceAlarmTriggered += Instance_PriceAlarmTriggered;
            LiveMarketService.Instance.Start();
            TopBarDate.Text = DateTime.Now.ToString("dd MMM yyyy  HH:mm");

            // Select dashboard by default
            MainTabControl.SelectedIndex = 6;
        }

        public MainWindow(string role, int userId) : this()
        {
            if (role != "Admin")
            {
                TabMusteriler.Visibility = Visibility.Collapsed;
                NavMusteriler.Visibility = Visibility.Collapsed;
            }
        }

        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tagStr && int.TryParse(tagStr, out int idx))
            {
                if (MainTabControl == null) return;
                MainTabControl.SelectedIndex = idx;
                if (PageTitleText != null && idx < PageTitles.Length)
                    PageTitleText.Text = PageTitles[idx];
                TryRefresh(MainTabControl.SelectedContent);
            }
        }

        private static void TryRefresh(object? content)
        {
            if (content is FrameworkElement fe && fe.DataContext != null)
            {
                var prop = fe.DataContext.GetType().GetProperty("RefreshCommand");
                if (prop?.GetValue(fe.DataContext) is ICommand cmd && cmd.CanExecute(null))
                    cmd.Execute(null);
            }
        }

        private void Instance_PriceAlarmTriggered(object? sender, Entities.PriceAlarmEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                string msg = e.Alarm.Direction == "Above"
                    ? $"🚨 ALARM! {e.Alarm.AssetCode} hedef fiyatı AŞTI! (Anlık: {e.TriggeredPrice:N2})"
                    : $"🚨 ALARM! {e.Alarm.AssetCode} hedef fiyatın ALTINA DÜŞTÜ! (Anlık: {e.TriggeredPrice:N2})";
                await ShowToastNotification(msg);
            });
        }

        private async System.Threading.Tasks.Task ShowToastNotification(string message)
        {
            ToastMessageText.Text = message;
            ToastNotificationBorder.Visibility = Visibility.Visible;
            await System.Threading.Tasks.Task.Delay(5000);
            ToastNotificationBorder.Visibility = Visibility.Collapsed;
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeToggleButton.IsChecked == true)
            {
                ThemeToggleButton.Content = "☀️  Açık Tema";
                SwitchTheme("DarkTheme");
            }
            else
            {
                ThemeToggleButton.Content = "🌙  Koyu Tema";
                SwitchTheme("LightTheme");
            }
        }

        private void SwitchTheme(string themeName)
        {
            var app = Application.Current;
            var themeDict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/BorsaApp.Wpf;component/Themes/{themeName}.xaml")
            };
            var stylesDict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/BorsaApp.Wpf;component/Themes/GlobalStyles.xaml")
            };
            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(themeDict);
            app.Resources.MergedDictionaries.Add(stylesDict);
        }
    }
}
