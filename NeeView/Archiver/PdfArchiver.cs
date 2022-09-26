using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public abstract class PdfArchiver : Archiver
    {
        protected PdfArchiver(string path, ArchiveEntry? source) : base(path, source)
        {
        }

        public abstract Size GetSourceSize(ArchiveEntry entry);
        public abstract System.Drawing.Image CraeteBitmap(ArchiveEntry entry, Size size);
        public abstract BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size);
        public abstract byte[] CreateBitmapData(ArchiveEntry entry, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality);
    }
}
