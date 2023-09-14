using NeeView.PageFrames;

namespace NeeView
{
    public class BitmapViewContent : ImageViewContent
    {
        public BitmapViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
        }
    }
}
