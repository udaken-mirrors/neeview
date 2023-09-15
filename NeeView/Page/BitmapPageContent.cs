using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace NeeView
{
    public class BitmapPageContent : PageContent
    {
        private BitmapPageContentLoader _loader;

        public BitmapPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
            _loader = new BitmapPageContentLoader(archiveEntry);
        }


        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                //Debug.WriteLine($"Loading...: {ArchiveEntry}");
#if DEBUG
                if (Debugger.IsAttached)
                {
                    NVDebug.AssertMTA();
                    await Task.Delay(200, token);
                }
#endif
                NVDebug.AssertMTA();
                var createPictureInfo = PictureInfo is null;
                var imageData = await _loader.LoadAsync(createPictureInfo, token);
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