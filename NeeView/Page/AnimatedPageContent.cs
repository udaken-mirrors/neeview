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

        private PageContentType _contentType = PageContentType.None;

        public AnimatedPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                // 初回アニメーション判定
                if (_contentType == PageContentType.None)
                {
                    using var stream = Entry.OpenEntry();
                    var bitmapInfo = BitmapInfo.Create(stream); // TODO: async
                    _contentType = bitmapInfo.FrameCount > 1 ? PageContentType.Animated : PageContentType.Bitmap;
                }

                // アニメーション画像
                if (_contentType == PageContentType.Animated) 
                {
                    // fileProxy
                    var fileProxy = Entry.GetFileProxy(); // TODO: async化
                    var entry = ArchiveEntryUtility.CreateTemporaryEntry(fileProxy.Path);

                    // pictureInfo
                    using var stream = entry.OpenEntry();
                    var bitmapInfo = BitmapInfo.Create(stream); // TODO: async
                    var pictureInfo = PictureInfo.Create(bitmapInfo, "MediaPlayer");
                    return new PageSource(new AnimatedPageData(fileProxy.Path), null, pictureInfo);
                }
                // 通常画像
                else
                {
                    var loader = new BitmapPageContentLoader(Entry);
                    var createPictureInfo = PictureInfo is null;
                    var imageData = await loader.LoadAsync(createPictureInfo, token);
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

}