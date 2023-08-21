using NeeLaboratory.ComponentModel;
using NeeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// サムネイル.
    /// Jpegで保持し、必要に応じてBitmapSourceを生成
    /// </summary>
    public class Thumbnail : BindableBase, IThumbnail, IDisposable
    {
        /// <summary>
        /// 開発用：キャッシュ読み込み無効
        /// </summary>
        public static bool DebugIgnoreCache { get; set; }



        /// <summary>
        /// キャシュ用ヘッダ
        /// </summary>
        private ThumbnailCacheHeader? _header;

        /// <summary>
        /// サムネイルデータ
        /// </summary>
        private byte[]? _image;



        /// <summary>
        /// 変更イベント
        /// </summary>
        public event EventHandler? Changed;

        public IDisposable SubscribeChanged(EventHandler handler)
        {
            Changed += handler;
            return new AnonymousDisposable(() => Changed -= handler);
        }


        /// <summary>
        /// 参照イベント
        /// </summary>
        public event EventHandler? Touched;

        public IDisposable SubscribeTouched(EventHandler handler)
        {
            Touched += handler;
            return new AnonymousDisposable(() => Touched -= handler);
        }



        /// <summary>
        /// 有効判定
        /// </summary>
        internal bool IsValid => _image != null;

        /// <summary>
        /// Jpeg化された画像
        /// </summary>
        public byte[]? Image
        {
            get { return _image; }
            set
            {
                if (_disposedValue) return;
                if (_image != value)
                {
                    _image = value;
                    if (Image != null)
                    {
                        Changed?.Invoke(this, EventArgs.Empty);
                        Touched?.Invoke(this, EventArgs.Empty);
                        RaisePropertyChanged("");
                    }
                }
            }
        }

        /// <summary>
        /// ユニークイメージ？
        /// </summary>
        public bool IsUniqueImage => _image == null || (_image != ThumbnailResource.EmptyImage && _image != ThumbnailResource.MediaImage && _image != ThumbnailResource.FolderImage);

        /// <summary>
        /// 標準イメージ？
        /// バナーでの引き伸ばし許可
        /// </summary>
        public bool IsNormalImage => _image == null || (_image != ThumbnailResource.MediaImage && _image != ThumbnailResource.FolderImage);

        /// <summary>
        /// View用Bitmapプロパティ
        /// </summary>
        public ImageSource? ImageSource => CreateBitmap();

        public double Width => ImageSource is BitmapSource bitmap ? bitmap.PixelWidth : ImageSource != null ? ImageSource.Width : 0.0;
        public double Height => ImageSource is BitmapSource bitmap ? bitmap.PixelHeight : ImageSource != null ? ImageSource.Height : 0.0;

        /// <summary>
        /// View用Bitmapの背景プロパティ
        /// </summary>
        public Brush Background
        {
            get
            {
                if (_image == ThumbnailResource.MediaImage)
                {
                    return ThumbnailResource.MediaBackground;
                }
                else
                {
                    return Brushes.Transparent;
                }
            }
        }

        /// <summary>
        /// 寿命間利用シリアルナンバー
        /// </summary>
        public int LifeSerial { get; set; }

        /// <summary>
        /// キャッシュ使用
        /// </summary>
        public bool IsCacheEnabled { get; set; }


        /// <summary>
        /// キャッシュを使用してサムネイル生成を試みる
        /// </summary>
        internal async Task InitializeAsync(ArchiveEntry entry, string? appendix, CancellationToken token)
        {
            if (_disposedValue) return;
            if (IsValid || !IsCacheEnabled) return;

#if DEBUG
            if (DebugIgnoreCache)
            {
                Image = null;
                return;
            }
#endif
            // NOTE: ディレクトリは更新日をサイズとする
            var length = entry.IsDirectory ? entry.LastWriteTime.ToBinary() : entry.Length;

            _header = new ThumbnailCacheHeader(entry.SystemPath, length, appendix, Config.Current.Thumbnail.GetThumbnailImageGenerateHash());
            var image = await ThumbnailCache.Current.LoadAsync(_header, token);
            ////Debug.WriteLine($"ThumbnailCache.Load: {_header.Key}: {(image == null ? "Miss" : "Hit!")}");
            Image = image;
        }

        /// <summary>
        /// 画像データから初期化
        /// </summary>
        /// <param name="source"></param>
        internal void Initialize(byte[]? image)
        {
            if (_disposedValue) return;
            if (IsValid) return;

            Image = image ?? ThumbnailResource.EmptyImage;

            SaveCacheAsync();
        }

        /// <summary>
        /// サムネイル基本タイプから初期化
        /// </summary>
        /// <param name="type"></param>
        internal void Initialize(ThumbnailType type)
        {
            if (_disposedValue) return;

            Image = type switch
            {
                ThumbnailType.Media => ThumbnailResource.MediaImage,
                ThumbnailType.Folder => ThumbnailResource.FolderImage,
                _ => ThumbnailResource.EmptyImage,
            };
        }

        /// <summary>
        /// サムネイルソースから初期化
        /// </summary>
        /// <param name="source"></param>
        internal void Initialize(ThumbnailSource source)
        {
            if (_disposedValue) return;

            if (source == null)
            {
                Initialize((byte[]?)null);
            }
            else if (source.Type == ThumbnailType.Unique)
            {
                Initialize(source.RawData);
            }
            else
            {
                Initialize(source.Type);
            }
        }

        /// <summary>
        /// キャッシュに保存
        /// </summary>
        internal void SaveCacheAsync()
        {
            if (_disposedValue) return;
            if (!IsCacheEnabled || _header == null) return;
            if (_image == null || _image == ThumbnailResource.EmptyImage || _image == ThumbnailResource.MediaImage || _image == ThumbnailResource.FolderImage) return;

            ThumbnailCache.Current.EntrySaveQueue(_header, _image);
        }

        /// <summary>
        /// image無効
        /// </summary>
        public void Clear()
        {
            // 通知は不要なので直接パラメータ変更
            _image = null;
        }

        /// <summary>
        /// Touch
        /// </summary>
        public void Touch()
        {
            Touched?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// ImageSource取得
        /// </summary>
        /// <returns></returns>
        public ImageSource? CreateBitmap()
        {
            if (_disposedValue) return null;
            if (_image is null) return null;

            Touched?.Invoke(this, EventArgs.Empty);
            if (_image == ThumbnailResource.EmptyImage)
            {
                return ThumbnailResource.EmptyImageSource;
            }
            else if (_image == ThumbnailResource.MediaImage)
            {
                return ThumbnailResource.MediaBitmapSource;
            }
            else if (_image == ThumbnailResource.FolderImage)
            {
                return ThumbnailResource.FolderBitmapSource;
            }
            else
            {
                return DecodeFromImageData(_image);
            }
        }


        /// <summary>
        /// ImageData to BitmapSource
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static BitmapSource DecodeFromImageData(byte[] image)
        {
            using var stream = new MemoryStream(image, false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _image = null;
                    Changed = null;
                    Touched = null;
                    ResetPropertyChanged();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
