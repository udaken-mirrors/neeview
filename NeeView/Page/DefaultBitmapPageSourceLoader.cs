using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DefaultBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async Task<BitmapPageSource> LoadAsync(ArchiveEntry entry, bool createPictureInfo, CancellationToken token)
        {
            try
            {
                var streamSource = new ArchiveEntryStreamSource(entry);
                var pictureInfo = createPictureInfo ? PictureInfo.Create(streamSource, ".NET BitmapImage") : null; // TODO: Async
                await Task.CompletedTask;
                return BitmapPageSource.Create(new BitmapPageData(streamSource), pictureInfo, this);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return BitmapPageSource.CreateError(ex.Message);
            }
        }
    }
}
