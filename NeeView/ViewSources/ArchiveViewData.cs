using System.Windows.Media;

namespace NeeView
{
    public class ArchiveViewData
    {
        public ArchiveViewData(ArchiveEntry entry, ImageSource? imageSource, ImageSource? iconSource)
        {
            Entry = entry;
            ImageSource = imageSource;
            IconSource = iconSource;
        }

        public ArchiveEntry Entry { get; }
        public ImageSource? ImageSource { get; }
        public ImageSource? IconSource { get; }
    }
}
