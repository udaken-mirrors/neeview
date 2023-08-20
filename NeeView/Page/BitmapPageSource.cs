using System.IO;
using System.Windows;

namespace NeeView
{
    public class BitmapPageSource : PageSource
    {
        public BitmapPageSource(object? data, string? errorMessage, PictureInfo? pictureInfo, IBitmapPageSourceLoader? imageDataLoader)
            : base(data, errorMessage, pictureInfo)
        {
            ImageDataLoader = imageDataLoader;
        }


        public byte[]? DataAsByteArray => (byte[]?)Data;
        public override long DataSize => DataAsByteArray?.LongLength ?? 0;
        public IBitmapPageSourceLoader? ImageDataLoader { get; }


        public static BitmapPageSource Create(byte[] data, PictureInfo? pictureInfo, IBitmapPageSourceLoader imageDataLoader)
        {
            return new BitmapPageSource(data, null, pictureInfo, imageDataLoader);
        }

        public new static BitmapPageSource CreateError(string errorMessage)
        {
            return new BitmapPageSource(null, errorMessage, null, null);
        }
    }

}
