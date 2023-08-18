using System;

namespace NeeView
{
    public enum PageFrameOrientation
    {
        [AliasName]
        Horizontal,

        [AliasName]
        Vertical
    }

    public static class PageFrameOrientationExtension
    {
        public static PageFrameOrientation GetToggle(this PageFrameOrientation mode)
        {
            return (PageFrameOrientation)(((int)mode + 1) % Enum.GetNames(typeof(PageFrameOrientation)).Length);
        }
    }
}
