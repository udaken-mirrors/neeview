using NeeView.Media.Imaging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class OriginalImageExporter : IImageExporter
    {
        private readonly ExportImageSource _source;
        private readonly Page _page;

        public bool HasBackground { get; set; }

        public OriginalImageExporter(ExportImageSource source)
        {
            _source = source;
            _page = _source?.Pages?.FirstOrDefault() ?? throw new ArgumentException("source must have any page");
        }

        public ImageExporterContent? CreateView()
        {
            if (_page == null) return null;

            try
            {
                var imageSource = Task.Run(() =>_page.LoadThumbnailAsync(CancellationToken.None)).Result;

                var image = new Image();
                image.Source = imageSource;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

                return new ImageExporterContent(image, Size.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public void Export(string path, bool isOverwrite, int qualityLevel)
        {
            _page.ArchiveEntry.ExtractToFile(path, isOverwrite);
        }

        public string CreateFileName()
        {
            return LoosePath.ValidFileName(_page.EntryLastName);
        }
    }
}
