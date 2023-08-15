using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Picture 用のリソース
    /// </summary>
    public class BitmapPictureSource : IPictureSource
    {
        private BitmapPageContent _pageContent;


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
        public ImageSource CreateImageSource(byte[] data, Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var _bitmapFactory = new BitmapFactory();

            Debug.WriteLine($"{ArchiveEntry}, {size:f0}", "CreateImageSource()");

            using (var stream = new MemoryStream(data))
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
    }

}
