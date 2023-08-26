using System;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    public partial class BaseScaleTransform : IScaleControl, IDisposable
    {
        private double _scale = 1.0;
        private ViewConfig _viewConfig;
        private ScaleTransform _scaleTransform;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();

        public BaseScaleTransform(ViewConfig viewConfig)
        {
            _viewConfig = viewConfig;
            _scaleTransform = new ScaleTransform();

            _disposables.Add(_viewConfig.SubscribePropertyChanged(nameof(ViewConfig.IsBaseScaleEnabled),
                (s, e) => UpdateTransform()));
            _disposables.Add(_viewConfig.SubscribePropertyChanged(nameof(ViewConfig.BaseScale),
                (s, e) => UpdateTransform()));

            UpdateTransform();
        }


        [Subscribable]
        public event EventHandler? ScaleChanged;


        public double Scale => _scale;

        public ScaleTransform ScaleTransform => _scaleTransform;


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

        public void SetScale(double value, TimeSpan span)
        {
            _viewConfig.BaseScale = value;
        }

        private void UpdateTransform()
        {
            var scale = _viewConfig.IsBaseScaleEnabled ? _viewConfig.BaseScale : 1.0;
            _scaleTransform.ScaleX = scale;
            _scaleTransform.ScaleY = scale;

            if (_scale != scale)
            {
                _scale = scale;
                ScaleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}


