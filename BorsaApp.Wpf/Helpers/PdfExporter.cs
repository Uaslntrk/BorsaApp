using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BorsaApp.Wpf.Helpers
{
    public static class PdfExporter
    {
        public static string? AskSavePath(string defaultName)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = defaultName,
                AddExtension = true,
                DefaultExt = "pdf"
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public static void Save(string path, IDocument doc)
        {
            try
            {
                // QuestPDF: path verirsen o path'e kaydeder :contentReference[oaicite:3]{index=3}
                doc.GeneratePdf(path);
                MessageBox.Show("PDF oluşturuldu ✅", "BorsaApp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PDF Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
