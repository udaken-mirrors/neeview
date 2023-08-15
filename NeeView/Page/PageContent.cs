using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using static System.Windows.Forms.AxHost;
using NeeView.ComponentModel;

namespace NeeView
{
    public class PageContent<T> : IPageContent, IDataSource<T>, IDataSource, IMemoryElement
    {
        public static Size DefaultSize = new Size(595, 842);

        private Size _size = DefaultSize;
        private ArchiveEntry _archiveEntry;

        private PageSource<T> _pageSource;
        private string? _errorMessage;
        private PictureInfo? _pictureInfo;

        private BookMemoryService? _bookMemoryService;


        // TODO: ArchiveEntryを渡すように
        public PageContent(ArchiveEntry archiveEntry, PageSource<T> pageSource, BookMemoryService? bookMemoryService)
        {
            _archiveEntry = archiveEntry;

            _pageSource = pageSource;
            _pageSource.SourceChanged += PageSource_SourceChanged;

            _bookMemoryService = bookMemoryService;
        }


        public event EventHandler? ContentChanged;

        public event EventHandler? SizeChanged;


        public int Index { get; set; }

        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState _state;
        public PageContentState State
        {
            get => _state;
            set => _state = value;
        }


        public ArchiveEntry ArchiveEntry => _archiveEntry;
        public ArchiveEntry Entry => _archiveEntry;

        public PictureInfo? PictureInfo => _pictureInfo;


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

        public Color Color => _pictureInfo?.Color ?? Colors.Black;

        public T? Data => _pageSource.Data;
        public long DataSize => _pageSource.DataSize;
        public string? ErrorMessage => _errorMessage ?? _pageSource.ErrorMessage;
        public bool IsLoaded => _pageSource.IsLoaded || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        object? IDataSource.Data => Data;

        public bool IsMemoryLocked => _state != PageContentState.None;


        public virtual async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;
            await _pageSource.LoadAsync(token);
        }

        public virtual void Unload()
        {
            _pageSource.Unload();
        }


        protected void RaiseContentChanged(object? sender, EventArgs e)
        {
            ContentChanged?.Invoke(sender, e);
        }

        protected void RaiseSizeChanged(object? sender, EventArgs e)
        {
            SizeChanged?.Invoke(sender, e);
        }

        protected void SetErrorMessage(string? errorMessage)
        {
            _errorMessage = errorMessage;
        }

        protected void SetPictureInfo(PictureInfo? pictureInfo)
        {
            _pictureInfo = pictureInfo;
        }

        protected virtual void OnPageSourceChanged()
        {
        }


        private void PageSource_SourceChanged(object? sender, EventArgs e)
        {
            if (GetMemorySize() > 0)
            {
                _bookMemoryService?.AddPageContent(this);
            }

            OnPageSourceChanged();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }


        public virtual long GetMemorySize()
        {
            return _pageSource.DataSize;
        }





        /// <summary>
        /// テンポラリファイル
        /// </summary>
        public FileProxy? FileProxy { get; private set; }

        /// <summary>
        /// テンポラリファイルの作成
        /// </summary>
        /// <param name="isKeepFileName">エントリ名準拠のテンポラリファイルを作成</param>
        public FileProxy CreateTempFile(bool isKeepFileName)
        {
            //ThrowIfDisposed();

            FileProxy = FileProxy ?? Entry.ExtractToTemp(isKeepFileName);
            return FileProxy;
        }
    }

}
