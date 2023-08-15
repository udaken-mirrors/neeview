using System;

namespace NeeView
{
    // 見開き時のページ並び
    public enum PageReadOrder
    {
        [AliasName]
        RightToLeft,

        [AliasName]
        LeftToRight,
    }

    public static class PageReadOrderExtensions
    {
        public static PageReadOrder GetToggle(this PageReadOrder mode)
        {
            return (PageReadOrder)(((int)mode + 1) % Enum.GetNames(typeof(PageReadOrder)).Length);
        }

        [Obsolete]
        public static int ToDirection(this PageReadOrder mode)
        {
            return mode == PageReadOrder.LeftToRight ? -1 : 1;
        }

        public static int ToSign(this PageReadOrder self)
        {
            return self == PageReadOrder.LeftToRight ? 1 : -1;
        }
    }
}
