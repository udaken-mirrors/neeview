using System.IO;
using System.Threading;
using System.Windows;

namespace NeeView
{
    // TODO: BitmapPictureSource 等との関係性
    public class ThumbnailTools
    {
        // TODO: Async
        public static byte[] CreateThumbnailImage(byte[] bytes, PictureInfo? pictureInfo, ThumbnailProfile profile, CancellationToken token)
        {
            using var stream = new MemoryStream(bytes);
            return CreateThumbnailImage(stream, pictureInfo, profile, token);
        }

        // TODO: Async
        public static byte[] CreateThumbnailImage(Stream stream, PictureInfo? pictureInfo, ThumbnailProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            pictureInfo = pictureInfo ?? CreatePictureInfo(stream);
            var size = ThumbnailProfile.GetThumbnailSize(pictureInfo.Size);
            var setting = profile.CreateBitmapCreateSetting(pictureInfo.BitmapInfo?.Metadata?.IsOriantationEnabled == true);
            stream.Seek(0, SeekOrigin.Begin);
            return CreateImage(stream, pictureInfo, size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }

        // TODO: async
        private static PictureInfo CreatePictureInfo(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var bitmapInfo = BitmapInfo.Create(stream);
            var pictureInfo = PictureInfo.Create(bitmapInfo, ".NET Framework");
            return pictureInfo;
        }

        // TODO: Async
        private static byte[] CreateImage(Stream stream, PictureInfo pictureInfo, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using (var outStream = new MemoryStream())
            {
                var bitmapFactory = new BitmapFactory();
                bitmapFactory.CreateImage(stream, pictureInfo.BitmapInfo, outStream, size, format, quality, setting, token);
                return outStream.ToArray();
            }
        }
    }
}
