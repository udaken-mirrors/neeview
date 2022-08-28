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


        private DisposableCollection _disposables = new();


        private BookmarkFolderList() : base(false, false, Config.Current.Bookmark)
        {
            ApplicationDisposer.Current.Add(this);

            _disposables.Add(Config.Current.Bookmark.SubscribePropertyChanged(nameof(BookmarkConfig.IsSyncBookshelfEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsSyncBookshelfEnabled));
            }));
        }


        public override bool IsSyncBookshelfEnabled
        {
            get => Config.Current.Bookmark.IsSyncBookshelfEnabled;
            set => Config.Current.Bookmark.IsSyncBookshelfEnabled = value;
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

            [DataMember, DefaultValue(true)]
            public bool IsSyncBookshelfEnabled { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                FolderList?.RestoreConfig(config.Bookmark);
                Config.Current.Bookmark.IsSyncBookshelfEnabled = IsSyncBookshelfEnabled;
            }
        }

        #endregion
    }
}
