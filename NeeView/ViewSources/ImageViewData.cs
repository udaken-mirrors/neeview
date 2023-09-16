using System.Windows.Media;

namespace NeeView
{
    public class ImageViewData : IHasImageSource
    {
        public ImageViewData(ImageSource imageSource)
        {
            ImageSource = imageSource;
        }

        public ImageSource ImageSource { get; }
    }
}
