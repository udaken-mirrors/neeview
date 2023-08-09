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

        public void MoveToFirst(object? sender)
        {
            MediaPlayerOperator.Current?.SetPositionFirst();
        }

        public void MoveToLast(object? sender)
        {
            MediaPlayerOperator.Current?.SetPositionLast();
        }

        public void MovePrev(object? sender)
        {
            MoveMediaPage(sender, -1);
        }

        public void MoveNext(object? sender)
        {
            MoveMediaPage(sender, +1);
        }

        public void MovePrevOne(object? sender)
        {
            MoveMediaPage(sender, -1);
        }

        public void MoveNextOne(object? sender)
        {
            MoveMediaPage(sender, +1);
        }

        public void MovePrevSize(object? sender, int size)
        {
            MoveMediaPage(sender, -size);
        }

        public void MoveNextSize(object? sender, int size)
        {
            MoveMediaPage(sender, +size);
        }

        public void MovePrevFolder(object? sender, bool isShowMessage)
        {
            // TODO: チャプター移動？
        }

        public void MoveNextFolder(object? sender, bool isShowMessage)
        {
            // TODO: チャプター移動？
        }

        public void MoveTo(object? sender, int index)
        {
            // TODO: 時間指定移動？
        }

        public void MoveToRandom(object? sender)
        {
        }

        public void ScrollToPrevFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            MainViewComponent.Current.ViewTransformControl.PrevScrollPage(sender, parameter);
        }

        public void ScrollToNextFrame(object? sender, ScrollPageCommandParameter parameter)
        {
            MainViewComponent.Current.ViewTransformControl.NextScrollPage(sender, parameter);
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