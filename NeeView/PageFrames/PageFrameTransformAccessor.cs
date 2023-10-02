using NeeLaboratory.Generators;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public partial class PageFrameTransformAccessor : IPageFrameTransform, IDisposable, INotifyTransformChanged
    {
        private readonly PageFrameTransformMap _transformMap;
        private readonly PageFrameTransform _source;
        private readonly PageFrameTransform _share;

        private PageFrameTransform _flipSource;
        private PageFrameTransform _scaleSource;
        private PageFrameTransform _angleSource;
        private PageFrameTransform _pointSource;

        private readonly TransformGroup _transformGroupCalc = new TransformGroup();
        private readonly TransformGroup _transformGroupView = new TransformGroup();

        private bool _disposedValue;


        public PageFrameTransformAccessor(PageFrameTransformMap transformMap, PageFrameTransform source)
        {
            _transformMap = transformMap;
            _share = transformMap.Share;
            _source = source;

            UpdateSource();

            _transformMap.PropertyChanged += TransformMap_PropertyChanged;
            _share.TransformChanged += Share_TransformChanged;
            _source.TransformChanged += Source_TransformChanged;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _transformMap.PropertyChanged -= TransformMap_PropertyChanged;
                    _share.TransformChanged -= Share_TransformChanged;
                    _source.TransformChanged -= Source_TransformChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;


        public double Angle => _angleSource.Angle;
        public double Scale => _scaleSource.Scale;
        public Point Point => _pointSource.Point;
        public bool IsFlipHorizontal => _flipSource.IsFlipHorizontal;
        public bool IsFlipVertical => _flipSource.IsFlipVertical;

        public Transform Transform => _transformGroupCalc;
        public Transform TransformView => _transformGroupView;


        private PageFrameTransform GetPageFrameTransform(TransformAction action)
        {
            return action switch
            {
                TransformAction.Scale => _scaleSource,
                TransformAction.Angle => _angleSource,
                TransformAction.Point => _pointSource,
                TransformAction.FlipHorizontal => _flipSource,
                TransformAction.FlipVertical => _flipSource,
                _ => throw new ArgumentOutOfRangeException(nameof(action)),
            };
        }

        private void Source_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            if (GetPageFrameTransform(e.Action) == _source)
            {
                TransformChanged?.Invoke(this, e);
            }
        }

        private void Share_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            if (GetPageFrameTransform(e.Action) == _share)
            {
                TransformChanged?.Invoke(this, e);
            }
        }


        private void TransformMap_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PageFrameTransformMap.IsFlipLocked):
                case nameof(PageFrameTransformMap.IsScaleLocked):
                case nameof(PageFrameTransformMap.IsAngleLocked):
                    Update();
                    break;
            }
        }

        public void SetScale(double value, TimeSpan span)
        {
            if (_disposedValue) return;

            _scaleSource.SetScale(value, span);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            if (_disposedValue) return;

            _angleSource.SetAngle(value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (_disposedValue) return;

            _pointSource.SetPoint(value, span, easeX, easeY);
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            if (_disposedValue) return;

            _flipSource.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            if (_disposedValue) return;

            _flipSource.SetFlipVertical(value, span);
        }

        public Vector GetVelocity()
        {
            return _pointSource.GetVelocity();
        }

        public void ResetVelocity()
        {
            _pointSource.ResetVelocity();
        }


        [MemberNotNull(nameof(_scaleSource), nameof(_angleSource), nameof(_pointSource), nameof(_flipSource))]
        private void UpdateSource()
        {
            _flipSource = _transformMap.IsFlipLocked ? _share : _source;
            _scaleSource = _transformMap.IsScaleLocked ? _share : _source;
            _angleSource = _transformMap.IsAngleLocked ? _share : _source;
            _pointSource = _source;

            _transformGroupCalc.Children.Clear();
            _transformGroupCalc.Children.Add(_flipSource.NormalTransform.FlipTransform);
            _transformGroupCalc.Children.Add(_scaleSource.NormalTransform.ScaleTransform);
            _transformGroupCalc.Children.Add(_angleSource.NormalTransform.RotateTransform);
            _transformGroupCalc.Children.Add(_pointSource.NormalTransform.TranslateTransform);

            _transformGroupView.Children.Clear();
            _transformGroupView.Children.Add(_flipSource.AnimatableTransform.FlipTransform);
            _transformGroupView.Children.Add(_scaleSource.AnimatableTransform.ScaleTransform);
            _transformGroupView.Children.Add(_angleSource.AnimatableTransform.RotateTransform);
            _transformGroupView.Children.Add(_pointSource.AnimatableTransform.TranslateTransform);
        }

        private void Update()
        {
            var scale = Scale;
            var angle = Angle;
            var point = Point;
            var isFlipHorizontal = IsFlipHorizontal;
            var isFlipVertical = IsFlipVertical;

            UpdateSource();

            if (scale != Scale)
            {
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Scale));
            }
            if (angle != Angle)
            {
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Angle));
            }
            if (point != Point)
            {
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Point));
            }
            if (isFlipHorizontal != IsFlipHorizontal)
            {
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.FlipHorizontal));
            }
            if (isFlipVertical != IsFlipVertical)
            {
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.FlipVertical));
            }
        }
    }

}
