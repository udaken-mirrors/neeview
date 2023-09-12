using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Threading.Jobs;
using NeeView.Collections.Generic;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    // TODO: ソート中等コマンド処理中表示
    public partial class BookCommandControl : IDisposable
    {
        private bool _disposedValue = false;
        private readonly DisposableCollection _disposables = new();

        // コマンドエンジン
        private readonly BookCommandEngine _commandEngine;
        //private readonly BookSource _book;
        //private readonly BookPageSetting _setting;
        //private readonly BookPageMarker _marker;
        //private IBookPageContext _pageContext;
        //private IBookPageMoveControl _moveControl;
        private readonly PageFrameBox _pageFrameBox;
        private readonly BookContext _bookContext;
        private readonly Book _book;
        private CancellationTokenSource? _sortCancellationTokenSource;


        public BookCommandControl(BookContext bookContext, PageFrameBox pageFrameBox)
        {
            //_book = book;
            //_setting = setting;
            //_marker = marker;
            //_pageContext = pageContext;
            //_moveControl = moveControl;
            _pageFrameBox = pageFrameBox;
            _bookContext = bookContext;
            _book = _bookContext.Book;

            _commandEngine = new();
            _disposables.Add(_commandEngine);

            _disposables.Add(_bookContext.SubscribePropertyChanged(nameof(BookContext.SortMode),
                (s, e) => RequestSort(this)));


            Start();
        }





        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    _sortCancellationTokenSource?.Cancel();
                    _sortCancellationTokenSource?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void Start()
        {
            if (_disposedValue) return;
            _commandEngine.StartEngine();
        }


        private void RequestSort(object sender)
        {
            _sortCancellationTokenSource?.Cancel();
            _sortCancellationTokenSource?.Dispose();
            _sortCancellationTokenSource = new CancellationTokenSource();

            var command = new BookCommandCancellableAction(sender, ExecuteAsync, 2, _sortCancellationTokenSource.Token);
            _commandEngine.Enqueue(command);

            async Task ExecuteAsync(object? s, CancellationToken token)
            {
                Debug.WriteLine($"Sort: {_book.Pages.SortMode}");
                //var page = _viewer.GetViewPage();

                var oldIndex = Math.Max(Math.Min(_bookContext.SelectedRange.Min.Index, _bookContext.Pages.Count - 1), 0);
                var page = _bookContext.Pages[oldIndex];

                _book.Pages.Sort(token);

                var index = (page is null || (_book.Pages.SortMode == PageSortMode.Random && Config.Current.Book.ResetPageWhenRandomSort)) ? 0 : _book.Pages.GetIndex(page);
                var pagePosition = new PagePosition(index, 0);
                //RequestSetPosition(this, pagePosition, 1);
                await AppDispatcher.BeginInvoke(() =>
                {
                    _pageFrameBox.MoveTo(pagePosition, LinkedListDirection.Next);
                    _pageFrameBox.FlushLayout();
                });

                await Task.CompletedTask;
            }
        }

        public void Invoke(Action action)
        {
            var command = new BookCommandAction(this, ExecuteAsync, 2);
            _commandEngine.Enqueue(command);

            async Task ExecuteAsync(object? sender, CancellationToken token)
            {
                await AppDispatcher.BeginInvoke(action);
            }
        }
    }
}
