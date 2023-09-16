using System.Windows.Media;

namespace NeeView
{
    public class MediaViewData : IHasImageSource
    {
        public MediaViewData(string path, ImageSource? imageSource)
        {
            Path = path;
            ImageSource = imageSource;
        }

        public string Path { get; }
        public ImageSource? ImageSource { get; }
    }
}
