using NeeView.PageFrames;

namespace NeeView
{
    public static class ViewContentSizeFactory
    {
        public static ViewContentSize Create(PageFrameElement element, PageFrameElementScale scale)
        {
            return element.Page.Content switch
            {
                AnimatedPageContent => new BitmapViewContentSize(element, scale),
                BitmapPageContent => new BitmapViewContentSize(element, scale),
                MediaPageContent => new MediaViewContentSize(element, scale),
                _ => new ViewContentSize(element, scale),
            };
        }
    }
}
