using NeeLaboratory.ComponentModel;
using NeeView.Media.Imaging;
using PhotoSauce.MagicScaler;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// エントリに対応する表示画像
    /// </summary>
    public class Picture : BindableBase
    {
        private readonly IPictureSource _pictureSource;
 
        /// <summary>
        /// リサイズパラメータのハッシュ。
        /// リサイズが必要かの判定に使用される
        /// </summary>
        private int _resizeHashCode;

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object _lock = new();


        public Picture(IPictureSource source)
        {
            _pictureSource = source;

            _resizeHashCode = GetEnvironmentoHashCode();
        }



        /// <summary>
        /// 画像情報
        /// </summary>
        public PictureInfo? PictureInfo => _pictureSource.PictureInfo;

        /// <summary>
        /// 表示する画像
        /// </summary>
        private ImageSource? _imageSource;
        public ImageSource? ImageSource
        {
            get { return _imageSource; }
            set { if (_imageSource != value) { _imageSource = value; RaisePropertyChanged(); } }
        }


        public long GetMemorySize()
        {
            if (_imageSource == null) return 0L;

            if (_imageSource is BitmapSource bitmapSource)
            {
                return (long)bitmapSource.Format.BitsPerPixel * bitmapSource.PixelWidth * bitmapSource.PixelHeight / 8;
            }
            else
            {
                return 1024 * 1024;
            }
        }

        // 画像生成に影響する設定のハッシュ値取得
        private static int GetEnvironmentoHashCode()
        {
            return Config.Current.ImageResizeFilter.GetHashCode() ^ Config.Current.ImageCustomSize.GetHashCodde();
        }

        // Imageが同じサイズであるか判定
        private bool IsEqualImageSizeMaybe(Size size, bool keepAspectRatio)
        {
            if (this.ImageSource is null) return false;
            if (this.PictureInfo is null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            const double margin = 1.1;
            if (keepAspectRatio)
            {
                // アスペクト比固定のため、PixelHeightのみで判定
                return Math.Abs(size.Height - this.ImageSource.GetPixelHeight()) < margin;
            }
            else
            {
                return Math.Abs(size.Height - this.ImageSource.GetPixelHeight()) < margin && Math.Abs(size.Width - this.ImageSource.GetPixelWidth()) < margin;
            }
        }

        /// <summary>
        /// ImageSource生成。
        /// サイズを指定し、必要であれば作り直す。不要であればなにもしない。
        /// </summary>
        /// TODO: 名前は UpdateImageSource のほうがふさわしい
        public bool CreateImageSource(byte[] data, Size size, CancellationToken token)
        {
            if (this.PictureInfo is null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;
            size = _pictureSource.FixedSize(size);

            // 規定サイズ判定
            if (!this.PictureInfo.IsLimited && size.IsEqualMaybe(this.PictureInfo.Size))
            {
                size = Size.Empty;
            }

            // アスペクト比固定?
            var cutomSize = Config.Current.ImageCustomSize;
            var keepAspectRatio = size.IsEmpty || !cutomSize.IsEnabled || cutomSize.AspectRatio == CustomSizeAspectRatio.Origin;

            int filterHashCode = GetEnvironmentoHashCode();
            bool isDartyResizeParameter = _resizeHashCode != filterHashCode;
            if (!isDartyResizeParameter && IsEqualImageSizeMaybe(size, keepAspectRatio))
            {
                return false;
            }

#if false
            Debug.WriteLine($"Resize: {this.PictureSource.ArchiveEntry.EntryLastName}");
            var nowSize = new Size(this.PictureInfo.BitmapInfo.PixelWidth, this.PictureInfo.BitmapInfo.PixelHeight);
            Debug.WriteLine($"Resize: {isDartyResizeParameter}: {nowSize.Truncate()} -> {size.Truncate()}");
            Debug.WriteLine($"BMP: {this.PictureSource.ArchiveEntry.EntryName}: {this.PictureInfo.Size} -> {size}");
#endif

            var image = CreateImageSource(data, size, keepAspectRatio, token);
            if (image == null)
            {
                return false;
            }

            lock (_lock)
            {
                _resizeHashCode = filterHashCode;
                this.ImageSource = image;
            }

            return true;
        }

        public void ClearImageSource()
        {
            lock (_lock)
            {
                _resizeHashCode = 0;
                this.ImageSource = null;
            }
        }

        private ImageSource CreateImageSource(byte[] data, Size size, bool keepAspectRatio, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var setting = new BitmapCreateSetting();

            if (!size.IsEmpty)
            {
                setting.IsKeepAspectRatio = keepAspectRatio;
                if (Config.Current.ImageResizeFilter.IsEnabled)
                {
                    setting.Mode = BitmapCreateMode.HighQuality;
                    setting.ProcessImageSettings = Config.Current.ImageResizeFilter.CreateProcessImageSetting();
                    setting.ProcessImageSettings.OrientationMode = this.PictureInfo?.Metadata?.IsOriantationEnabled == true ? OrientationMode.Normalize : OrientationMode.Ignore;
                }
            }

            return MemoryControl.Current.RetryFuncWithMemoryCleanup(() => _pictureSource.CreateImageSource(data, size, setting, token));
        }
    }

}
