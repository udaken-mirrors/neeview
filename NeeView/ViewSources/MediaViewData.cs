using System.Windows.Media;

namespace NeeView
{
    public class MediaViewData : IHasImageSource
    {
        public MediaViewData(MediaSource mediaSource, ImageSource? imageSource)
        {
            MediaSource = mediaSource;
            ImageSource = imageSource;
        }

        public MediaSource MediaSource { get; }
        public ImageSource? ImageSource { get; }
    }
}
