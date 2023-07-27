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

        public void FirstPage(object? sender)
        {
            _moveControl.FirstPage(sender);
        }

        public void JumpPage(object? sender, int index)
        {
            _moveControl.JumpPage(sender, index);
        }

        public void JumpRandomPage(object? sender)
        {
            _moveControl.JumpRandomPage(sender);
        }

        public void LastPage(object? sender)
        {
            _moveControl.LastPage(sender);
        }

        public void NextFolderPage(object? sender, bool isShowMessage)
        {
            _moveControl.NextFolderPage(sender, isShowMessage);
        }

        public void NextOnePage(object? sender)
        {
            _moveControl.NextOnePage(sender);
        }

        public void NextPage(object? sender)
        {
            _moveControl.NextPage(sender);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _moveControl.NextScrollPage(sender, parameter);
        }

        public void NextSizePage(object? sender, int size)
        {
            _moveControl.NextSizePage(sender, size);
        }

        public void PrevFolderPage(object? sender, bool isShowMessage)
        {
            _moveControl.PrevFolderPage(sender, isShowMessage);
        }

        public void PrevOnePage(object? sender)
        {
            _moveControl.PrevOnePage(sender);
        }

        public void PrevPage(object? sender)
        {
            _moveControl.PrevPage(sender);
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            _moveControl.PrevScrollPage(sender, parameter);
        }

        public void PrevSizePage(object? sender, int size)
        {
            _moveControl.PrevSizePage(sender, size);
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