using Microsoft.Win32;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BorsaApp.Wpf.Helpers
{
    public static class ExcelExporter
    {
        public static string? AskSavePath(string defaultName)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = defaultName,
                AddExtension = true,
                DefaultExt = "xlsx"
            };

            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public static void SaveWorkbook(string path, Action<XLWorkbook> build)
        {
            try
            {
                using var wb = new XLWorkbook();
                build(wb);
                wb.SaveAs(path);
                MessageBox.Show("Excel oluşturuldu ✅", "BorsaApp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Excel Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void StyleHeader(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.LightGray;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }
}
