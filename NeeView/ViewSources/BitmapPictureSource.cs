using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Picture 用のリソース
    /// </summary>
    public class BitmapPictureSource : IPictureSource<IStreamSource>
    {
        private static readonly BitmapFactory _bitmapFactory = new();


        private readonly PageContent _pageContent;


        public BitmapPictureSource(PageContent pageContent)
        {
            _pageContent = pageContent;
        }


        public ArchiveEntry ArchiveEntry => _pageContent.ArchiveEntry;

        public PictureInfo? PictureInfo => _pageContent.PictureInfo;


        /// <summary>
        /// ImageSource作成
        /// </summary>
        /// <remarks>
        /// 元データは _pageContent からではなく引数で渡す
        /// </remarks>
        /// <param name="streamSource">元データ</param>
        /// <param name="size">作成する画像サイズ。Size.Emptyの場合は元データサイズで作成</param>
        /// <param name="setting">生成オプション</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>ImageSource</returns>
        public ImageSource CreateImageSource(IStreamSource streamSource, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            //Debug.WriteLine($"{ArchiveEntry}, {size:f0}", "CreateImageSource()");

            using var stream = streamSource.OpenStream();

            if (setting.IsKeepAspectRatio && !size.IsEmpty)
            {
                size = new Size(size.Width, 0);
            }

            var bitmapSource = _bitmapFactory.CreateBitmapSource(stream, PictureInfo?.BitmapInfo, size, setting, token);

            // 色情報とBPP設定。
            PictureInfo?.SetPixelInfo(bitmapSource);

            return bitmapSource;
        }

        public byte[] CreateImage(IStreamSource streamSource, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using var stream = streamSource.OpenStream();
            return CreateImage(stream, size, setting, format, quality, token);
        }

        public byte[] CreateImage(Stream stream, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using var outStream = new MemoryStream();
            _bitmapFactory.CreateImage(stream, PictureInfo?.BitmapInfo, outStream, size, format, quality, setting, token);
            return outStream.ToArray();
        }

        public byte[] CreateThumbnail(IStreamSource streamSource, ThumbnailProfile profile, CancellationToken token)
        {
            using var stream = streamSource.OpenStream();
            return CreateThumbnail(stream, profile, token);
        }

        public byte[] CreateThumbnail(Stream stream, ThumbnailProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Size size;
            BitmapInfo? bitmapInfo;
            if (PictureInfo != null)
            {
                size = PictureInfo.Size;
                bitmapInfo = PictureInfo.BitmapInfo;
            }
            else
            {
                bitmapInfo = BitmapInfo.Create(stream);
                size = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
            }

            size = ThumbnailProfile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting(bitmapInfo?.Metadata?.IsOriantationEnabled == true);
            stream.Seek(0, SeekOrigin.Begin);
            return CreateImage(stream, size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }


        public Size FixedSize(Size size)
        {
            Debug.Assert(PictureInfo != null);

            var maxWidth = Math.Max(PictureInfo.Size.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(PictureInfo.Size.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWidth, maxHeight);
            return size.Limit(maxSize);
        }
    }
}
