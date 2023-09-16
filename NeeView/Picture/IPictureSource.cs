using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public interface IPictureSource
    {
        ArchiveEntry ArchiveEntry { get; }
        PictureInfo? PictureInfo { get; }

        Size FixedSize(Size size);
        ImageSource CreateImageSource(object data, Size size, BitmapCreateSetting setting, CancellationToken token);
        byte[] CreateImage(object data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
        byte[] CreateThumbnail(object data, ThumbnailProfile profile, CancellationToken token);
    }


    public interface IPictureSource<T> : IPictureSource
    {
        ImageSource IPictureSource.CreateImageSource(object data, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            return CreateImageSource((T)data, size, setting, token);
        }

        byte[] IPictureSource.CreateImage(object data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            return CreateImage((T)data, size, setting, format, quality, token);
        }

        byte[] IPictureSource.CreateThumbnail(object data, ThumbnailProfile profile, CancellationToken token)
        {
            return CreateThumbnail((T)data, profile, token);
        }

        ImageSource CreateImageSource(T data, Size size, BitmapCreateSetting setting, CancellationToken token);
        byte[] CreateImage(T data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
        byte[] CreateThumbnail(T data, ThumbnailProfile profile, CancellationToken token);
    }

}