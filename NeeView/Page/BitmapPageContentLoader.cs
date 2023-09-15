using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class BitmapPageContentLoader
    {
        private readonly ArchiveEntry _entry;
        private IBitmapPageSourceLoader? _imageDataLoader;

        public BitmapPageContentLoader(ArchiveEntry entry)
        {
            _entry = entry;
        }

        public async Task<BitmapPageSource> LoadAsync(bool createPictureInfo, CancellationToken token)
        {
            var loader = _imageDataLoader ?? new BitmapPageSourceLoader();
            var imageData = await loader.LoadAsync(_entry, createPictureInfo, token);
            _imageDataLoader = imageData.ImageDataLoader;
            return imageData;
        }
    }
}