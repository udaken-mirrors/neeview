using NeeView.ComponentModel;
using System.IO;
using System.Windows;

namespace NeeView
{
    public class ImageData : IDataSource<byte[]>
    {
        public ImageData(byte[]? data, string? errorMessage, PictureInfo? pictureInfo, IImageDataLoader? imageDataLoader)
        {
            Data = data;
            ErrorMessage = errorMessage;
            PictureInfo = pictureInfo;
            ImageDataLoader = imageDataLoader;
        }

        public PictureInfo? PictureInfo { get; private set; }
        public IImageDataLoader? ImageDataLoader { get; private set; }
        public byte[]? Data { get; private set; }
        public long DataSize => Data?.LongLength ?? 0;
        public string? ErrorMessage { get; private set; }
        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;


        public static ImageData Create(byte[] data, PictureInfo? pictureInfo, IImageDataLoader imageDataLoader)
        {
            return new ImageData(data, null, pictureInfo, imageDataLoader);
        }

        public static ImageData CreateError(string errorMessage)
        {
            return new ImageData(null, errorMessage, null, null);
        }
    }


}
