using NeeView.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ImageDataLoader : IImageDataLoader
    {
        public static ImageDataLoader Default { get; } = new ImageDataLoader();

        private readonly DefaultImageDataLoader _default = new();
        private readonly SusieImageDataLoader _susie = new();


        public ImageDataLoader()
        {
        }


        public async Task<ImageData> LoadAsync(ArchiveEntry entry, bool createPictureInfo, CancellationToken token)
        {
            ImageData? imageData = null;
            foreach (var loader in CreateOrderList())
            {
                imageData = await loader.LoadAsync(entry, createPictureInfo, token);
                if (imageData.DataState == DataState.Loaded)
                {
                    return imageData;
                }
            }
            return imageData ?? ImageData.CreateError("Failed to load image");
        }


        private List<IImageDataLoader> CreateOrderList()
        {
            if (!Config.Current.Susie.IsEnabled)
            {
                return new List<IImageDataLoader>() { _default };
            }
            else if (Config.Current.Susie.IsFirstOrderSusieImage)
            {
                return new List<IImageDataLoader>() { _susie, _default };
            }
            else
            {
                return new List<IImageDataLoader>() { _default, _susie };
            }
        }
    }
}
