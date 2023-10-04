using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedViewSourceStrategy : IViewSourceStrategy
    {
        private readonly PageContent _pageContent;


        public AnimatedViewSourceStrategy(PageContent pageContent)
        {
            _pageContent = pageContent;
        }


        public async Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            if (data.Data is not AnimatedPageData pageData) throw new InvalidOperationException(nameof(data.Data));

            // TODO: この画像が何度も読み込まれてないか調査すること
            var image = LoadImage(pageData.MediaSource);
            await Task.CompletedTask;

            // 色情報とBPP設定。
            if (image is not null)
            {
                _pageContent.PictureInfo?.SetPixelInfo(image);
            }

            var viewData = new AnimatedViewData(pageData.MediaSource, image);
            return new DataSource(viewData, 0, null);
        }


        // TODO: Async
        private BitmapImage? LoadImage(MediaSource mediaSource)
        {
            try
            {
                using (var stream = mediaSource.OpenStream())
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoadFailed: {ex.Message}");
                return null;
            }
        }
    }
}
