using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace NeeView
{
    public class BookPageProperty : BindableBase, IDisposable, IBookPageProperty
    {
        private Book _book;
        private bool _disposedValue;

        public BookPageProperty(Book book)
        {
            _book = book;

            _book.Pages.PagesSorted += Book_PagesSorted;
            _book.Pages.PageRemoved += Book_PageRemoved;
            _book.Viewer.Loader.ViewContentsChanged += Book_ViewContentsChanged;
            _book.Viewer.SelectedRangeChanged += Book_SelectedRangeChanged;

            UpdatePageList();
        }



        // ページが変更された
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;

        // ページリストが変更された
        public event EventHandler? PageListChanged;

        // 選択項目変更
        public event EventHandler<SelectedRangeChangedEventArgs>? SelectedItemChanged;



        public int SelectedIndex
        {
            get => _book.Viewer.SelectedRange.Min.Index;
            set => throw new NotImplementedException();
        }

        public int MaxIndex
        {
            get => _book.Pages.Count <= 0 ? 0 : _book.Pages.Count - 1;
        }



        public bool IsBusy
        {
            get { return _book.Viewer.Loader.IsBusy; }
        }

        public IReadOnlyList<Page>? PageList => _book.Pages;

        public PageSortModeClass PageSortModeClass
        {
            get { return _book.PageSortModeClass; }
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _book.Pages.PagesSorted -= Book_PagesSorted;
                    _book.Pages.PageRemoved -= Book_PageRemoved;
                    _book.Viewer.Loader.ViewContentsChanged -= Book_ViewContentsChanged;
                    _book.Viewer.SelectedRangeChanged -= Book_SelectedRangeChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Book_PagesSorted(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() => UpdatePageList());
        }

        private void Book_PageRemoved(object? sender, PageRemovedEventArgs e)
        {
            AppDispatcher.Invoke(() => UpdatePageList());
        }

        private void Book_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            AppDispatcher.Invoke(() => ViewContentsChanged?.Invoke(sender, e));
        }

        private void Book_SelectedRangeChanged(object? sender, SelectedRangeChangedEventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                RaisePropertyChanged(nameof(SelectedIndex));
                SelectedItemChanged?.Invoke(sender, e);
            });
        }


        // 現在ベージ取得
        public Page? GetPage()
        {
            return _book.Viewer.GetViewPage();
        }

        // 現在ページ番号取得
        public int GetPageIndex()
        {
            return _book.Viewer.SelectedRange.Min.Index;
        }

        /// <summary>
        /// 最大ページ番号取得
        /// </summary>
        /// <returns></returns>
        public int GetMaxPageIndex()
        {
            var count = _book.Pages.Count - 1;
            if (count < 0) count = 0;
            return count;
        }

        /// <summary>
        /// ページ数取得
        /// </summary>
        /// <returns></returns>
        public int GetPageCount()
        {
            return _book.Pages.Count;
        }


        // ページリスト更新
        private void UpdatePageList()
        {
            RaisePropertyChanged(nameof(PageList));
            PageListChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}
