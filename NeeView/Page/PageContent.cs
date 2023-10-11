﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using NeeView.ComponentModel;
using NeeView.Threading;
using System.ComponentModel;
using NeeLaboratory.Generators;

namespace NeeView
{
    [NotifyPropertyChanged]
    public abstract partial class PageContent : IDataSource, IMemoryElement, INotifyPropertyChanged
    {
        public static Size DefaultSize { get; } = new(480, 640);

        /// <summary>
        /// サイズ取得前のサイズ
        /// </summary>
        /// <reamerks>
        /// 直前の選択ページのサイズが保持される。
        /// サイズ確定前のLoadingページのサイズになる。
        /// </reamerks>
        /// 
        public static Size UndefinedSize { get; set; } = DefaultSize;

        private readonly ArchiveEntry _archiveEntry;
        private readonly BookMemoryService? _bookMemoryService;
        private PictureInfo? _pictureInfo;
        private Size? _size;
        public PageContentState _state;
        private readonly AsyncLock _asyncLock = new();
        private readonly object _lock = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private object? _data;
        private long _dataSize;
        private string? _errorMessage;


        public PageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
        {
            _archiveEntry = archiveEntry;
            _bookMemoryService = bookMemoryService;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        
        public event EventHandler? ContentChanged;
        
        public event EventHandler? SizeChanged;


        public int Index { get; set; }

        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public virtual bool IsFileContent => false;

        public ArchiveEntry ArchiveEntry => _archiveEntry;

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
            get { return _size ?? UndefinedSize; }
            protected set
            {
                if (SetProperty(ref _size, value))
                {
                    Debug.Assert(_size is not null && _size.Value.Width > 0);
                    SizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Color Color => PictureInfo?.Color ?? Colors.Black;

        public object? Data
        {
            get { return _data; }
            private set { SetProperty(ref _data, value); }
        }

        public long DataSize
        {
            get { return _dataSize; }
            private set { SetProperty(ref _dataSize, value); }
        }

        public string? ErrorMessage
        {
            get { return _errorMessage; }
            private set { SetProperty(ref _errorMessage, value); }
        }

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

            lock (_lock)
            {
                Data = null;
                DataSize = 0;
                ErrorMessage = null;
            }
        }


        private void SetSource(PageSource source)
        {
            if (source.PictureInfo is not null && source.PictureInfo.Size.IsEmptyArea())
            {
                SetSource(PageSource.CreateError("Image area is empty"));
                return;
            }

            lock (_lock)
            {
                Data = source.Data;
                DataSize = source.DataSize;
                ErrorMessage = source.ErrorMessage;
                PictureInfo = source.PictureInfo;

                if (GetMemorySize() > 0)
                {
                    _bookMemoryService?.AddPageContent(this);
                }

                Size = PictureInfo?.Size ?? UndefinedSize;
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        public DataSource CreateDataSource()
        {
            lock (_lock)
            {
                return new DataSource(Data, DataSize, ErrorMessage);
            }
        }


        public virtual long GetMemorySize()
        {
            return DataSize;
        }

    }

}
