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
        public MainWindow()
        {
            InitializeComponent();
            LiveMarketService.Instance.PriceAlarmTriggered += Instance_PriceAlarmTriggered;
            LiveMarketService.Instance.Start();
        }

        public MainWindow(string role, int userId) : this()
        {
            if (role != "Admin")
                TabMusteriler.Visibility = Visibility.Collapsed;
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
                ThemeToggleButton.Content = "☀️ Açık";
                SwitchTheme("DarkTheme");
            }
            else
            {
                ThemeToggleButton.Content = "🌙 Koyu";
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

        // ─── Auto-Refresh on Tab Change ───────────────────────────────────────
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Find the TabControl in the visual tree and subscribe
            var tab = FindTabControl(this);
            if (tab != null)
                tab.SelectionChanged += TabControl_SelectionChanged;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard: only process direct TabControl selection changes, not nested grids/lists
            if (e.Source is not TabControl tc) return;
            if (tc.SelectedItem is not TabItem selectedTab) return;

            // The TabItem.Content is a UserControl — get its DataContext
            var view = selectedTab.Content as FrameworkElement;
            if (view?.DataContext == null) return;

            TryRefresh(view.DataContext);
        }

        private static void TryRefresh(object dataContext)
        {
            // Find the RefreshCommand property via reflection and execute it
            var prop = dataContext.GetType().GetProperty("RefreshCommand");
            if (prop == null) return;

            if (prop.GetValue(dataContext) is ICommand cmd && cmd.CanExecute(null))
                cmd.Execute(null);
        }

        private static TabControl? FindTabControl(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TabControl tc) return tc;
                var found = FindTabControl(child);
                if (found != null) return found;
            }
            return null;
        }
    }
}
