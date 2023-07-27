using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// BookOperation ページ操作 (Media用)
    /// </summary>
    public class MediaPageMoveControl : IBookPageMoveControl
    {
        private Book _book;

        public MediaPageMoveControl(Book book)
        {
            Debug.Assert(book.IsMedia);
            _book = book;
        }

        public void FirstPage(object? sender)
        {
            MediaPlayerOperator.Current?.SetPositionFirst();
        }

        public void LastPage(object? sender)
        {
            MediaPlayerOperator.Current?.SetPositionLast();
        }

        public void PrevPage(object? sender)
        {
            MoveMediaPage(sender, -1);
        }

        public void NextPage(object? sender)
        {
            MoveMediaPage(sender, +1);
        }

        public void PrevOnePage(object? sender)
        {
            MoveMediaPage(sender, -1);
        }

        public void NextOnePage(object? sender)
        {
            MoveMediaPage(sender, +1);
        }

        public void PrevSizePage(object? sender, int size)
        {
            MoveMediaPage(sender, -size);
        }

        public void NextSizePage(object? sender, int size)
        {
            MoveMediaPage(sender, +size);
        }

        public void PrevFolderPage(object? sender, bool isShowMessage)
        {
            // TODO: チャプター移動？
        }

        public void NextFolderPage(object? sender, bool isShowMessage)
        {
            // TODO: チャプター移動？
        }

        public void JumpPage(object? sender, int index)
        {
            // TODO: 時間指定移動？
        }

        public void JumpRandomPage(object? sender)
        {
        }

        public void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            MainViewComponent.Current.ViewController.PrevScrollPage(sender, parameter);
        }

        public void NextScrollPage(object? sender, ScrollPageCommandParameter parameter)
        {
            MainViewComponent.Current.ViewController.NextScrollPage(sender, parameter);
        }


        // ページ移動量をメディアの時間移動量に変換して移動
        private void MoveMediaPage(object? sender, int delta)
        {
            if (MediaPlayerOperator.Current == null) return;

            var isTerminated = MediaPlayerOperator.Current.AddPosition(TimeSpan.FromSeconds(delta * Config.Current.Archive.Media.PageSeconds));

            if (isTerminated)
            {
                _book?.Viewer.RaisePageTerminatedEvent(sender, delta < 0 ? -1 : 1);
            }
        }
    }
}