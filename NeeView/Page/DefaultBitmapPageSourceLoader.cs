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
                var buffer = await entry.LoadAsync(token);
                var pictureInfo = createPictureInfo ? PictureInfo.Create(buffer, ".NET BitmapImage") : null;
                return BitmapPageSource.Create(new BitmapPageData(buffer), pictureInfo, this);
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
