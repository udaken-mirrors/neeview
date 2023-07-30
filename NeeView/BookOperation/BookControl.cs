using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace NeeView
{
    public class BookControl : BindableBase, IBookControl, IDisposable
    {
        private Book _book;
        private bool _disposedValue;

        public BookControl(Book book)
        {
            _book = book;
            _book.Viewer.Loader.PropertyChanged += BookLoader_PropertyChanged;
        }


        // ブックマーク判定
        public bool IsBookmark => BookmarkCollection.Current.Contains(_book.Path);

        public bool IsBusy => _book.Viewer.Loader.IsBusy;
        public PageSortModeClass PageSortModeClass => _book.PageSortModeClass;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _book.Viewer.Loader.PropertyChanged -= BookLoader_PropertyChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void BookLoader_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsBusy):
                    RaisePropertyChanged(nameof(IsBusy));
                    break;
            }
        }

        /// <summary>
        /// ブックの再読み込み
        /// </summary>
        public void ReLoad()
        {
            var book = _book;
            if (book is null) return;

            var viewPage = book.Viewer.GetViewPage();
            var page = book.Pages.GetValidPage(viewPage);
            BookHub.Current.RequestReLoad(this, page?.EntryName);
        }

        /// <summary>
        /// 削除された可能性のあるページの処理
        /// </summary>
        /// <remarks>
        /// 主にドラッグ処理の後始末
        /// </remarks>
        /// <param name="pages">削除された可能性のあるページ</param>
        public void ValidateRemoveFile(IEnumerable<Page> pages)
        {
            if (pages.All(e => e.Entry.Exists())) return;
            ReLoad();
        }


        // 現在表示しているブックの削除可能？
        public bool CanDeleteBook()
        {
            return Config.Current.System.IsFileWriteAccessEnabled && _book != null && (_book.LoadOption & BookLoadOption.Undeliteable) == 0 && (File.Exists(_book.SourcePath) || Directory.Exists(_book.SourcePath));
        }

        // 現在表示しているブックを削除する
        public async void DeleteBook()
        {
            if (CanDeleteBook())
            {
                var bookAddress = _book?.SourcePath;
                if (bookAddress is null) return;

                var item = BookshelfFolderList.Current.FindFolderItem(bookAddress);
                if (item != null)
                {
                    await BookshelfFolderList.Current.RemoveAsync(item);
                }
                else if (FileIO.ExistsPath(bookAddress))
                {
                    var entry = StaticFolderArchive.Default.CreateArchiveEntry(bookAddress);
                    await ConfirmFileIO.DeleteAsync(entry, Resources.FileDeleteBookDialog_Title, null);
                }
            }
        }

        #region BookCommand : ブックマーク

        // ブックマーク登録可能？
        public bool CanBookmark()
        {
            return true;
        }

        // ブックマーク設定
        public void SetBookmark(bool isBookmark)
        {
            if (CanBookmark())
            {
                var query = new QueryPath(_book.Path);

                if (isBookmark)
                {
                    // ignore temporary directory
                    if (_book.Path.StartsWith(Temporary.Current.TempDirectory))
                    {
                        ToastService.Current.Show(new Toast(Resources.Bookmark_Message_TemporaryNotSupportedError, "", ToastIcon.Error));
                        return;
                    }

                    BookmarkCollectionService.Add(query);
                }
                else
                {
                    BookmarkCollectionService.Remove(query);
                }

                RaisePropertyChanged(nameof(IsBookmark));
            }
        }

        // ブックマーク切り替え
        public void ToggleBookmark()
        {
            if (CanBookmark())
            {
                SetBookmark(!IsBookmark);
            }
        }

        #endregion
    }




}