using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// スライダーやフィルムストリップと連動したページ選択の提供
    /// </summary>
    public class PageSelector : BindableBase
    {
        static PageSelector() => Current = new PageSelector();
        public static PageSelector Current { get; }


        private int _selectedIndex;


        private PageSelector()
        {
            BookOperation.Current.BookChanging += BookOperation_BookChanging;
            BookOperation.Current.BookChanged += BookOperation_BookChanged;
            BookOperation.Current.Control.PagesChanged += BookOperation_PageListChanged;
            //BookOperation.Current.Property.ViewContentsChanged += BookOperation_ViewContentsChanged;
            BookOperation.Current.Control.SelectedRangeChanged += BookOperation_SelectedRangeChanged; ;
        }



        // NOTE: ChangingとChangedは必ずしもペアではない
        public event EventHandler? CollectionChanging;

        public IDisposable SubscribeCollectionChanging(EventHandler handler)
        {
            CollectionChanging += handler;
            return new AnonymousDisposable(() => CollectionChanging -= handler);
        }

        public event EventHandler? CollectionChanged;

        public IDisposable SubscribeCollectionChanged(EventHandler handler)
        {
            CollectionChanged += handler;
            return new AnonymousDisposable(() => CollectionChanged -= handler);
        }

        public event EventHandler? SelectionChanged;

        public IDisposable SubscribeSelectionChanged(EventHandler handler)
        {
            SelectionChanged += handler;
            return new AnonymousDisposable(() => SelectionChanged -= handler);
        }

#if false
        public event EventHandler<ViewContentsChangedEventArgs>? ViewContentsChanged;

        public IDisposable SubscribeViewContentsChanged(EventHandler<ViewContentsChangedEventArgs> handler)
        {
            ViewContentsChanged += handler;
            return new AnonymousDisposable(() => ViewContentsChanged -= handler);
        }
#endif

        public event EventHandler<SelectedPagesChangedEventArgs>? SelectedPagesChanged;

        public IDisposable SubscribeSelectedPagesChanged(EventHandler<SelectedPagesChangedEventArgs> handler)
        {
            SelectedPagesChanged += handler;
            return new AnonymousDisposable(() => SelectedPagesChanged -= handler);
        }




        public PageMode PageMode => BookOperation.Current.Book?.Setting.PageMode ?? PageMode.SinglePage;

        public bool IsSupportedSingleFirstPage => BookOperation.Current.Book?.Setting.IsSupportedSingleFirstPage ?? false;
        
        public bool IsSupportedSingleLastPage => BookOperation.Current.Book?.Setting.IsSupportedSingleLastPage ?? false;

        public int ViewPageCount => BookOperation.Current.Book?.Pages.Count ?? 0;

        public int MaxIndex => Math.Max(BookOperation.Current.Control.Pages.Count - 1, 0);

        public int SelectedIndex
        {
            get { return _selectedIndex; }
        }

        public Page? SelectedItem
        {
            get
            {
                var book = BookOperation.Current.Book;
                if (book is null || _selectedIndex < 0 || book.Pages.Count <= _selectedIndex) return null;
                return book.Pages[_selectedIndex];
            }
        }


        internal void FlushSelectedIndex(object sender)
        {
            SetSelectedIndex(sender, BookOperation.Current.Control.SelectedRange.Min.Index, true);
        }

        public bool SetSelectedIndex(object? sender, int value, bool raiseChangedEvent)
        {
            if (SetProperty(ref _selectedIndex, value, nameof(SelectedIndex)))
            {
                ////Debug.WriteLine($"> PageSelector.SelectedIndex={_selectedIndex}");

                if (raiseChangedEvent)
                {
                    SelectionChanged?.Invoke(sender, EventArgs.Empty);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Jump(object sender)
        {
            ////Debug.WriteLine($"Jump: {_selectedIndex}");
            BookOperation.Current.Control.MoveTo(sender, _selectedIndex);
        }

        private void BookOperation_BookChanging(object? sender, BookChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, EventArgs.Empty);
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            UpdateCollection(sender);
        }

        private void BookOperation_PageListChanged(object? sender, EventArgs e)
        {
            if (BookOperation.Current.IsLoading) return;
            UpdateCollection(sender);
        }

        private void UpdateCollection(object? sender)
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(MaxIndex));
            //RaiseViewContentsChanged(sender, BookOperation.Current.Book?.Viewer.ViewPageCollection, true);
            RaiseViewContentsChanged(sender, BookOperation.Current.Control.SelectedRange, true);
        }

        //private void BookOperation_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        //{
        //    RaiseViewContentsChanged(sender, e?.ViewPageCollection, false);
        //}

        private void BookOperation_SelectedRangeChanged(object? sender, SelectedRangeChangedEventArgs e)
        {
            RaiseViewContentsChanged(sender, BookOperation.Current.Control.SelectedRange, false);
        }

        private void RaiseViewContentsChanged(object? sender, PageRange range, bool isBookOpen)
        {
            var pages = CollectPage(BookOperation.Current.Book, range);
            if (pages is null) return;

            SelectedPagesChanged?.Invoke(sender, new SelectedPagesChangedEventArgs(pages));

            SetSelectedIndex(sender, range.Min.Index, false);
            SelectionChanged?.Invoke(sender, EventArgs.Empty);
        }

        // PageRange に含まれる Page を収集
        private List<Page>? CollectPage(Book? book, PageRange range)
        {
            if (book is null) return null;
            var indexs = Enumerable.Range(range.Min.Index, range.Max.Index - range.Min.Index + 1);
            return indexs.Where(e => book.Pages.IsValidIndex(e)).Select(e => book.Pages[e]).ToList();
        }

#if false
        private void RaiseViewContentsChanged(object? sender, ViewContentSourceCollection? viewPageCollection, bool isBookOpen)
        {
            if (viewPageCollection is null) return;

            var contents = viewPageCollection.Collection;
            if (contents == null) return;

            ViewContentsChanged?.Invoke(sender, new ViewContentsChangedEventArgs(viewPageCollection, isBookOpen));

            var mainContent = contents.Count > 0 ? (contents.First().PagePart.Position < contents.Last().PagePart.Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SetSelectedIndex(sender, mainContent.Page.Index, false);
                SelectionChanged?.Invoke(sender, EventArgs.Empty);
            }
        }
#endif
    }


#if false
    // 表示コンテンツ変更イベント
    public class ViewContentsChangedEventArgs : EventArgs
    {
        public ViewContentsChangedEventArgs(ViewContentSourceCollection viewPageCollection, bool isBookOpen)
        {
            ViewPageCollection = viewPageCollection;
            IsBookOpen = isBookOpen;
        }

        /// <summary>
        /// 表示コンテンツ
        /// </summary>
        public ViewContentSourceCollection ViewPageCollection { get; private set; }

        /// <summary>
        /// 本を新しく開いたとき
        /// </summary>
        public bool IsBookOpen { get; private set; }
    }
#endif
}

