using Jint.Native;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.Presenter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView
{

    public partial class BookPageMoveControl : IBookPageMoveControl
    {
        private PageFrameBoxPresenter _presenter;

        //private DisposableCollection _disposables = new();
        //private bool _disposedValue = false;


        public BookPageMoveControl(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;

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

        public IReadOnlyList<Page> Pages => _presenter.Pages;

        public PageRange SelectedRange => _presenter.SelectedRange;


        public void MovePrev(object? sender)
        {
            _presenter.MoveToNextFrame(LinkedListDirection.Previous);
        }

        public void MoveNext(object? sender)
        {
            _presenter.MoveToNextFrame(LinkedListDirection.Next);
        }

        public void MovePrevOne(object? sender)
        {
            _presenter.MoveToNextPage(LinkedListDirection.Previous);
        }

        public void MoveNextOne(object? sender)
        {
            _presenter.MoveToNextPage(LinkedListDirection.Next);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _presenter.ScrollToNextFrame(LinkedListDirection.Previous, parameter, parameter.LineBreakStopMode, parameter.EndMargin);

        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _presenter.ScrollToNextFrame(LinkedListDirection.Next, parameter, parameter.LineBreakStopMode, parameter.EndMargin);
        }

        public void MoveTo(object? sender, int index)
        {
            _presenter.MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
        }

        public void MoveToRandom(object? sender)
        {
            var random = new Random();
            var index = random.Next(Pages.Count);
            _presenter.MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _presenter.MoveTo(new PagePosition(SelectedRange.Min.Index - size, 0), LinkedListDirection.Previous);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _presenter.MoveTo(new PagePosition(SelectedRange.Min.Index + size, 0), LinkedListDirection.Next);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _presenter.MoveToNextFolder(LinkedListDirection.Previous, isShowMessage);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _presenter.MoveToNextFolder(LinkedListDirection.Next, isShowMessage);
        }

        public void MoveToFirst(object? sender)
        {
            _presenter.MoveTo(new PagePosition(0, 0), LinkedListDirection.Next);
        }

        public void MoveToLast(object? sender)
        {
            _presenter.MoveTo(new PagePosition(Pages.Count - 1, 0), LinkedListDirection.Next);
        }
    }


#if false
    /// <summary>
    /// BookOperation と Book.Control をつなぐアダプタ
    /// </summary>
    public class BookPageMoveControlX : IBookPageMoveControl
    {
        private Book _book;

        public BookPageMoveControlX(Book book)
        {
            Debug.Assert(!book.IsMedia);
            _book = book;
        }


        public void MoveToFirst(object? sender)
        {
            _book.Control.FirstPage(sender);
        }

        public void MoveToLast(object? sender)
        {
            _book.Control.LastPage(sender);
        }

        public void MovePrev(object? sender)
        {
            _book.Control.PrevPage(sender, 0);
        }

        public void MoveNext(object? sender)
        {
            _book.Control.NextPage(sender, 0);
        }

        public void MovePrevOne(object? sender)
        {
            _book.Control.PrevPage(sender, 1);
        }

        public void MoveNextOne(object? sender)
        {
            _book.Control.NextPage(sender, 1);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _book.Control.PrevPage(sender, size);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _book.Control.NextPage(sender, size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            var index = _book.Control.PrevFolderPage(sender);
            ShowMoveFolderPageMessage(index, Properties.Resources.Notice_FirstFolderPage, isShowMessage);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            var index = _book.Control.NextFolderPage(sender);
            ShowMoveFolderPageMessage(index, Properties.Resources.Notice_LastFolderPage, isShowMessage);
        }

        public void MoveTo(object? sender, int index)
        {
            if (_book == null || _book.IsMedia) return;

            _book.Control.JumpPage(sender, new PagePosition(index, 0), 1);
        }

        public void MoveToRandom(object? sender)
        {
            if (_book.Pages.Count <= 1) return;

            var currentIndex = _book.Viewer.GetViewPageIndex();

            var random = new Random();
            var index = random.Next(_book.Pages.Count - 1);

            if (index == currentIndex)
            {
                index = _book.Pages.Count - 1;
            }

            _book.Control.JumpPage(sender, new PagePosition(index, 0), 1);
        }


        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            MainViewComponent.Current.ViewTransformControl.PrevScrollPage(sender, parameter);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            MainViewComponent.Current.ViewTransformControl.NextScrollPage(sender, parameter);
        }



        private void ShowMoveFolderPageMessage(int index, string termianteMessage, bool isShowMessage)
        {
            if (index < 0)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, termianteMessage);
            }
            else if (isShowMessage)
            {
                var directory = _book?.Pages[index].GetSmartDirectoryName();
                if (string.IsNullOrEmpty(directory))
                {
                    directory = "(Root)";
                }
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, directory);
            }
        }

    }
#endif

}