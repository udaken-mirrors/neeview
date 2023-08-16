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
        ImageSource CreateImageSource(byte[] data, Size size, BitmapCreateSetting setting, CancellationToken token);

        byte[] CreateImage(byte[] data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
        byte[] CreateThumbnail(byte[] data, ThumbnailProfile profile, CancellationToken token);
        
        //PictureInfo CreatePictureInfo(CancellationToken token);
        //long GetMemorySize();
    }
}