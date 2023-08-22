using NeeView.Media.Imaging;
using NeeView.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class SvgPictureSource : IPictureSource
    {
        private SvgPageContent _pageContent;

        public SvgPictureSource(SvgPageContent pageContent)
        {
            _pageContent = pageContent;
        }

        public ArchiveEntry ArchiveEntry => _pageContent.Entry;

        public PictureInfo? PictureInfo => _pageContent.PictureInfo;


        // TODO: async
        public byte[] CreateImage(object data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            if (size.IsEmptyOrZero()) throw new ArgumentOutOfRangeException(nameof(size));

            token.ThrowIfCancellationRequested();
            var drawing = (DrawingGroup)data;

            var imageSource = CreateImageSource(drawing);

            BitmapSource? bitmap = null;
            var task = new Task(() =>
            {
                // TODO: 関数名がおかしい
                bitmap = imageSource.CreateThumbnail(size);
            });
            task.Start(SingleThreadedApartment.TaskScheduler); // STA
            task.Wait(token);

            using (var outStream = new MemoryStream())
            {
                var encoder = DefaultBitmapFactory.CreateEncoder(format, quality);
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(outStream);
                return outStream.ToArray();
            }
        }

        public ImageSource CreateImageSource(object data, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var drawing = (DrawingGroup)data;
            return CreateImageSource(drawing);
        }

        public byte[] CreateThumbnail(object data, ThumbnailProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            
            Debug.Assert(PictureInfo != null);
            var size = PictureInfo.Size;
            
            size = ThumbnailProfile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting(true);
            return CreateImage(data, size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }


        private ImageSource CreateImageSource(DrawingGroup drawing)
        {
            var image = new DrawingImage();
            image.Drawing = drawing;
            image.Freeze();
            return image;
        }

        public Size FixedSize(Size size)
        {
            // SVGはサイズ制限なし
            Debug.Assert(PictureInfo != null);
            return this.PictureInfo.Size;
        }
    }
}
