using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace NeeView
{
    public class AppSettings
    {
        private static AppSettings? _current;
        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                {
                    var fileName = "/NeeView.settings.json";
                    var resource_uri = new Uri(fileName, UriKind.Relative);
                    var info = Application.GetContentStream(resource_uri) ?? throw new FileNotFoundException($"File not found: {fileName}");
                    using var stream = info.Stream;
                    _current = JsonSerializer.Deserialize<AppSettings>(stream, UserSettingTools.GetSerializerOptions());
                    if (_current is null) throw new FormatException($"Cannot read: {fileName}");
                }
                return _current;
            }
        }


        // e.g. Dev
        public string PackageType { get; set; } = "Dev";
        // e.g. false
        public bool SelfContained { get; set; }
        // e.g. true
        public bool Watermark { get; set; }
        // e.g. false
        public bool UseLocalApplicationData { get; set; }
        // e.g. 12345678
        public string Revision { get; set; } = "??";
        // e.g. 0000
        public string DateVersion { get; set; } = "??";
        // e.g. Pdfium
        public string? PdfRenderer { get; set; }
        // e.g. TraceLog.txt
        public string? LogFile { get; set; }
        // e.g. 36.2
        public string? CheckVersion { get; set; }
        // e.g. https://neelabo.bitbucket.io/NeeViewUpdateCheck.html
        public string? DistributionUrl { get; set; }
    }

}
