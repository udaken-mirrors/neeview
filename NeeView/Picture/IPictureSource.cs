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
        ImageSource CreateImageSource(Size size, BitmapCreateSetting setting, CancellationToken token);

        //byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token);
        //PictureInfo CreatePictureInfo(CancellationToken token);
        //byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token);
        //long GetMemorySize();
    }
}