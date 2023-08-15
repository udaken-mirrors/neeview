using System.ComponentModel;

namespace NeeView.PageFrames
{
    public enum PageFrameAlignment
    {
        Min,
        Center,
        Max,
    }

    public static class PageFrameAlignExtensions
    {
        public static PageFrameAlignment Reverse(this PageFrameAlignment alignment)
        {
            return alignment switch
            {
                PageFrameAlignment.Min => PageFrameAlignment.Max,
                PageFrameAlignment.Center => PageFrameAlignment.Center,
                PageFrameAlignment.Max => PageFrameAlignment.Min,
                _ => throw new InvalidEnumArgumentException(nameof(alignment)),
            };
        }
    }
}
