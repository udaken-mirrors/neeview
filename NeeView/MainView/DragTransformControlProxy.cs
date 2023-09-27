using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class DragTransformControlProxy : IDisposable, IDragTransformControl
    {
        private PageFrameBoxPresenter _presenter;
        private DragTransformControl? _dragTransformControl;
        private bool _disposedValue;

        public DragTransformControlProxy(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
            _presenter.PageFrameBoxChanging += Presenter_PageFrameBoxChanging;
            _presenter.PageFrameBoxChanged += Presenter_PageFrameBoxChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _presenter.PageFrameBoxChanging -= Presenter_PageFrameBoxChanging;
                    _presenter.PageFrameBoxChanged -= Presenter_PageFrameBoxChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Presenter_PageFrameBoxChanging(object? sender, PageFrameBoxChangingEventArgs e)
        {
            _dragTransformControl = null;
        }

        private void Presenter_PageFrameBoxChanged(object? sender, PageFrameBoxChangedEventArgs e)
        {
            if (e.Box is not null)
            {
                _dragTransformControl = new DragTransformControl(e.Box, DragActionTable.Current, Config.Current.View);
            }
            else
            {
                _dragTransformControl = null;
            }
        }

        public void ResetState()
        {
            _dragTransformControl?.ResetState();
        }

        public void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp, DragActionUpdateOptions options)
        {
            _dragTransformControl?.UpdateState(buttons, keys, point, timestamp, options);
        }

        public void MouseWheel(MouseButtonBits buttons, ModifierKeys keys, MouseWheelEventArgs e)
        {
            _dragTransformControl?.MouseWheel(buttons, keys, e);
        }
    }




}