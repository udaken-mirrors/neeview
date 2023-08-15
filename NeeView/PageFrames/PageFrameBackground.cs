using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeView.Windows;


namespace NeeView.PageFrames
{
    public class PageFrameBackground : Grid, IDisposable
    {
        private DpiScaleProvider _dpiScaleProvider;
        private ContentCanvasBrushSource _brushSource;
        private ContentCanvasBrush _canvasBrush;
        private DisposableCollection _disposables = new();
        private bool _disposedValue;
        private Grid _bg1;
        private Grid _bg2;

        public PageFrameBackground(DpiScaleProvider dpiScaleProvider) : this(dpiScaleProvider, null)
        {
        }

        public PageFrameBackground(DpiScaleProvider dpiScaleProvider, Page? page)
        {
            _dpiScaleProvider = dpiScaleProvider;
            _brushSource = new ContentCanvasBrushSource(_dpiScaleProvider, page);
            _canvasBrush = new ContentCanvasBrush(_brushSource);

            _bg1 = this;
            _bg2 = new Grid();
            _bg1.Children.Add(_bg2);

            _disposables.Add(_canvasBrush.SubscribePropertyChanged(nameof(_canvasBrush.BackgroundBrush), (s, e) => UpdateBackground1()));
            _disposables.Add(_canvasBrush.SubscribePropertyChanged(nameof(_canvasBrush.BackgroundFrontBrush), (s, e) => UpdateBackground2()));

            UpdateBackground1();
            UpdateBackground2();

            //_bg1.SetBinding(Grid.BackgroundProperty, new Binding(nameof(_canvasBrush.BackgroundBrush)) { Source = _canvasBrush });
            //_bg2.SetBinding(Grid.BackgroundProperty, new Binding(nameof(_canvasBrush.BackgroundFrontBrush)) { Source = _canvasBrush });
        }

        private void UpdateBackground1()
        {
            _bg1.Background = _canvasBrush.BackgroundBrush;
        }

        private void UpdateBackground2()
        {
            _bg2.Background = _canvasBrush.BackgroundFrontBrush;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _canvasBrush.Dispose();
                    //BindingOperations.ClearBinding(_bg1, Grid.BackgroundProperty);
                    //BindingOperations.ClearBinding(_bg2, Grid.BackgroundProperty);
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetPage(Page? page)
        {
            _brushSource.SetPage(page);
        }

    }
}