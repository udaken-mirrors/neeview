using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページ読み込み。
    /// 表示ページと先読みページの読み込み、読み込み完了時のイベント発行
    /// </summary>
    public class BookPageLoader : BindableBase, IDisposable
    {
        private readonly BookSource _book;
        private readonly BookPageSetting _setting;

        // ブックのビュー更新カウンター
        private readonly BookPageCounter _viewCounter = new();

        // リソースを保持しておくページ
        private List<Page> _keepPages = new();

        // JOBリクエスト
        private readonly PageContentJobClient _jobClient = new("View", JobCategories.PageViewContentJobCategory);

        // メモリ管理
        private readonly BookMemoryService _bookMemoryService;

        // 先読み
        private readonly BookAhead _ahead;
        private readonly BookAheadCalculator _aheadCalculator;

        // コンテンツ生成
        private BookPageViewGenerater? _pageViewGenerater;

        // 処理中フラグ
        private bool _isBusy;

        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue = false;


        public BookPageLoader(BookSource book, BookPageSetting setting, BookMemoryService memoryService)
        {
            _book = book;
            _setting = setting;
            _bookMemoryService = memoryService;

            // NOTE: Page.Loadedの開放はPageのDisposeに任せる
            // TODO: ↑よろしくない
            foreach (var page in _book.Pages)
            {
                page.Loaded += Page_Loaded;
            }

            _ahead = new BookAhead(_bookMemoryService);
            _disposables.Add(_ahead.SubscribePropertyChanged(nameof(BookAhead.IsBusy),
                        (s, e) => UpdateIsBusy()));

            _aheadCalculator = new BookAheadCalculator(_book);
        }


        // 表示コンテンツ変更
        // 表示の更新を要求
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;

        public IDisposable SubscribeViewContentsChanged(EventHandler<ViewContentSourceCollectionChangedEventArgs> handler)
        {
            ViewContentsChanged += handler;
            return new AnonymousDisposable(() => ViewContentsChanged -= handler);
        }

        // 先読みコンテンツ変更
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? NextContentsChanged;

        public IDisposable SubscribeNextContentsChanged(EventHandler<ViewContentSourceCollectionChangedEventArgs> handler)
        {
            NextContentsChanged += handler;
            return new AnonymousDisposable(() => NextContentsChanged -= handler);
        }


        // 処理中フラグ
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }


        // 処理中フラグ更新
        private void UpdateIsBusy()
        {
            IsBusy = _ahead.IsBusy || (_pageViewGenerater != null && _pageViewGenerater.IsBusy);
        }

        private PageRange CreatePageRange(IEnumerable<Page> pages, int direction)
        {
            var pos0 = new PagePosition(pages.Select(e => e.Index).Min(), 0);
            var pos1 = new PagePosition(pages.Select(e => e.Index).Max(), 1);
            return new PageRange(pos0, pos1, direction);
        }


        public void LoadEmpty(object? sender)
        {
            ViewContentsChanged?.Invoke(sender, new ViewContentSourceCollectionChangedEventArgs(_book.Path, new ViewContentSourceCollection()));
        }

        public async Task LoadAsync(object? sender, List<Page> viewPages, int direction, CancellationToken token)
        {
            var viewPageRange = CreatePageRange(viewPages, direction);

            // pre load
            _ahead.Clear();
            var aheadPageRange = _aheadCalculator.CreateAheadPageRange(viewPageRange);
            var aheadPages = CreatePagesFromRange(aheadPageRange, viewPages);

            var loadPages = viewPages.Concat(aheadPages).Distinct().ToList();

            // update content lock
            var unloadPages = _keepPages.Except(viewPages).ToList();
            foreach (var page in unloadPages)
            {
                page.State = PageContentState.None;
            }
            foreach (var (page, index) in viewPages.ToTuples())
            {
                page.State = PageContentState.View;
            }
            _keepPages = loadPages;

            // update contents
            _pageViewGenerater?.Dispose();
            _pageViewGenerater = new BookPageViewGenerater(sender, _book, _setting.Source, viewPageRange, aheadPageRange, _viewCounter);
            _pageViewGenerater.ViewContentsChanged += (s, e) => ViewContentsChanged?.Invoke(s, e);
            _pageViewGenerater.NextContentsChanged += (s, e) => NextContentsChanged?.Invoke(s, e);
            _pageViewGenerater.AddPropertyChanged(nameof(_pageViewGenerater.IsBusy), (s, e) => UpdateIsBusy());

            _bookMemoryService.SetReference(viewPages.First().Index);
            _jobClient.Order(viewPages.Cast<IPageContentLoader>().ToList());
            _ahead.Order(aheadPages);

            UpdateIsBusy();
            _pageViewGenerater.UpdateNextContents();

            try
            {
                // wait load time (max 5sec.)
                var timeout = (BookProfile.Current.CanPrioritizePageMove() && _viewCounter.Counter != 0) ? 100 : 5000;
                await _pageViewGenerater.WaitVisibleAsync(timeout, token);
            }
            catch (TimeoutException)
            {
                _pageViewGenerater.UpdateViewContents();
                _pageViewGenerater.UpdateNextContents();
            }
        }

        /// <summary>
        /// ページロード完了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            if (sender is not Page page) return;

            _bookMemoryService.AddPageContent(page);

            _ahead.OnPageLoaded(this, new PageChangedEventArgs(page));

            ////if (!BookProfile.Current.CanPrioritizePageMove()) return;

            _pageViewGenerater?.UpdateNextContents();
        }


        /// <summary>
        /// ページ範囲からページ列を生成
        /// </summary>
        /// <param name="ranges"></param>
        /// <param name="excepts">除外するページ</param>
        /// <returns></returns>
        private List<Page> CreatePagesFromRange(List<PageRange> ranges, List<Page> excepts)
        {
            return ranges.Select(e => CreatePagesFromRange(e, excepts))
                .SelectMany(e => e)
                .ToList();
        }

        private List<Page> CreatePagesFromRange(PageRange range, List<Page> excepts)
        {
            if (range.IsEmpty())
            {
                return new List<Page>();
            }

            return Enumerable.Range(0, range.PageSize)
                .Select(e => range.Position.Index + e * range.Direction)
                .Where(e => 0 <= e && e < _book.Pages.Count)
                .Select(e => _book.Pages[e])
                .Except(excepts)
                .ToList();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();

                    this.ResetPropertyChanged();
                    //this.PageTerminated = null;
                    this.ViewContentsChanged = null;
                    this.NextContentsChanged = null;

                    _ahead.Dispose();
                    _pageViewGenerater?.Dispose();

                    _jobClient.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
