using Jint.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// BookOperation と Book.Control をつなぐアダプタ
    /// </summary>
    public class BookPageMoveControl : IBookPageMoveControl
    {
        private Book _book;

        public BookPageMoveControl(Book book)
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


}