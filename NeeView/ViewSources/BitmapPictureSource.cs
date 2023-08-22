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
    public class BitmapPictureSource : IPictureSource
    {
        private BitmapPageContent _pageContent;

        // TODO: 毎回作ってるけど大丈夫？
        private BitmapFactory _bitmapFactory = new BitmapFactory();

        public BitmapPictureSource(BitmapPageContent pageContent)
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
        /// <param name="data">元データ</param>
        /// <param name="size">作成する画像サイズ。Size.Emptyの場合は元データサイズで作成</param>
        /// <param name="setting">生成オプション</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>ImageSource</returns>
        public ImageSource CreateImageSource(object data, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            var bytes = (byte[])data;
            token.ThrowIfCancellationRequested();

            Debug.WriteLine($"{ArchiveEntry}, {size:f0}", "CreateImageSource()");

            using (var stream = new MemoryStream(bytes))
            {
                if (setting.IsKeepAspectRatio && !size.IsEmpty)
                {
                    size = new Size(size.Width, 0);
                }

                var bitmapSource = _bitmapFactory.CreateBitmapSource(stream, PictureInfo?.BitmapInfo, size, setting, token);

                // 色情報とBPP設定。
                PictureInfo?.SetPixelInfo(bitmapSource);

                return bitmapSource;
            }
        }


        public Size FixedSize(Size size)
        {
            Debug.Assert(PictureInfo != null);

            var maxWixth = Math.Max(PictureInfo.Size.Width, Config.Current.Performance.MaximumSize.Width);
            var maxHeight = Math.Max(PictureInfo.Size.Height, Config.Current.Performance.MaximumSize.Height);
            var maxSize = new Size(maxWixth, maxHeight);
            return size.Limit(maxSize);
        }



        public byte[] CreateImage(object data, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            var bytes = (byte[])data;
            token.ThrowIfCancellationRequested();

            using (var stream = new MemoryStream(bytes))
            {
                using (var outStream = new MemoryStream())
                {
                    _bitmapFactory.CreateImage(stream, PictureInfo?.BitmapInfo, outStream, size, format, quality, setting, token);
                    return outStream.ToArray();
                }
            }
        }


        public byte[] CreateThumbnail(object data, ThumbnailProfile profile, CancellationToken token)
        {
            ////Debug.WriteLine($"## CreateThumbnail: {this.ArchiveEntry}");
            var bytes = (byte[])data;
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
                using (var stream = new MemoryStream(bytes))
                {
                    bitmapInfo = BitmapInfo.Create(stream);
                    size = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
                }
            }

            size = ThumbnailProfile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting(bitmapInfo?.Metadata?.IsOriantationEnabled == true);
            return CreateImage(data, size, setting, Config.Current.Thumbnail.Format, Config.Current.Thumbnail.Quality, token);
        }
    }

}
