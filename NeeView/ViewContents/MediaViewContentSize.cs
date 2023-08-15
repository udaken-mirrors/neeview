using NeeView.PageFrames;
using System.Windows;


namespace NeeView
{
    public class MediaViewContentSize : ViewContentSize
    {
        public MediaViewContentSize(PageFrameElement element, PageFrameElementScale scale) : base(element, scale)
        {
        }

        public override Size GetPictureSize()
        {
            return SourceSize;
        }
    }


}
