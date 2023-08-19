using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DefaultImageDataLoader : IImageDataLoader
    {
        public async Task<ImageData> LoadAsync(ArchiveEntry entry, bool createPctureInfo, CancellationToken token)
        {
            try
            {
                using var stream = entry.OpenEntry();
                var length = stream.Length;
                var buffer = new byte[length];
                var readSize = await stream.ReadAsync(buffer, 0, (int)length, token);
                if (readSize < length) throw new IOException("This file size is too large to read.");
                var pictureInfo = createPctureInfo ? PictureInfo.Create(buffer, ".NET BitmapImage") : null;
                return ImageData.Create(buffer, pictureInfo, this);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ImageData.CreateError(ex.Message);
            }
        }
    }
}
