using NeeLaboratory.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class BitmapContentLoader : IContentLoader, IMemoryElement
    {
        private readonly BitmapContent _content;
        private readonly object _lock = new();

        public BitmapContentLoader(BitmapContent content)
        {
            _content = content;
        }


        public event EventHandler? Loaded;

        public IDisposable SubscribeLoaded(EventHandler handler)
        {
            Loaded += handler;
            return new AnonymousDisposable(() => Loaded -= handler);
        }


        public PictureSource? PictureSource => _content.PictureSource;

        public bool IsPictureSourceLocked => _content.IsContentLocked;

        public int Index => _content.Index;

        public bool IsMemoryLocked => _content.IsContentLocked;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Loaded = null;
                    //// 以下、不要では？
                    ////UnloadContent(); 
                    ////UnloadPictureSource();
                    ////_content.SetPictureInfo(null);
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

        protected void RaiseLoaded()
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// PictureSource初期化
        /// </summary>
        private PictureSource LoadPictureSource(CancellationToken token)
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                var source = _content.PictureSource;
                if (source == null)
                {
                    source = PictureSourceFactory.Create(_content.Entry, _content.PictureInfo, PictureSourceCreateOptions.None, token);
                    _content.SetPictureInfo(MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreatePictureInfo(token)));
                    _content.SetPictureSource(source);

                    Book.Default?.BookMemoryService.AddPictureSource(this);
                }

                return source;
            }
        }

        /// <summary>
        /// PictureSource開放
        /// </summary>
        public void UnloadPictureSource()
        {
            lock (_lock)
            {
                _content.SetPictureSource(null);
            }
        }


        /// <summary>
        /// 画像読込
        /// </summary>
        protected Picture LoadPicture(ArchiveEntry entry, CancellationToken token)
        {
            try
            {
                ThrowIfDisposed();

                var source = LoadPictureSource(token);
                var picture = new Picture(source);

                // NOTE: リサイズフィルター有効の場合はBitmapSourceの生成をサイズ確定まで遅延させる
                if (!Config.Current.ImageResizeFilter.IsEnabled)
                {
                    picture.CreateImageSource(Size.Empty, token);
                }

                return picture;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // TODO: これはLoaderの役割
                _content.SetPageMessage(ex);
                throw;
            }
        }

        public void UnloadPicture()
        {
            _content.SetPicture(null);
        }

        /// <summary>
        /// Pictureの標準画像生成
        /// </summary>
        protected void PictureCreateBitmapSource(CancellationToken token)
        {
            if (_disposedValue) return;

            try
            {
                _content.Picture?.CreateImageSource(Size.Empty, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _content.SetPageMessage(ex);
                throw;
            }
        }

        /// <summary>
        /// コンテンツロード (Template)
        /// </summary>
        protected virtual async Task LoadContentAsyncTemplate(Action? append, CancellationToken token)
        {
            if (_disposedValue) return;
            if (_content.IsLoaded) return;

            try
            {
                _content.SetPicture(LoadPicture(_content.Entry, token));
                append?.Invoke();
            }
            finally
            {
                RaiseLoaded();
                _content.UpdateDevStatus();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// コンテンツロード
        /// </summary>
        public virtual async Task LoadContentAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            await LoadContentAsyncTemplate(() =>
            {
                // NOTE: リサイズフィルター有効の場合はBitmapSourceの生成をサイズ確定まで遅延させる
                if (!Config.Current.ImageResizeFilter.IsEnabled)
                {
                    PictureCreateBitmapSource(token);
                }
            },
            token);
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public void UnloadContent()
        {
            _content.SetPageMessage((PageMessage?)null);
            UnloadPicture();
            _content.UpdateDevStatus();

            MemoryControl.Current.GarbageCollect();
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        public virtual async Task LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (_disposedValue) return;

            await _content.Thumbnail.InitializeAsync(_content.Entry, null, token);
            if (_content.Thumbnail.IsValid) return;

            var source = LoadPictureSource(token);

            byte[]? thumbnailRaw = null;

            if (_content.PageMessage != null)
            {
                thumbnailRaw = null;
            }
            else
            {
                try
                {
                    thumbnailRaw = MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreateThumbnail(ThumbnailProfile.Current, token));
                }
                catch
                {
                    // NOTE: サムネイル画像取得失敗時はEnptyなサムネイル画像を適用する
                }
            }

            token.ThrowIfCancellationRequested();
            _content.Thumbnail.Initialize(thumbnailRaw);
        }

        public long GetMemorySize()
        {
            var source = PictureSource;
            return source != null ? source.GetMemorySize() : 0;
        }

        public void Unload()
        {
            UnloadPictureSource();
        }
    }

}
