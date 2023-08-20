using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace NeeView
{

    public class BitmapPageContent : PageContent
    {
        private IBitmapPageSourceLoader? _imageDataLoader;

        public BitmapPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            try
            {
                //Debug.WriteLine($"Loading...: {ArchiveEntry}");
#if DEBUG
                if (Debugger.IsAttached)
                {
                    NVDebug.AssertMTA();
                    await Task.Delay(200, token);
                }
#endif
                NVDebug.AssertMTA();
                var loader = _imageDataLoader ?? new BitmapPageSourceLoader();
                var createPictureInfo = PictureInfo is null;
                var imageData = await loader.LoadAsync(Entry, createPictureInfo, token);
                _imageDataLoader = imageData.ImageDataLoader;
                return imageData;
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
