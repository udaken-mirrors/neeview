using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class AnimatedViewContent : MediaViewContent, IHasImageSource
    {
        public AnimatedViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource) : base(element, scale, viewSource, activity, backgroundSource)
        {
            MediaStartDelay = TimeSpan.FromMilliseconds(16);
        }
    }

}
