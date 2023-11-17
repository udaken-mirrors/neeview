using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NeeView
{
    public class PdfPictureSource : IPictureSource<ArchiveEntry>
    {
        private readonly PdfArchiver _pdfArchive;

        public PdfPictureSource(ArchiveEntry archiveEntry, PictureInfo? pictureInfo)
        {
            ArchiveEntry = archiveEntry;
            PictureInfo = pictureInfo;

            _pdfArchive = ArchiveEntry.Archiver as PdfArchiver ?? throw new InvalidOperationException();
        }


        public ArchiveEntry ArchiveEntry { get; }

        public PictureInfo? PictureInfo { get; }

        private Size GetImageSize() => PictureInfo?.Size ?? new Size(480, 640);


        public byte[] CreateImage(ArchiveEntry entry, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            Debug.Assert(entry == ArchiveEntry);
            size = size.IsEmpty ? GetImageSize() : size;
            return _pdfArchive.CreateBitmapData(entry, size, setting, format, quality);
        }

        public ImageSource CreateImageSource(ArchiveEntry entry, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            Debug.Assert(entry == ArchiveEntry);
            size = size.IsEmpty ? GetImageSize() : size;
            var bitmap = _pdfArchive.CreateBitmapSource(entry, size);

            // 色情報設定
            PictureInfo?.SetPixelInfo(bitmap);

            return bitmap;
        }

        public byte[] CreateThumbnail(ArchiveEntry entry, ThumbnailProfile profile, CancellationToken token)
        {
            Debug.Assert(entry == ArchiveEntry);
            var size = ThumbnailProfile.GetThumbnailSize(GetImageSize());
            var setting = profile.CreateBitmapCreateSetting(true);
            return CreateImage(entry, size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }


        public Size FixedSize(Size size)
        {
            var imageSize = GetImageSize();

            size = size.IsEmpty ? imageSize : size;

            // 最小サイズ
            if (Config.Current.Archive.Pdf.RenderSize.IsContains(size))
            {
                size = size.Uniformed(Config.Current.Archive.Pdf.RenderSize);
            }

            // 最大サイズ
            var maxWidth = Math.Max(imageSize.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(imageSize.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWidth, maxHeight);
            size = size.Limit(maxSize);

            return size;
        }

    }
}
