using System;
using System.Linq;

namespace NeeView.PageFrames
{
    public class PageFrameContainersSelectedPageWatcher : IDisposable
    {
        private PageFrameBox _box;
        private Book _book;
        private bool _disposedValue;

        public PageFrameContainersSelectedPageWatcher(PageFrameBox box, Book book)
        {
            _box = box;
            _book = book;

            _box.ViewContentChanged += Box_ViewContentChanged;
        }

        private void Box_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            if (e.Action < ViewContentChangedAction.Content) return;
            var viewPages = e.PageFrameContent.PageFrame.Elements.Select(e => e.Page).Distinct().ToList();
            _book.Pages.SetViewPageFlag(viewPages);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _box.ViewContentChanged -= Box_ViewContentChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}