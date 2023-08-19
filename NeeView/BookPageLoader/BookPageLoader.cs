using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.PageFrames;

namespace NeeView
{
    // TODO: BookMemoryService: RawSourceとViewSource を同列に管理。ページ単位で増減させる方向で。
    public class BookPageLoader : IDisposable
    {
        private IBook _book;
        private PageFrameFactory _frameFactory;
        private ViewSourceMap _viewSourceMap;
        private PageFrameElementScaleFactory _elementScaleFactory;
        private BookMemoryService _bookMemoryService;
        private readonly PerformanceConfig _performanceConfig;
        private CancellationTokenSource? _cancellationTokenSource;

        private DisposableCollection _disposables = new();
        private bool _disposedValue;

        private readonly PageContentJobClient _jobClient;
        private readonly PageContentJobClient _jobAheadClient;

        private List<Page> _viewPages = new();
        private List<Page> _aheadPages = new();

        private object _lock = new();

        private bool _isEnabled = true;
        private BookLoadContext _loadContext = new BookLoadContext();


        public BookPageLoader(IBook book, PageFrameFactory frameFactory, ViewSourceMap viewSourceMap, PageFrameElementScaleFactory elementScaleFactory, BookMemoryService bookMemoryService, PerformanceConfig performanceConfig)
        {
            _book = book;
            _frameFactory = frameFactory;
            _viewSourceMap = viewSourceMap;
            _elementScaleFactory = elementScaleFactory;
            _bookMemoryService = bookMemoryService;
            _performanceConfig = performanceConfig;

            _jobClient = new("View", JobCategories.PageViewContentJobCategory);
            _disposables.Add(_jobClient);

            _jobAheadClient = new("Ahead", JobCategories.PageAheadContentJobCategory);
            _disposables.Add(_jobAheadClient);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Cancel();
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void Pause()
        {
            lock (_lock)
            {
                _isEnabled = false;
                _loadContext = new BookLoadContext();
            }
        }

        public void Resume()
        {
            lock (_lock)
            {
                _isEnabled = true;
                if (_loadContext.IsValid)
                {
                    RequestLoad(_loadContext.PageRange, _loadContext.Direction);
                }
            }
        }


        public void RequestLoad(PageRange range, int direction)
        {
            lock (_lock)
            {
                if (!_isEnabled)
                {
                    _loadContext = new BookLoadContext(range, direction);
                    return;
                }
            }

            Task.Run(() => LoadAsync(range, direction, CancellationToken.None));
        }

        private async Task LoadAsync(PageRange range, int direction, CancellationToken token)
        {
            Debug.Assert(direction is 1 or -1);

            lock (_lock)
            {
                Debug.WriteLine($"** BookPageLoader.LoadAsync: {range}, {direction}");
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);
            linkedTokenSource.Token.ThrowIfCancellationRequested();

            // 先読みサイズ
            int limit = _performanceConfig.PreLoadSize;

            // ページ状態クリア
            ClearPageState();

            try
            {
                // 指定分読み込み
                await LoadMainAsync(range, direction, linkedTokenSource.Token);

                // 先読み 次方向
                var count = await LoadAheadAsync(range.Next(direction), direction, limit, linkedTokenSource.Token);

                // 先読み 前方向
                var rest = limit - count;
                if (0 < rest)
                {
                    var pages = await LoadAheadAsync(range.Next(-direction), -direction, rest, linkedTokenSource.Token);
                }
            }
            catch (OperationCanceledException ex)
            {
                //Debug.WriteLine($"BookPageLoader.Canceled: {ex.StackTrace}");
            }

            //Debug.WriteLine($"BookPageLoader: Views.Count = {_book.Pages.Count(e => e.State == PageContentState.View)}");
            //Debug.WriteLine($"BookPageLoader: Aheads.Count = {_book.Pages.Count(e => e.State == PageContentState.Ahead)}");
        }

        private void ClearPageState()
        {
            lock (_lock)
            {
                foreach (var page in _viewPages)
                {
                    page.State = PageContentState.None;
                }
                foreach (var page in _aheadPages)
                {
                    page.State = PageContentState.None;
                }

                _viewPages.Clear();
                _aheadPages.Clear();

                Debug.Assert(_book.Pages.All(e => e.State == PageContentState.None));
            }
        }


        public void Cancel()
        {
            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _loadContext = new BookLoadContext();
            }
        }


