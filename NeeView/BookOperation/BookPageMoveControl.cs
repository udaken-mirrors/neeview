using Jint.Native;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView
{

    public partial class BookPageMoveControl : IBookPageMoveControl
    {
        private PageFrameBox _box;

        //private DisposableCollection _disposables = new();
        //private bool _disposedValue = false;


        public BookPageMoveControl(PageFrameBox box)
        {
            _box = box;

            //_disposables.Add(_presenter.SubscribePropertyChanged(nameof(_presenter.Pages), (_, _) => RaisePropertyChanged(nameof(Pages))));
           // _disposables.Add(_presenter.SubscribePropertyChanged(nameof(_presenter.SelectedRange), (_, _) => RaisePropertyChanged(nameof(SelectedRange))));
        }

#if false
        #region IDisposable Support

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
        #endregion IDisposable Support

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? PagesChanged;
#endif

        public IReadOnlyList<Page> Pages => _box.Pages;

        public PageRange SelectedRange => _box.SelectedRange;


        public void MovePrev(object? sender)
        {
            _box.MoveToNextFrame(LinkedListDirection.Previous);
        }

        public void MoveNext(object? sender)
        {
            _box.MoveToNextFrame(LinkedListDirection.Next);
        }

        public void MovePrevOne(object? sender)
        {
            _box.MoveToNextPage(LinkedListDirection.Previous);
        }

        public void MoveNextOne(object? sender)
        {
            _box.MoveToNextPage(LinkedListDirection.Next);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _box.ScrollToNextFrame(LinkedListDirection.Previous, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _box.ScrollToNextFrame(LinkedListDirection.Next, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }

        public void MoveTo(object? sender, int index)
        {
            _box.MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
        }

        public void MoveToRandom(object? sender)
        {
            var random = new Random();
            var index = random.Next(Pages.Count);
            _box.MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _box.MoveTo(new PagePosition(SelectedRange.Min.Index - size, 0), LinkedListDirection.Previous);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _box.MoveTo(new PagePosition(SelectedRange.Min.Index + size, 0), LinkedListDirection.Next);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _box.MoveToNextFolder(LinkedListDirection.Previous, isShowMessage);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _box.MoveToNextFolder(LinkedListDirection.Next, isShowMessage);
        }

        public void MoveToFirst(object? sender)
        {
            _box.MoveTo(new PagePosition(0, 0), LinkedListDirection.Next);
        }

        public void MoveToLast(object? sender)
        {
            _box.MoveTo(new PagePosition(Pages.Count - 1, 0), LinkedListDirection.Next);
        }
    }

}