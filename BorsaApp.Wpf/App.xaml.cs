using Microsoft.Extensions.Configuration;
using QuestPDF.Infrastructure;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using WpfLibrary1;

namespace BorsaApp.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;

            base.OnStartup(e);

            var config = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .Build();

            // ✅ Unhandled exception yakala - uygulama kapanmasın
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            var cs = config.GetConnectionString("Default")
                     ?? throw new InvalidOperationException("ConnectionStrings:Default bulunamadı.");

            Db.Init(cs);

            try
            {
                BorsaApp.DAL.DatabaseInitializer.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı başlatılırken hata oluştu: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Beklenmeyen Hata",
                MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true; // ✅ kapanmayı engeller
        }
    }

}
