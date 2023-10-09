using System.Windows.Media;

namespace NeeView
{
    public class ArchiveViewData
    {
        public ArchiveViewData(ArchiveEntry entry, ThumbnailBitmap thumbnail, ImageSource? imageSource)
        {
            Entry = entry;
            Thumbnail = thumbnail;
            ImageSource = imageSource;
        }

        public ArchiveEntry Entry { get; }
        public ThumbnailBitmap Thumbnail { get; }
        public ImageSource? ImageSource { get; }
    }
}