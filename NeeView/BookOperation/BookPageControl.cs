using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookPageControl : IBookPageControl, IDisposable
    {
        private Book _book;
        private IBookPageMoveControl _moveControl;
        private IBookPageActionControl _actionControl;
        private bool _disposedValue;


        public BookPageControl(Book book, IBookControl bookControl)
        {
            _book = book;

            if (book.IsMedia)
            {
                _moveControl = new MediaPageMoveControl(_book);
                _actionControl = new BookPageActionControl(_book, bookControl);
            }
            else
            {
                _moveControl = new BookPageMoveControl(_book);
                _actionControl = new BookPageActionControl(_book, bookControl);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #region IBookPageMoveControl

        public void MoveToFirst(object? sender)
        {
            _moveControl.MoveToFirst(sender);
        }

        public void MoveTo(object? sender, int index)
        {
            _moveControl.MoveTo(sender, index);
        }

        public void MoveToRandom(object? sender)
        {
            _moveControl.MoveToRandom(sender);
        }

        public void MoveToLast(object? sender)
        {
            _moveControl.MoveToLast(sender);
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            _moveControl.MoveNextFolder(sender, isShowMessage);
        }

        public void MoveNextOne(object? sender)
        {
            _moveControl.MoveNextOne(sender);
        }

        public void MoveNext(object? sender)
        {
            _moveControl.MoveNext(sender);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _moveControl.ScrollToNextFrame(sender, parameter);
        }

        public void MoveNextSize(object? sender, int size)
        {
            _moveControl.MoveNextSize(sender, size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            _moveControl.MovePrevFolder(sender, isShowMessage);
        }

        public void MovePrevOne(object? sender)
        {
            _moveControl.MovePrevOne(sender);
        }

        public void MovePrev(object? sender)
        {
            _moveControl.MovePrev(sender);
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            _moveControl.ScrollToPrevFrame(sender, parameter);
        }

        public void MovePrevSize(object? sender, int size)
        {
            _moveControl.MovePrevSize(sender, size);
        }

        #endregion IBookPageMoveControl

        #region IBookPageActionControl

        public bool CanDeleteFile()
        {
            return _actionControl.CanDeleteFile();
        }

        public bool CanExport()
        {
            return _actionControl.CanExport();
        }

        public bool CanOpenFilePlace()
        {
            return _actionControl.CanOpenFilePlace();
        }

        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            _actionControl.CopyToClipboard(parameter);
        }

        public Task DeleteFileAsync()
        {
            return _actionControl.DeleteFileAsync();
        }

        public void Export(ExportImageCommandParameter parameter)
        {
            _actionControl.Export(parameter);
        }

        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            _actionControl.ExportDialog(parameter);
        }

        public void OpenApplication(OpenExternalAppCommandParameter parameter)
        {
            _actionControl.OpenApplication(parameter);
        }

        public void OpenFilePlace()
        {
            _actionControl.OpenFilePlace();
        }

        public bool CanDeleteFile(List<Page> pages)
        {
            return _actionControl.CanDeleteFile(pages);
        }

        public Task DeleteFileAsync(List<Page> pages)
        {
            return _actionControl.DeleteFileAsync(pages);
        }

        #endregion IBookPageActionControl
    }


}