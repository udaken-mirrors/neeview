using NeeView.Drawing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
#warning not imprement PDF
#if false
    public class PdfPictureSource : PictureSource
    {
        private readonly PdfArchiver _pdfArchive;

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
            pictureInfo.Decoder = _pdfArchive.ToString();
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
            var bitmap = _pdfArchive.CreateBitmapSource(ArchiveEntry, size);

            // 色情報設定
            PictureInfo?.SetPixelInfo(bitmap);

            return bitmap;
        }

        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            size = size.IsEmpty ? GetImageSize() : size;
            return _pdfArchive.CreateBitmapData(ArchiveEntry, size, setting, format, quality);
        }

        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            var size = ThumbnailProfile.GetThumbnailSize(GetImageSize());
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
#endif
}
