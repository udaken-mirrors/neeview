using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class BitmapPageContentLoader
    {
        private IBitmapPageSourceLoader? _imageDataLoader;

        public BitmapPageContentLoader()
        {
        }

        public async Task<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, CancellationToken token)
        {
            var loader = _imageDataLoader ?? new BitmapPageSourceLoader();
            var imageData = await loader.LoadAsync(streamSource, createPictureInfo, token);
            _imageDataLoader = imageData.ImageDataLoader;
            return imageData;
        }
    }
}