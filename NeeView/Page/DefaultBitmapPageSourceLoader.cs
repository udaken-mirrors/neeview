using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DefaultBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async Task<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, bool createSource, CancellationToken token)
        {
            try
            {
                var pictureInfo = createPictureInfo ? PictureInfo.Create(streamSource, ".NET BitmapImage") : null; // TODO: Async
                await Task.CompletedTask;
                var data = createSource ? new BitmapPageData(streamSource) : null;
                return BitmapPageSource.Create(data, pictureInfo, this);
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
