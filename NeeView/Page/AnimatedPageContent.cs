using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedPageContent : PageContent
    {
        enum PageContentType
        {
            None,
            Bitmap,
            Animated,
        }

        private readonly AnimatedImageType _imageType;
        private PageContentType _contentType = PageContentType.None;

        public AnimatedPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService, AnimatedImageType imageType) : base(archiveEntry, bookMemoryService)
        {
            _imageType = imageType;
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                var streamSource = new ArchiveEntryStreamSource(ArchiveEntry);

                // 初回アニメーション判定
                if (_contentType == PageContentType.None)
                {
                    using var stream = streamSource.OpenStream();
                    _contentType = AnimatedImageChecker.IsAnimatedImage(stream, _imageType) ? PageContentType.Animated : PageContentType.Bitmap;
                }

                // アニメーション画像
                if (_contentType == PageContentType.Animated)
                {
                    // pictureInfo
                    using var stream = streamSource.OpenStream();
                    var bitmapInfo = BitmapInfo.Create(stream); // TODO: async
                    var pictureInfo = PictureInfo.Create(bitmapInfo, "AnimatedImage");
                    return new AnimatedPageSource(new AnimatedPageData(new MediaSource(streamSource)), null, pictureInfo);
                }
                // 通常画像
                else
                {
                    var loader = new BitmapPageContentLoader(ArchiveEntry);
                    var createPictureInfo = PictureInfo is null;
                    var imageData = await loader.LoadAsync(streamSource, createPictureInfo, token);
                    return imageData;
                }

            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }
    }


    /// <summary>
    /// AnimationPage 用データソース
    /// </summary>
    /// TODO: キャッシュサイズ取得だけなので汎用化できそう
    public class AnimatedPageSource : PageSource
    {
        public AnimatedPageSource(object? data, string? errorMessage, PictureInfo? pictureInfo) : base(data, errorMessage, pictureInfo)
        {
        }

        public override long DataSize => (Data as IHasCache)?.CacheSize ?? 0;
    }

}