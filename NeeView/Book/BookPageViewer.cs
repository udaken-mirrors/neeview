using Esprima.Ast;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 表示ページとその移動
    /// </summary>
    public class BookPageViewer : BindableBase, IDisposable
    {
        private readonly BookSource _book;
        private readonly BookPageSetting _setting;
        private readonly BookPageLoader _loader;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue = false;

        // 表示ページコンテキスト
        private ViewContentSourceCollection _viewPageCollection = new();



        public BookPageViewer(BookSource book, BookMemoryService memoryService, BookPageSetting setting)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _setting = setting;

            _loader = new BookPageLoader(book, _setting, memoryService);
            _disposables.Add(_loader);
            _disposables.Add(_loader.SubscribeViewContentsChanged(Loader_ViewContentsChanged));
        }



        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        public event EventHandler<PageTerminatedEventArgs>? PageTerminated;

        public IDisposable SubscribePageTerminated(EventHandler<PageTerminatedEventArgs> handler)
        {
            PageTerminated += handler;
            return new AnonymousDisposable(() => PageTerminated -= handler);
        }


        #region SelectedRange
        // ページリストの更新等に ViewContentChanged が使用されているが、その代替になるように

        public event EventHandler<SelectedRangeChangedEventArgs>? SelectedRangeChanged;

        public IDisposable SubscribeSelectedRangeChanged(EventHandler<SelectedRangeChangedEventArgs> handler)
        {
            SelectedRangeChanged += handler;
            return new AnonymousDisposable(() => SelectedRangeChanged -= handler);
        }

        public PageRange _selectedRange;

        public PageRange SelectedRange
        {
            get { return _selectedRange; }
            set { SetSelectedItem(this, value, true); }
        }

        public void SetSelectedItem(object? sender, PageRange value, bool fromOutsize)
        {
            if (_selectedRange != value)
            {
                _selectedRange = value;
                RaisePropertyChanged(nameof(SelectedRange));
                SelectedRangeChanged?.Invoke(sender, new SelectedRangeChangedEventArgs(fromOutsize));
            }
        }

        #endregion SelectedRange


        public BookPageLoader Loader => _loader;

        // 表示ページ変更回数
        public int PageChangeCount { get; private set; }

        // 終端ページ表示
        public bool IsPageTerminated { get; private set; }

        // TODO: このパラメータだけ公開するのは微妙。
        public ViewContentSourceCollection ViewPageCollection => _viewPageCollection;



        private void Loader_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            Interlocked.Exchange(ref _viewPageCollection, e.ViewPageCollection);
            SetSelectedItem(sender, e.ViewPageCollection.Range, false);
        }


        // 動画用：外部から終端イベントを発行
        public void RaisePageTerminatedEvent(object? sender, int direction)
        {
            if (_disposedValue) return;

            PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(direction));
        }

        // 表示ページ番号
        public int GetViewPageIndex() => _viewPageCollection.Range.Min.Index;

        // 表示ページ
        public Page? GetViewPage() => _book.Pages.GetPage(_viewPageCollection.Range.Min.Index);

        // 表示ページ群
        public List<Page> GetViewPages() => _viewPageCollection.Collection.Select(e => e.Page).ToList();


        // 表示ページ再読込
        public async Task RefreshViewPageAsync(object? sender, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return;

            var range = new PageRange(_viewPageCollection.Range.Min, 1, _setting.PageMode.Size());
            await UpdateViewPageAsync(sender, range, token);
        }

        // 表示ページ移動
        public async Task MoveViewPageAsync(object? sender, int step, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return;

            var viewRange = _viewPageCollection.Range;

            var direction = step < 0 ? -1 : 1;

            var pos = Math.Abs(step) == _setting.PageMode.Size() ? viewRange.Next(direction) : viewRange.Move(step);
            if (pos < _book.Pages.FirstPosition() && !viewRange.IsContains(_book.Pages.FirstPosition()))
            {
                pos = new PagePosition(0, direction < 0 ? 1 : 0);
            }
            else if (pos > _book.Pages.LastPosition() && !viewRange.IsContains(_book.Pages.LastPosition()))
            {
                pos = new PagePosition(_book.Pages.Count - 1, direction < 0 ? 1 : 0);
            }

            var range = new PageRange(pos, direction, _setting.PageMode.Size());

            await UpdateViewPageAsync(sender, range, token);
        }

        // 表示ページ指定移動
        public async Task JumpViewPageAsync(object? sender, PagePosition position, int direction, CancellationToken token)
        {

            var range = new PageRange(position, direction, _setting.PageMode.Size());
            await UpdateViewPageAsync(sender, range, token);
        }

        // 表示ページ更新
        private async Task UpdateViewPageAsync(object? sender, PageRange viewPageRange, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return;

            // ページ終端を越えたか判定
            if (viewPageRange.Position < _book.Pages.FirstPosition())
            {
                PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(-1));
                return;
            }
            else if (viewPageRange.Position > _book.Pages.LastPosition())
            {
                PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(+1));
                return;
            }

            // ページ数０の場合は表示コンテンツなし
            if (_book.Pages.Count == 0)
            {
                _loader.LoadEmpty(sender);
                return;
            }

            // view pages
            var viewPages = new List<Page>();
            for (int i = 0; i < _setting.PageMode.Size(); ++i)
            {
                var page = _book.Pages[_book.Pages.ClampPageNumber(viewPageRange.Position.Index + viewPageRange.Direction * i)];
                if (!viewPages.Contains(page))
                {
                    viewPages.Add(page);
                }
            }

            this.PageChangeCount++;
            this.IsPageTerminated = viewPageRange.Max >= _book.Pages.LastPosition();

            // NOTE: PageRangeが終端を超えているとpagesが制限されるので違いが出るが問題なし
            //Debug.Assert(CreatePageRange(viewPages, viewPageRange.Direction) == viewPageRange);

            await _loader.LoadAsync(sender, viewPages, viewPageRange.Direction, token);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    this.ResetPropertyChanged();
                    this.PageTerminated = null;
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
