using System;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IImageExporter : IDisposable
    {
        ImageExporterContent? CreateView(ImageExporterCreateOptions options);
        void Export(string path, bool isOverwrite, int qualityLevel, ImageExporterCreateOptions options);
        string CreateFileName();
    }

    public class ImageExporterCreateOptions
    {
        public bool HasBackground { get; set; }
        public bool IsOriginalSize { get; set; }
        public bool IsDotKeep { get; set; }
    }
}
