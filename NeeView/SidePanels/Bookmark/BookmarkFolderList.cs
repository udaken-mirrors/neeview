using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// BookmarkFolderList
    /// </summary>
    public class BookmarkFolderList : FolderList, IDisposable
    {
        static BookmarkFolderList() => Current = new BookmarkFolderList();
        public static BookmarkFolderList Current { get; }


        private readonly DisposableCollection _disposables = new();


        private BookmarkFolderList() : base(false, false, Config.Current.Bookmark)
        {
            ApplicationDisposer.Current.Add(this);

            _disposables.Add(Config.Current.Bookmark.SubscribePropertyChanged(nameof(BookmarkConfig.IsSyncBookshelfEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsSyncBookshelfEnabled));
            }));

            this.SearchBoxModel = new SearchBoxModel(new BookmarkSearchBoxComponent(this));
        }


        public override bool IsSyncBookshelfEnabled
        {
            get => Config.Current.Bookmark.IsSyncBookshelfEnabled;
            set => Config.Current.Bookmark.IsSyncBookshelfEnabled = value;
        }

        protected override bool IsSearchIncludeSubdirectories
        {
            get => Config.Current.Bookmark.IsSearchIncludeSubdirectories;
        }

        public void UpdateItems()
        {
            if (_disposedValue) return;

            if (FolderCollection == null)
            {
                RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.None);
            }
        }

        public override bool CanMoveToParent()
        {
            if (_disposedValue) return false;

            var parentQuery = FolderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return parentQuery.Scheme == QueryScheme.Bookmark;
        }

        public override void Sync()
        {
        }

        public override QueryPath GetFixedHome()
        {
            return new QueryPath(QueryScheme.Bookmark, null, null);
        }

        protected override bool CheckScheme(QueryPath query)
        {
            if (query.Scheme != QueryScheme.Bookmark) throw new NotSupportedException($"need scheme \"{QueryScheme.Bookmark.ToSchemeString()}\"");
            return true;
        }

        #region IDisposable support

        private bool _disposedValue;

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


        /// <summary>
        /// 検索ボックスコンポーネント
        /// </summary>
        public class BookmarkSearchBoxComponent : ISearchBoxComponent
        {
            private readonly BookmarkFolderList _self;

            public BookmarkSearchBoxComponent(BookmarkFolderList self)
            {
                _self = self;
            }

            public HistoryStringCollection? History => BookHistoryCollection.Current.BookmarkSearchHistory;

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public void Search(string keyword) => _self.RequestSearchPlace(keyword, false);
        }
    }
}
