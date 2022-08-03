using NeeView.Drawing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class PdfPictureSource : PictureSource
    {
        private PdfArchiver _pdfArchive;

        public PdfPictureSource(ArchiveEntry entry, PictureInfo? pictureInfo, PictureSourceCreateOptions createOptions) : base(entry, pictureInfo, createOptions)
        {
            _pdfArchive = entry.Archiver as PdfArchiver ?? throw new InvalidOperationException();
        }

        public override PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (PictureInfo != null) return PictureInfo;

            var pictureInfo = new PictureInfo();
            var originalSize = _pdfArchive.GetSourceSize(ArchiveEntry);
            pictureInfo.OriginalSize = originalSize;
            var maxSize = Config.Current.Performance.MaximumSize;
            var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : originalSize;
            pictureInfo.Size = size;
            pictureInfo.BitsPerPixel = 32;
            pictureInfo.Decoder = "WinRT";
            this.PictureInfo = pictureInfo;

            return PictureInfo;
        }

        private Size GetImageSize()
        {
            if (this.PictureInfo is null)
            {
                CreatePictureInfo(CancellationToken.None);
            }

            return PictureInfo.Size;
        }

        public override ImageSource CreateImageSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            size = size.IsEmpty ? GetImageSize() : size;
            using (var stream = _pdfArchive.CraeteBitmapAsStream(ArchiveEntry, size))
            {
                // Bitmap生成
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                // 色情報設定
                PictureInfo?.SetPixelInfo(bitmap);

                return bitmap;
            }
        }


        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            size = size.IsEmpty ? GetImageSize() : size;

            using (var outStream = new MemoryStream())
            {
                using (Stream bitmapStream = _pdfArchive.CraeteBitmapAsStream(ArchiveEntry, size))
                {
                    var bitmap = BitmapFrame.Create(bitmapStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    var encoder = CreateFormat(format, quality);
                    encoder.Frames.Add(bitmap);
                    encoder.Save(outStream);
                }

                return outStream.ToArray();
            }
        }



        private BitmapEncoder CreateFormat(BitmapImageFormat format, int quality)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    return new JpegBitmapEncoder() { QualityLevel = quality };
                case BitmapImageFormat.Png:
                    return new PngBitmapEncoder();
            }
        }


        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            var size = profile.GetThumbnailSize(GetImageSize());
            var setting = profile.CreateBitmapCreateSetting(true);
            return CreateImage(size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }

        public override Size FixedSize(Size size)
        {
            var imageSize = GetImageSize();

            size = size.IsEmpty ? imageSize : size;

            // 最小サイズ
            if (Config.Current.Archive.Pdf.RenderSize.IsContains(size))
            {
                size = size.Uniformed(Config.Current.Archive.Pdf.RenderSize);
            }

            // 最大サイズ
            var maxWixth = Math.Max(imageSize.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(imageSize.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWixth, maxHeight);
            size = size.Limit(maxSize);

            return size;
        }
    }
}
