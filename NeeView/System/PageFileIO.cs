using NeeLaboratory.Linq;
using NeeView.Properties;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Effects;


namespace NeeView
{
    public static class PageFileIO
    {
        /// <summary>
        /// ページ削除済？
        /// </summary>
        public static bool IsPageRemoved(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;

            var path = page.GetFilePlace();
            return !(File.Exists(path) || Directory.Exists(path));
        }

        /// <summary>
        /// ページ削除可能？
        /// </summary>
        public static bool CanRemovePage(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;

            var path = page.GetFilePlace();
            return (File.Exists(path) || Directory.Exists(path));
        }

        /// <summary>
        /// ページ削除可能？
        /// </summary>
        public static bool CanRemovePage(List<Page> pages)
        {
            return pages.All(e => CanRemovePage(e));
        }

        /// <summary>
        /// ページファイル削除
        /// </summary>
        public static async Task<bool> RemovePageAsync(Page page)
        {
            if (page == null) return false;

            var path = page.GetFilePlace();
            if (path is null) return false;

            var thumbnail = await CreatePageVisualAsync(page);
            return await FileIO.RemoveFileAsync(path, Resources.FileDeletePageDialog_Title, thumbnail);
        }

        /// <summary>
        /// ページファイル削除
        /// </summary>
        public static async Task<bool> RemovePageAsync(List<Page> pages)
        {
            if (pages == null || pages.Count == 0)
            {
                return false;
            }

            if (pages.Count == 1)
            {
                return await RemovePageAsync(pages.First());
            }
            else
            {
                var files = pages
                    .Select(e => e.GetFilePlace())
                    .WhereNotNull()
                    .ToList();
                return await FileIO.RemoveFileAsync(files, Resources.FileDeletePageDialog_Title);
            }
        }

        /// <summary>
        /// ページからダイアログ用サムネイル作成
        /// </summary>
        public static async Task<Image> CreatePageVisualAsync(Page page)
        {
            var imageSource = await page.LoadThumbnailAsync(CancellationToken.None);

            var image = new Image();
            image.Source = imageSource;
            image.Effect = new DropShadowEffect()
            {
                Opacity = 0.5,
                ShadowDepth = 2,
                RenderingBias = RenderingBias.Quality
            };
            image.MaxWidth = 96;
            image.MaxHeight = 96;

            return image;
        }
    }
}
