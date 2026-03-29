using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BorsaApp.Wpf.Helpers
{
    public static class LogoLoader
    {
        // Örn: "Assets/logo.png"
        public static byte[] LoadResourceBytes(string relativePackPath)
        {
            var uri = new Uri($"pack://application:,,,/{relativePackPath}", UriKind.Absolute);
            var streamInfo = Application.GetResourceStream(uri)
                ?? throw new FileNotFoundException($"Logo bulunamadı: {relativePackPath}");

            using var s = streamInfo.Stream;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