        /// <summary>
        /// 表示ページ読み込み
        /// </summary>
        /// <param name="range">ページ範囲</param>
        /// <param name="direction">ページ方向</param>
        /// <param name="token">キャンセルトークン</param>
        public async Task LoadMainAsync(PageRange range, int direction, CancellationToken token)
        {
            var indexs = Enumerable.Range(range.Min.Index, range.Max.Index - range.Min.Index + 1).ToList();
            var pages = indexs.Direction(direction).Select(e => _book.Pages[e]).ToList();

            lock (_lock)
            {
                token.ThrowIfCancellationRequested();
                foreach (var page in pages)
                {
                    _viewPages.Add(page);
                    page.State = PageContentState.View;
                }
            }

            var contens = pages.Cast<IPageContentLoader>().ToList();
            //foreach (var page in pages)
            //{
            //    Debug.WriteLine($"BookPageLoader.LoadMainAsync: Job.{page}");
            //}
            _jobClient.Order(contens);
            await _jobClient.WaitAsync(contens, -1, token);

            // NOTE: ViewSource は表示部で作成される
        }

        /// <summary>
        /// ページ先読み
        /// </summary>
        /// <param name="position">先読み開始位置</param>
        /// <param name="direction">先読み方向</param>
        /// <param name="limit">先読みページ数上限</param>
        /// <param name="token">キャンセルトークン</param>
        public async Task<int> LoadAheadAsync(PagePosition position, int direction, int limit, CancellationToken token)
        {
            var count = 0;
            var pos = position;
            while (count < limit)
            {
                NVDebug.AssertMTA();
                token.ThrowIfCancellationRequested();

                // メモリ許容チェック
                _bookMemoryService.Cleanup();
                if (_bookMemoryService.IsFull)
                {
                    Debug.WriteLine($"BookPageLoader: Memory full.");
                    break;
                }

                // つぎのフレーム作成
                var frame = _frameFactory.CreatePageFrame(pos, direction);
                if (frame is null) break;

                // ページ読み込み
                var pages = frame.Elements.Select(e => e.Page).ToList();

                lock (_lock)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (var page in pages)
                    {
                        _aheadPages.Add(page);
                        page.State = PageContentState.Ahead;
                    }
                }

                var contents = pages.Cast<IPageContentLoader>().ToList();
                //foreach (var page in pages)
                //{
                //    Debug.WriteLine($"BookPageLoader.LoadAheadAsync[{Thread.CurrentThread.ManagedThreadId}]: Job.{page}");
                //}
                _jobAheadClient.Order(contents);
                await _jobAheadClient.WaitAsync(contents, -1, token);

                // フレーム確定
                frame = _frameFactory.CreatePageFrame(pos, direction);
                if (frame is null) break;

                // ViewSourceを作成
                foreach (var element in frame.Elements)
                {
                    token.ThrowIfCancellationRequested();
                    var viewSource = _viewSourceMap.Get(element.Page, element.PagePart);
                    var viewContentSize = ViewContentSizeFactory.Create(element, _elementScaleFactory.Create(frame));
                    var pictureSize = viewContentSize.GetPictureSize();
                    await viewSource.LoadAsync(pictureSize, token);
                }

                count += frame.Elements.Count;
                pos = frame.FrameRange.Next(direction);
            }

            return count;
        }
    }


    public record class BookLoadContext
    {
        public BookLoadContext()
        {
            PageRange = PageRange.Empty;
        }

        public BookLoadContext(PageRange pageRange, int direction)
        {
            PageRange = pageRange;
            Direction = direction;
        }

        public PageRange PageRange { get; init; }
        public int Direction { get; init; }

        public bool IsValid => !PageRange.IsEmpty();
    }



    public class BookPageLoadPauser : IDisposable
    {
        public BookPageLoader _loader;

        public BookPageLoadPauser(BookPageLoader loader)
        {
            _loader = loader;
            _loader.Pause();
        }

        public void Dispose()
        {
            _loader.Resume();
        }
    }


}
