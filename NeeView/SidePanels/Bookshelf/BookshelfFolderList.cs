using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// BookshelfFolderList
    /// </summary>
    public class BookshelfFolderList : FolderList, IDisposable
    {
        static BookshelfFolderList() => Current = new BookshelfFolderList();
        public static BookshelfFolderList Current { get; }


        private FolderItem? _visibledItem;
        private Regex? _excludeRegex;
        private DisposableCollection _disposables = new();


        private BookshelfFolderList() : base(true, true, Config.Current.Bookshelf)
        {
            History = new BookshelfFolderHistory(this);

            ApplicationDisposer.Current.Add(this);

            _disposables.Add(Config.Current.System.SubscribePropertyChanged(nameof(SystemConfig.IsHiddenFileVisibled), async (s, e) =>
            {
                await RefreshAsync(true, true);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.IsVisibleHistoryMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.IsVisibleBookmarkMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.ExcludePattern), (s, e) =>
            {
                UpdateExcludeRegex();
            }));

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.IsSearchIncludeSubdirectories), (s, e) =>
            {
                RequestSearchPlace(true);
            }));


            _disposables.Add(BookOperation.Current.SubscribeBookChanging((s, e) =>
            {
                UpdateVisibledItem(e.Address, false);
            }));

            _disposables.Add(BookOperation.Current.SubscribeBookChanged((s, e) =>
            {
                UpdateVisibledItem(BookOperation.Current.Address, false);
            }));

            _disposables.Add(this.SubscribeCollectionChanged((s, e) =>
            {
                UpdateVisibledItem(BookOperation.Current.Address, true);
            }));


            UpdateExcludeRegex();
        }


        // フォルダー履歴
        public BookshelfFolderHistory History { get; }

        // 除外パターンの正規表現
        public Regex? ExcludeRegex
        {
            get { return _excludeRegex; }
            set { SetProperty(ref _excludeRegex, value); }
        }

        /// <summary>
        /// 現在ブックマーク更新
        /// </summary>
        private void UpdateVisibledItem(string? path, bool force)
        {
            if (_disposedValue) return;

            if (force && _visibledItem != null)
            {
                _visibledItem.IsVisibled = false;
                _visibledItem = null;
            }

            if (_visibledItem != null && _visibledItem.EntityPath.SimplePath == path)
            {
                return;
            }

            var item = FolderCollection != null
                ? FolderCollection.Items.FirstOrDefault(x => x.TargetPath.SimplePath == path) ?? FolderCollection.Items.FirstOrDefault(x => x.EntityPath.SimplePath == path)
                : null;

            if (_visibledItem != item)
            {
                if (_visibledItem != null)
                {
                    _visibledItem.IsVisibled = false;
                }
                _visibledItem = item;
                if (_visibledItem != null)
                {
                    _visibledItem.IsVisibled = true;
                }
            }
        }

        internal void ToggleVisibleFoldersTree()
        {
            if (_disposedValue) return;

            var command = CommandTable.Current.GetElement("ToggleVisibleFoldersTree");
            if (command.CanExecute(this, CommandArgs.Empty))
            {
                command.Execute(this, CommandArgs.Empty);
            }
        }

        public override QueryPath GetFixedHome()
        {
            var path = new QueryPath(Config.Current.Bookshelf.Home);

            switch (path.Scheme)
            {
                case QueryScheme.Root:
                    return path;

                case QueryScheme.File:
                    if (Directory.Exists(path.SimplePath))
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(GetDefaultHomePath());
                    }

                case QueryScheme.Bookmark:
                    if (BookmarkCollection.Current.FindNode(path.SimplePath)?.Value is BookmarkFolder)
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(QueryScheme.Bookmark, null, null);
                    }

                default:
                    Debug.WriteLine($"Not support yet: {Config.Current.Bookshelf.Home}");
                    return new QueryPath(GetDefaultHomePath());
            }
        }

        public static string GetDefaultHomePath()
        {
            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture))
            {
                return myPicture;
            }

            // 救済措置
            return System.Environment.CurrentDirectory;
        }

        public override async void Sync()
        {
            if (_disposedValue) return;

            var book = BookHub.Current?.GetCurrentBook();
            var address = book?.Path;

            if (address != null)
            {
                // TODO: Queryの求め方はこれでいいのか？
                var path = new QueryPath(address);
                var parent = new QueryPath(book?.Source.GetFolderPlace() ?? LoosePath.GetDirectoryName(address));

                SetDarty(); // 強制更新
                await SetPlaceAsync(parent, new FolderItemPosition(path), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.FileSystem);

                RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                SetDarty(); // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.FileSystem);

                RaiseSelectedItemChanged(true);
            }

            if (Config.Current.Bookshelf.IsSyncFolderTree && Place != null)
            {
                BookshelfFolderTreeModel.Current?.SyncDirectory(Place.SimplePath);
            }
        }

        protected override void CloseBookIfNecessary()
        {
            if (_disposedValue) return;

            if (Config.Current.Bookshelf.IsCloseBookWhenMove)
            {
                BookHub.Current.RequestUnload(this, true);
            }
        }

        protected override bool IsCruise()
        {
            return Config.Current.Bookshelf.IsCruise;
        }

        // 除外パターンの正規表現を更新
        private void UpdateExcludeRegex()
        {
            if (_disposedValue) return;

            try
            {
                ExcludeRegex = string.IsNullOrWhiteSpace(Config.Current.Bookshelf.ExcludePattern) ? null : new Regex(Config.Current.Bookshelf.ExcludePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FolderList exclute: {ex.Message}");
                ExcludeRegex = null;
            }
        }

        protected override bool IsIncrementalSearchEnabled()
        {
            return Config.Current.Bookshelf.IsIncrementalSearchEnabled;
        }

        protected override bool IsSearchIncludeSubdirectories()
        {
            return Config.Current.Bookshelf.IsSearchIncludeSubdirectories;
        }

        protected override void OnPlaceChanged(object? sender, FolderSetPlaceOption options)
        {
            if (_disposedValue) return;

            base.OnPlaceChanged(sender, options);

            if (options.HasFlag(FolderSetPlaceOption.UpdateHistory))
            {
                var place = Place?.ReplaceSearch(null);
                if (place is null) return;
                this.History?.Add(place);
            }
        }

        #region FolderHistory

        public bool CanMoveToPrevious()
        {
            if (_disposedValue) return false;

            return this.History.CanMoveToPrevious();
        }

        public override async void MoveToPrevious()
        {
            if (_disposedValue) return;

            await this.History.MoveToPreviousAsync();
        }

        public bool CanMoveToNext()
        {
            if (_disposedValue) return false;

            return this.History.CanMoveToNext();
        }

        public override async void MoveToNext()
        {
            if (_disposedValue) return;

            await this.History.MoveToNextAsync();
        }

        public async void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            if (_disposedValue) return;

            await this.History.MoveToHistoryAsync(item);
        }

        // NOTE: Historyから呼ばれる
        public async Task MoveToHistoryAsync(QueryPath path)
        {
            if (_disposedValue) return;

            await SetPlaceAsync(path, null, FolderSetPlaceOption.Focus);
            CloseBookIfNecessary();
        }

        #endregion FolderHistory

        #region IDisposable support

        private bool _disposedValue;

        private new void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        #endregion IDisposable support


        #region Memento

        [DataContract]
        public new class Memento
        {
            [DataMember]
            public FolderList.Memento? FolderList { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string? Home { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsMultipleRarFilterEnabled { get; set; }

            [DataMember]
            public string? ExcludePattern { get; set; }

            [DataMember]
            public bool IsCruise { get; set; }

            [DataMember]
            public bool IsCloseBookWhenMove { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsIncrementalSearchEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSearchIncludeSubdirectories { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsOpenNextBookWhenRemove { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                FolderList?.RestoreConfig(config.Bookshelf);

                config.Bookshelf.Home = Home ?? "";
                config.Bookshelf.IsVisibleHistoryMark = IsVisibleHistoryMark;
                config.Bookshelf.IsVisibleBookmarkMark = IsVisibleBookmarkMark;
                config.Bookshelf.IsSyncFolderTree = FolderList?.IsSyncFolderTree ?? false;
                config.Bookshelf.IsCloseBookWhenMove = IsCloseBookWhenMove;
                config.Bookshelf.IsOpenNextBookWhenRemove = IsOpenNextBookWhenRemove;
                config.Bookshelf.IsInsertItem = IsInsertItem;
                config.Bookshelf.IsMultipleRarFilterEnabled = IsMultipleRarFilterEnabled;
                config.Bookshelf.ExcludePattern = ExcludePattern;
                config.Bookshelf.IsCruise = IsCruise;
                config.Bookshelf.IsIncrementalSearchEnabled = IsIncrementalSearchEnabled;
                config.Bookshelf.IsSearchIncludeSubdirectories = IsSearchIncludeSubdirectories;
            }
        }

        #endregion
    }

}
