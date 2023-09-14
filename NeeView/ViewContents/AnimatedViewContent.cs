using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class AnimatedViewContent : MediaViewContent, IHasImageSource
    {
        public AnimatedViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
        }
    }

}
