using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DefaultBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async Task<BitmapPageSource> LoadAsync(ArchiveEntry entry, bool createPctureInfo, CancellationToken token)
        {
            try
            {
                using var stream = entry.OpenEntry();
                var length = stream.Length;
                var buffer = new byte[length];
                var readSize = await stream.ReadAsync(buffer, 0, (int)length, token);
                if (readSize < length) throw new IOException("This file size is too large to read.");
                var pictureInfo = createPctureInfo ? PictureInfo.Create(buffer, ".NET BitmapImage") : null;
                return BitmapPageSource.Create(buffer, pictureInfo, this);
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
