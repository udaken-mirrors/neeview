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
            BookOperation.Current.PageListChanged += BookOperation_PageListChanged;
            BookOperation.Current.ViewContentsChanged += BookOperation_ViewContentsChanged;
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

        public event EventHandler<ViewContentsChangedEventArgs>? ViewContentsChanged;

        public IDisposable SubscribeViewContentsChanged(EventHandler<ViewContentsChangedEventArgs> handler)
        {
            ViewContentsChanged += handler;
            return new AnonymousDisposable(() => ViewContentsChanged -= handler);
        }


        public PageMode PageMode => BookOperation.Current.Book?.Viewer.PageMode ?? PageMode.SinglePage;

        public bool IsSupportedSingleFirstPage => BookOperation.Current.Book?.Viewer.IsSupportedSingleFirstPage ?? false;
        
        public bool IsSupportedSingleLastPage => BookOperation.Current.Book?.Viewer.IsSupportedSingleLastPage ?? false;

        public int ViewPageCount => BookOperation.Current.Book?.Viewer.GetViewPages()?.Count ?? 0;

        public int MaxIndex => BookOperation.Current.GetMaxPageIndex();

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
            SetSelectedIndex(sender, BookOperation.Current.GetPageIndex(), true);
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
            BookOperation.Current.RequestPageIndex(sender, _selectedIndex);
        }

        private void BookOperation_BookChanging(object? sender, BookChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, EventArgs.Empty);
        }

        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            // NOTE: PageListChangedイベントで処理
        }

        private void BookOperation_PageListChanged(object? sender, EventArgs e)
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged(nameof(MaxIndex));
            RaiseViewContentsChanged(sender, BookOperation.Current.Book?.Viewer.ViewPageCollection, true);
        }

        private void BookOperation_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            RaiseViewContentsChanged(sender, e?.ViewPageCollection, false);
        }

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
    }


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
}

