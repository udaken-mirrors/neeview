//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class Thumbnail : BindableBase, IThumbnail, IDisposable
    {
        /// <summary>
        /// 開発用：キャッシュ読み込み無効
        /// </summary>
        public static bool DebugIgnoreCache { get; set; }

        /// <summary>
        /// 開発用：シリアル番号
        /// </summary>
        private static int _serialCount;
        private readonly int _serialNumber = _serialCount++;

        /// <summary>
        /// キャシュ用ヘッダ
        /// </summary>
        private ThumbnailCacheHeader? _header;

        /// <summary>
        /// サムネイルデータ
        /// </summary>
        /// <remarks>
        /// 非同期に値が変更される可能性があるので取り扱い注意
        /// </remarks>
        private byte[]? _image;



        /// <summary>
        /// 変更イベント
        /// </summary>
        [Subscribable]
        public event EventHandler? Changed;

        /// <summary>
        /// 参照イベント
        /// </summary>
        [Subscribable]
        public event EventHandler? Touched;



        /// <summary>
        /// 有効判定
        /// </summary>
        public bool IsValid => _image != null;

        /// <summary>
        /// Empty画像？
        /// </summary>
        public bool IsEmptyImage => _image == ThumbnailResource.EmptyImage;

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
                    if (_image != null)
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
        public bool IsUniqueImage
        {
            get
            {
                var image = _image;
                return image == null || (image != ThumbnailResource.EmptyImage && image != ThumbnailResource.MediaImage && image != ThumbnailResource.FolderImage);
            }
        }

        /// <summary>
        /// 標準イメージ？
        /// バナーでの引き伸ばし許可
        /// </summary>
        public bool IsNormalImage
        {
            get
            {
                var image = _image;
                return image == null || (image != ThumbnailResource.MediaImage && image != ThumbnailResource.FolderImage);
            }
        }

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
        public async Task InitializeFromCacheAsync(ArchiveEntry entry, string? appendix, CancellationToken token)
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
            Trace($"ThumbnailCache.Load: {_header.Key}: {(image == null ? "Miss" : "Hit!")}");
            Image = image;
        }

        /// <summary>
        /// 画像データから初期化
        /// </summary>
        /// <param name="source"></param>
        public void Initialize(byte[]? image)
        {
            if (_disposedValue) return;
            if (IsValid) return;

            Trace($"Initialize: from binary={image?.Length ?? 0} byte");
            Image = image ?? ThumbnailResource.EmptyImage;

            SaveCacheAsync();
        }

        /// <summary>
        /// サムネイル基本タイプから初期化
        /// </summary>
        /// <param name="type"></param>
        public void Initialize(ThumbnailType type)
        {
            if (_disposedValue) return;

            Trace($"Initialize: from type={type}");
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
        public void Initialize(ThumbnailSource source)
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
        public void SaveCacheAsync()
        {
            if (_disposedValue) return;
            if (!IsCacheEnabled || _header == null) return;

            var image = _image;
            if (image == null || image == ThumbnailResource.EmptyImage || image == ThumbnailResource.MediaImage || image == ThumbnailResource.FolderImage) return;

            ThumbnailCache.Current.EntrySaveQueue(_header, image);
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
        public ImageSource? CreateImageSource()
        {
            if (_disposedValue) return null;

            var image = _image;
            if (image is null) return null;

            Touched?.Invoke(this, EventArgs.Empty);
            if (image == ThumbnailResource.EmptyImage)
            {
                return ThumbnailResource.EmptyImageSource;
            }
            else if (image == ThumbnailResource.MediaImage)
            {
                return ThumbnailResource.MediaBitmapSource;
            }
            else if (image == ThumbnailResource.FolderImage)
            {
                return ThumbnailResource.FolderBitmapSource;
            }
            else
            {
                return DecodeFromImageData(image);
            }
        }

        public ThumbnailSource CreateSource()
        {
            var image = _image;
            if (image == ThumbnailResource.EmptyImage)
            {
                return new ThumbnailSource(ThumbnailType.Empty);
            }
            else if (image == ThumbnailResource.MediaImage)
            {
                return new ThumbnailSource(ThumbnailType.Media);
            }
            else if (image == ThumbnailResource.FolderImage)
            {
                return new ThumbnailSource(ThumbnailType.Folder);
            }
            else
            {
                return new ThumbnailSource(ThumbnailType.Unique, image);
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

        public override string ToString()
        {
            var name = _header?.Key ?? "(none)";
            return $"{name}: LifeSerial={LifeSerial}: Length={_image?.Length ?? 0:#,0}";
         }


        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}({_serialNumber}): {string.Format(s, args)}");
        }
    }


}
