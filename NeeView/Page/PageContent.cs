using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using NeeView.ComponentModel;
using NeeView.Threading;

namespace NeeView
{
    public abstract class PageContent : IDataSource, IMemoryElement
    {
        public static Size DefaultSize = new Size(480, 640);

        private ArchiveEntry _archiveEntry;
        private BookMemoryService? _bookMemoryService;
        private PictureInfo? _pictureInfo;
        private Size _size = DefaultSize;
        public PageContentState _state;
        private AsyncLock _asyncLock = new();
        private CancellationTokenSource? _cancellationTokenSource;


        public PageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
        {
            _archiveEntry = archiveEntry;

            _bookMemoryService = bookMemoryService;
        }


        public event EventHandler? ContentChanged;

        public event EventHandler? SizeChanged;


        public int Index { get; set; }

        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get => _state;
            set => _state = value;
        }

        [Obsolete("use Entry")]
        public ArchiveEntry ArchiveEntry => _archiveEntry;

        public ArchiveEntry Entry => _archiveEntry;

        public BookMemoryService? BookMemoryService => _bookMemoryService;

        /// <summary>
        /// Picture info.
        /// 一度だけ設定可能
        /// </summary>
        public PictureInfo? PictureInfo
        {
            get => _pictureInfo;
            private set => _pictureInfo = _pictureInfo ?? value;
        }

        /// <summary>
        /// Picture size
        /// </summary>
        public Size Size
        {
            get => _size;
            protected set
            {
                if (_size != value)
                {
                    _size = value;
                    Debug.Assert(_size.Width > 0);
                    SizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Color Color => PictureInfo?.Color ?? Colors.Black;

        public object? Data { get; private set; }
        public long DataSize { get; private set; }
        public string? ErrorMessage { get; private set; }

        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;

        public bool IsMemoryLocked => _state != PageContentState.None;



        public virtual async Task LoadAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync(token))
            {
                if (IsLoaded)
                {
                    return;
                }

                try
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);
                    if (linkedTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    var source = await LoadSourceAsync(token);
                    SetSource(source);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }

        }

        public abstract Task<PageSource> LoadSourceAsync(CancellationToken token);

        public virtual void Unload()
        {
            _cancellationTokenSource?.Cancel();

            Data = null;
            DataSize = 0;
            ErrorMessage = null;
        }


        private void SetSource(PageSource source)
        {
            Data = source.Data;
            DataSize = source.DataSize;
            ErrorMessage = source.ErrorMessage;
            PictureInfo = source.PictureInfo;

            if (GetMemorySize() > 0)
            {
                _bookMemoryService?.AddPageContent(this);
            }

            Size = PictureInfo?.Size ?? DefaultSize;
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }


        public virtual long GetMemorySize()
        {
            return DataSize;
        }

    }

}
