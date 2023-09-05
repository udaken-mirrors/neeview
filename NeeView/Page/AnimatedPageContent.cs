using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedPageContent : PageContent
    {
        public AnimatedPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService) : base(archiveEntry, bookMemoryService)
        {
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                // fileProxy
                var fileProxy = Entry.GetFileProxy(); // TODO: async化
                var entry = ArchiveEntryUtility.CreateTemporaryEntry(fileProxy.Path);

                // pictureInfo
                var factory = new DefaultBitmapFactory();
                using (var stream = entry.OpenEntry())
                {
                    var bitmapInfo = BitmapInfo.Create(stream); // TODO: async
                    var pictureInfo = PictureInfo.Create(bitmapInfo, "MediaPlayer");
                    
                    await Task.CompletedTask;
                    
                    return new PageSource(fileProxy.Path, null, pictureInfo);
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

        public class AnimatedPageSource
        {
            public AnimatedPageSource(string path, BitmapImage bitmapImage)
            {
                Path = path;
                this.bitmapImage = bitmapImage;
            }

            public string Path { get; }
            public BitmapImage bitmapImage { get; }
        }
    }
}