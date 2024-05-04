using NeeView.PageFrames;
using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class DragTransformControlProxy : IDisposable, IDragTransformControl
    {
        private readonly PageFrameBoxPresenter _presenter;
        private readonly IDragTransformContextFactory _transformContextFactory;
        private DragTransformControl? _dragTransformControl;
        private bool _disposedValue;

        public DragTransformControlProxy(PageFrameBoxPresenter presenter, IDragTransformContextFactory transformContextFactory)
        {
            _presenter = presenter;
            _transformContextFactory = transformContextFactory;
            _dragTransformControl = CreateDragTransformControl(_transformContextFactory);

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
            _dragTransformControl = CreateDragTransformControl(e.Box ?? _transformContextFactory);
        }

        private DragTransformControl CreateDragTransformControl(IDragTransformContextFactory factory)
        {
            return new DragTransformControl(factory, DragActionTable.Current, Config.Current.View);
        }

        public void ResetState()
        {
            _dragTransformControl?.ResetState();
        }

        public void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp, ISpeedometer? speedometer, DragActionUpdateOptions options)
        {
            _dragTransformControl?.UpdateState(buttons, keys, point, timestamp, speedometer, options);
        }

        public void MouseWheel(MouseButtonBits buttons, ModifierKeys keys, MouseWheelEventArgs e)
        {
            _dragTransformControl?.MouseWheel(buttons, keys, e);
        }
    }




}
