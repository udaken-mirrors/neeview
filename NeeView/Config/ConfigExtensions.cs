using System;

namespace NeeView
{
    public static class ConfigExtensions
    {
        /// <summary>
        /// ページモードのフレームページ数取得
        /// </summary>
        public static int GetFramePageSize(this Config config, PageMode pageMode)
        {
            return pageMode switch
            {
                PageMode.SinglePage
                    => 1,
                PageMode.WidePage
                    => 2,
                PageMode.Panorama
                    => config.Book.IsTwoPagePanorama ? 2 : 1,
                _
                    => throw new NotSupportedException()
            };
        }
    }
}

