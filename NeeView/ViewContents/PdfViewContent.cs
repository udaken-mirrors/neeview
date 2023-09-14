using NeeView.PageFrames;

namespace NeeView
{
    public class PdfViewContent : ImageViewContent
    {
        public PdfViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
        }
    }
}
