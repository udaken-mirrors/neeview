using System;

namespace NeeView
{
    // ページ表示モード
    public enum PageMode
    {
        [AliasName]
        SinglePage,

        [AliasName]
        WidePage,

        [AliasName]
        Panorama,
    }


    public static class PageModeExtension
    {
        public static PageMode GetToggle(this PageMode mode)
        {
            return (PageMode)(((int)mode + 1) % Enum.GetNames(typeof(PageMode)).Length);
        }

        public static int Size(this PageMode mode)
        {
            return mode == PageMode.WidePage ? 2 : 1;
        }

        public static PageMode Validate(this PageMode mode)
        {
            if (mode < PageMode.SinglePage) return PageMode.SinglePage;
            if (mode > PageMode.Panorama) return PageMode.Panorama;
            return mode;
        }
    }
}
