﻿using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// エントリに対応する表示画像
    /// </summary>
    public class Picture : BindableBase
    {
        #region Fields

        /// <summary>
        /// リサイズパラメータのハッシュ。
        /// リサイズが必要かの判定に使用される
        /// </summary>
        private int _resizeHashCode;

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private object _lock = new object();

        #endregion

        #region Constructors

        public Picture(PictureSource source)
        {
            PictureSource = source;

            _resizeHashCode = GetEnvironmentoHashCode();
        }

        #endregion

        #region Properties

        public PictureSource PictureSource { get; private set; }


        /// <summary>
        /// 画像情報
        /// </summary>
        public PictureInfo PictureInfo => PictureSource.PictureInfo;

        /// <summary>
        /// 表示する画像
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        public long GetMemorySize()
        {
            return _bitmapSource != null ? (long)_bitmapSource.Format.BitsPerPixel * _bitmapSource.PixelWidth * _bitmapSource.PixelHeight / 8 : 0L;
        }

        // 画像生成に影響する設定のハッシュ値取得
        private int GetEnvironmentoHashCode()
        {
            return ImageFilter.Current.GetHashCode() ^ PictureProfile.Current.CustomSize.GetHashCodde();
        }

        // Bitmapが同じサイズであるか判定
        private bool IsEqualBitmapSizeMaybe(Size size, bool keepAspectRatio)
        {
            if (this.BitmapSource == null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            const double margin = 1.1;
            if (keepAspectRatio)
            {
                // アスペクト比固定のため、PixelHeightのみで判定
                return Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin;
            }
            else
            {
                return Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin && Math.Abs(size.Width - this.BitmapSource.PixelWidth) < margin;
            }
        }

        /// <summary>
        /// BitmapSource生成。
        /// サイズを指定し、必要であれば作り直す。不要であればなにもしない。
        /// </summary>
        public bool CreateBitmapSource(Size size, CancellationToken token)
        {
            size = size.IsEmpty ? this.PictureInfo.Size : size;
            size = PictureSource.FixedSize(size);

            // 規定サイズ判定
            if (!this.PictureInfo.IsLimited && size.IsEqualMaybe(this.PictureInfo.Size))
            {
                size = Size.Empty;
            }

            // アスペクト比固定?
            var cutomSize = PictureProfile.Current.CustomSize;
            var keepAspectRatio = size.IsEmpty || !cutomSize.IsEnabled || cutomSize.IsUniformed;

            int filterHashCode = GetEnvironmentoHashCode();
            bool isDartyResizeParameter = _resizeHashCode != filterHashCode;
            if (!isDartyResizeParameter && IsEqualBitmapSizeMaybe(size, keepAspectRatio))
            {
                return false;
            }

            ////var nowSize = new Size(this.BitmapSource.PixelWidth, this.BitmapSource.PixelHeight);
            ////Debug.WriteLine($"Resize: {isDartyResizeParameter}: {nowSize.Truncate()} -> {size.Truncate()}");
            ////Debug.WriteLine($"BMP: {this.PictureInfo.Size} -> {size}");

            var bitmap = CreateBitmapSource(size, keepAspectRatio, token);
            if (bitmap == null)
            {
                return false;
            }

            token.ThrowIfCancellationRequested();

            lock (_lock)
            {
                _resizeHashCode = filterHashCode;
                this.BitmapSource = bitmap;
            }

            return true;
        }

        private BitmapSource CreateBitmapSource(Size size, bool keepAspectRatio, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var setting = new BitmapCreateSetting();

            if (!size.IsEmpty)
            {
                setting.IsKeepAspectRatio = keepAspectRatio;
                if (PictureProfile.Current.IsResizeFilterEnabled)
                {
                    setting.Mode = BitmapCreateMode.HighQuality;
                    setting.ProcessImageSettings = ImageFilter.Current.CreateProcessImageSetting();
                }
            }

            return MemoryControl.Current.RetryFuncWithMemoryCleanup(() => PictureSource.CreateBitmapSource(size, setting, token));
        }

        #endregion
    }


}
