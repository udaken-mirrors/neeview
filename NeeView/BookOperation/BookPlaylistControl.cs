using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace NeeView
{
    public class BookPlaylistControl : BindableBase, IDisposable, IBookPlaylistControl
    {
        private Book _book;
        private bool _disposedValue;

        public BookPlaylistControl(Book book)
        {
            Debug.Assert(!book.IsMedia);
            _book = book;

            PlaylistHub.Current.PlaylistCollectionChanged += Playlist_CollectionChanged;
            //_book.Viewer.Loader.ViewContentsChanged += BookLoader_ViewContentsChanged;
            _book.Pages.PagesSorted += Book_PagesSorted;
            _book.Pages.PageRemoved += Book_PageRemoved;
        }




        // プレイリストに追加、削除された
        public event EventHandler? MarkersChanged;

        //public IDisposable SubscribeMarkersChanged(EventHandler handler)
        //{
        //    MarkersChanged += handler;
        //    return new AnonymousDisposable(() => MarkersChanged -= handler);
        //}


        // 表示ページのマーク判定
        public bool IsMarked
        {
            get
            {
#warning 未実装
                return false;
#if false
                var page = _book.Viewer.GetViewPage();
                if (page is null) return false;
                return _book.Marker.IsMarked(page);
#endif
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    PlaylistHub.Current.PlaylistCollectionChanged -= Playlist_CollectionChanged;
                    //_book.Viewer.Loader.ViewContentsChanged -= BookLoader_ViewContentsChanged;
                    _book.Pages.PagesSorted -= Book_PagesSorted;
                    _book.Pages.PageRemoved -= Book_PageRemoved;
                    MarkersChanged = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        private void Book_PageRemoved(object? sender, PageRemovedEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                // プレイリストから削除
                var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
                bookPlaylist.Remove(e.Pages);

                RaisePropertyChanged(nameof(IsMarked));
            });
        }

        private void Book_PagesSorted(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(IsMarked)));
        }

#warning ページ変更時処理未実装
#if false
        private void BookLoader_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            AppDispatcher.Invoke(() => RaisePropertyChanged(nameof(IsMarked)));
        }
#endif

        private void Playlist_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //NvDebug.AssertSTA();

            var newItems = e.NewItems?.Cast<PlaylistItem>();
            var oldItems = e.OldItems?.Cast<PlaylistItem>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    UpdateMarkers();
                    break;

                case NotifyCollectionChangedAction.Add:
                    if (newItems is null) throw new InvalidOperationException();
                    if (newItems.Any(x => x.Path.StartsWith(_book.Path)))
                    {
                        UpdateMarkers();
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (oldItems is null) throw new InvalidOperationException();
                    if (oldItems.Any(x => x.Path.StartsWith(_book.Path)))
                    {
                        UpdateMarkers();
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (newItems is null) throw new InvalidOperationException();
                    if (oldItems is null) throw new InvalidOperationException();
                    if (!oldItems.SequenceEqual(newItems) && oldItems.Union(newItems).Any(x => x.Path.StartsWith(_book.Path)))
                    {
                        UpdateMarkers();
                    }
                    break;
            }
        }


        // ページマーク登録可能？
        public bool CanMark()
        {
#warning 未実装
            return false;
#if false
            var page = _book.Viewer.GetViewPage();
            if (page is null) return false;
            return CanMark(page);
#endif
        }

        public bool CanMark(Page page)
        {
            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            return bookPlaylist.IsEnabled(page);
        }

        // マーカー追加/削除
        public PlaylistItem? SetMark(bool isMark)
        {
#warning 未実装
            return null;
#if false
            var page = _book.Viewer.GetViewPage();
            if (page is null) return null;

            if (!CanMark(page))
            {
                return null;
            }

            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            return bookPlaylist.Set(page, isMark);
#endif
        }

        // マーカー切り替え
        public PlaylistItem? ToggleMark()
        {
#warning 未実装
            return null;
#if false
            var page = _book.Viewer.GetViewPage();
            if (page is null) return null;

            if (!CanMark(page))
            {
                return null;
            }

            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            return bookPlaylist.Toggle(page);
#endif
        }

        #region 開発用

        /// <summary>
        /// (開発用) たくさんのページマーク作成
        /// </summary>
        [Conditional("DEBUG")]
        public void Test_MakeManyMarkers()
        {
            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);

            for (int index = 0; index < _book.Pages.Count; index += 100)
            {
                var page = _book.Pages[index];
                bookPlaylist.Add(page);
            }
        }

        #endregion

        // マーカー表示更新
        public void UpdateMarkers()
        {
            // 本にマーカを設定
            var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
            var pages = bookPlaylist.Collect();

            _book.Marker.SetMarkers(pages);

            // 表示更新
            MarkersChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(IsMarked));
        }

        public bool CanPrevMarkInPlace(MovePlaylsitItemInBookCommandParameter param)
        {
            return (_book.Marker.Markers != null && _book.Marker.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        public bool CanNextMarkInPlace(MovePlaylsitItemInBookCommandParameter param)
        {
            return (_book.Marker.Markers != null && _book.Marker.Markers.Count > 0) || param.IsIncludeTerminal;
        }

        public void PrevMarkInPlace(object? sender, MovePlaylsitItemInBookCommandParameter param)
        {
#warning 未実装
#if false
            var result = _book.Control.JumpToMarker(this, -1, param.IsLoop, param.IsIncludeTerminal);
            if (result != null)
            {
                var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
                var item = bookPlaylist.Find(result);
                PlaylistPresenter.Current?.PlaylistListBox?.SetSelectedItem(item);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_FirstPlaylistItem);
            }
#endif
        }

        public void NextMarkInPlace(object? sender, MovePlaylsitItemInBookCommandParameter param)
        {
#warning 未実装
#if false
            var result = _book.Control.JumpToMarker(this, +1, param.IsLoop, param.IsIncludeTerminal);
            if (result != null)
            {
                var bookPlaylist = new BookPlaylist(_book, PlaylistHub.Current.Playlist);
                var item = bookPlaylist.Find(result);
                PlaylistPresenter.Current?.PlaylistListBox?.SetSelectedItem(item);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_LastPlaylistItem);
            }
#endif
        }

    }

}