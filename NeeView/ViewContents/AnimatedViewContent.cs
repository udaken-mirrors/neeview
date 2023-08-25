using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class AnimatedViewContent : MediaViewContent, IHasImageSource
    {
        public AnimatedViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity) : base(element, scale, viewSource, activity)
        {
            MediaStartDelay = TimeSpan.FromMilliseconds(16);
        }
    }

}
