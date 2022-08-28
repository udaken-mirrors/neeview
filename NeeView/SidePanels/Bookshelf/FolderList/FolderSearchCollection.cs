using NeeView.IO;
using NeeLaboratory.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// 検索コレクション
    /// </summary>
    public class FolderSearchCollection : FolderCollection, IDisposable
    {
        private NeeLaboratory.IO.Search.SearchResultWatcher _searchResult;
        private FolderCollectionEngine? _engine;
        private bool _isWatchSearchResult;


        public FolderSearchCollection(QueryPath path, NeeLaboratory.IO.Search.SearchResultWatcher searchResult, bool isWatchSearchResult, bool isOverlayEnabled) : base(path, isOverlayEnabled)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));
            Debug.Assert(path.Search == searchResult.Keyword);

            _searchResult = searchResult;
            _isWatchSearchResult = isWatchSearchResult;

            if (_isWatchSearchResult)
            {
                _engine = new FolderCollectionEngine(this);
            }
        }


        public override bool IsSearchEnabled => true;

        public override FolderOrderClass FolderOrderClass => FolderOrderClass.WithPath;



        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            await Task.Run(() => InitializeItems(token));
        }

        private void InitializeItems(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            var items = _searchResult.Items
                .Select(e => CreateFolderItem(e))
                .WhereNotNull()
                .ToList();

            var list = Sort(items, token);

            if (!list.Any())
            {
                list.Add(_folderItemFactory.CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            if (_isWatchSearchResult)
            {
                _searchResult.SearchResultChanged += SearchResult_NodeChanged;
            }
        }


        public override void RequestCreate(QueryPath path)
        {
            if (_disposedValue) return;

            _engine?.RequestCreate(path);
        }

        public override void RequestDelete(QueryPath path)
        {
            if (_disposedValue) return;

            _engine?.RequestDelete(path);
        }

        public override void RequestRename(QueryPath oldPath, QueryPath path)
        {
            if (_disposedValue) return;

            _engine?.RequestRename(oldPath, path);
        }

        private void SearchResult_NodeChanged(object? sender, NeeLaboratory.IO.Search.SearchResultChangedEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case NeeLaboratory.IO.Search.NodeChangedAction.Add:
                    RequestCreate(new QueryPath(e.Content.Path));
                    break;
                case NeeLaboratory.IO.Search.NodeChangedAction.Remove:
                    RequestDelete(new QueryPath(e.Content.Path));
                    break;
                case NeeLaboratory.IO.Search.NodeChangedAction.Rename:
                    var rename = (NeeLaboratory.IO.Search.SearchResultRenamedEventArgs)e;
                    RequestRename(new QueryPath(rename.OldPath), new QueryPath(e.Content.Path));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// 検索結果からFolderItem作成
        /// </summary>
        private FolderItem? CreateFolderItem(NeeLaboratory.IO.Search.NodeContent nodeContent)
        {
            if (nodeContent.FileInfo.IsDirectory)
            {
                return CreateFolderItemDirectory(nodeContent);
            }
            else
            {
                return CreateFolderItemFile(nodeContent);
            }
        }

        private FolderItem CreateFolderItemDirectory(NeeLaboratory.IO.Search.NodeContent nodeContent)
        {
            return new FileFolderItem(_isOverlayEnabled)
            {
                Type = FolderItemType.Directory,
                Place = Place,
                Name = Path.GetFileName(nodeContent.Path),
                TargetPath = new QueryPath(nodeContent.Path),
                LastWriteTime = nodeContent.FileInfo.LastWriteTime,
                Length = -1,
                Attributes = FolderItemAttribute.Directory,
                IsReady = true
            };
        }

        private FolderItem? CreateFolderItemFile(NeeLaboratory.IO.Search.NodeContent nodeContent)
        {
            if (FileShortcut.IsShortcut(nodeContent.Path))
            {
                var shortcut = new FileShortcut(nodeContent.Path);
                return _folderItemFactory.CreateFolderItem(shortcut);
            }

            var archiveType = ArchiverManager.Current.GetSupportedType(nodeContent.Path);
            if (archiveType != ArchiverType.None)
            {
                var item = new FileFolderItem(_isOverlayEnabled)
                {
                    Type = FolderItemType.File,
                    Place = Place,
                    Name = Path.GetFileName(nodeContent.Path),
                    TargetPath = new QueryPath(nodeContent.Path),
                    LastWriteTime = nodeContent.FileInfo.LastWriteTime,
                    Length = nodeContent.FileInfo.Size,
                    IsReady = true
                };
                if (archiveType == ArchiverType.PlaylistArchiver)
                {
                    item.Type = FolderItemType.Playlist;
                    item.Attributes = FolderItemAttribute.Playlist;
                    item.Length = -1;
                }
                return item;
            }

            return null;
        }


        #region IDisposable Support

        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_engine != null)
                    {
                        _engine.Dispose();
                    }

                    _searchResult.Dispose();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
