using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace NeeView
{
    public class BitmapPageContent : PageContent
    {
        private readonly BitmapPageContentLoader _loader;

        public BitmapPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
            _loader = new BitmapPageContentLoader();
        }


        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                token.ThrowIfCancellationRequested();

#if DEBUG
                if (Debugger.IsAttached)
                {
                    //Debug.WriteLine($"Loading...: {ArchiveEntry}");
                    await Task.Delay(200, token);
                    NVDebug.AssertMTA();
                }
#endif

                var streamSource = new ArchiveEntryStreamSource(ArchiveEntry);
                streamSource.CreateCache();

                var createPictureInfo = PictureInfo is null;
                var imageData = await _loader.LoadAsync(streamSource, createPictureInfo, token);
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