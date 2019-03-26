﻿using NeeView.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class PdfPictureSource : PictureSource
    {
        private MagicScalerBitmapFactory _magicScaler = new MagicScalerBitmapFactory();

        private PdfArchiver _pdfArchive;

        public PdfPictureSource(ArchiveEntry entry, PictureSourceCreateOptions createOptions) : base(entry, createOptions)
        {
            _pdfArchive = (PdfArchiver)entry.Archiver;
        }

        public override void InitializePictureInfo(CancellationToken token)
        {
            this.PictureInfo = new PictureInfo(ArchiveEntry);

            var size = _pdfArchive.GetRenderSize(ArchiveEntry);
            PictureInfo.OriginalSize = size;
            PictureInfo.Size = size;
            PictureInfo.Decoder = "PDFium";
        }

        public override BitmapSource CreateBitmapSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            size = size.IsEmpty ? _pdfArchive.GetRenderSize(ArchiveEntry) : size;
            var bitmapSource = _pdfArchive.CraeteBitmapSource(ArchiveEntry, size).ToBitmapSource();

            // 色情報とBPP設定
            PictureInfo.SetPixelInfo(bitmapSource);

            return bitmapSource;
        }

        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using (var outStream = new MemoryStream())
            {
                size = size.IsEmpty ? _pdfArchive.GetRenderSize(ArchiveEntry) : size;
                _pdfArchive.CraeteBitmapSource(ArchiveEntry, size).SaveWithQuality(outStream, CreateFormat(format), quality);
                return outStream.ToArray();
            }
        }

        private System.Drawing.Imaging.ImageFormat CreateFormat(BitmapImageFormat format)
        {
            switch (format)
            {
                default:
                case BitmapImageFormat.Jpeg:
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case BitmapImageFormat.Png:
                    return System.Drawing.Imaging.ImageFormat.Png;
            }
        }


        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            Size size;
            if (PictureInfo != null)
            {
                size = PictureInfo.Size;
            }
            else
            {
                size = _pdfArchive.GetRenderSize(ArchiveEntry);
            }

            size = profile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting();
            return CreateImage(size, setting, profile.Format, profile.Quality, token);
        }
    }
}
