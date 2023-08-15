using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using NeeLaboratory.Generators;
using NeeView.Presenter;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class NavigateTransformControl : ITransformControl, INotifyPropertyChanged
    {
        private PageFrameBoxPresenter _presenter;
        private PageFrameTransformAccessor? _source;

        public NavigateTransformControl(PageFrameBoxPresenter presenter)
        {
            _presenter = presenter;
            _presenter.SelectedRangeChanged += (s, e) => UpdateSource();

            UpdateSource();
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public double Scale => _source?.Scale ?? 1.0;

        public double Angle => _source?.Angle ?? 0.0;

        public Point Point => _source?.Point ?? default;

        public bool IsFlipHorizontal => _source?.IsFlipHorizontal ?? false;

        public bool IsFlipVertical => _source?.IsFlipVertical ?? false;


        private void UpdateSource()
        {
            var source = _presenter.CreateSelectedTransform();
            if (_source == source) return;

            Detach();
            Attach(source);
        }

        private void Attach(PageFrameTransformAccessor? source)
        {
            Debug.Assert(_source is null);
            if (source is null) return;
            _source = source;
            _source.TransformChanged += Source_TransformChanged;
            RaisePropertyChanged(null);
        }

        private void Detach()
        {
            if (_source is null) return;
            _source.TransformChanged -= Source_TransformChanged;
            _source = null;
        }

        private void Source_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            switch (e.Action)
            {
                case TransformAction.Scale:
                    RaisePropertyChanged(nameof(Scale));
                    break;
                case TransformAction.Angle:
                    RaisePropertyChanged(nameof(Angle));
                    break;
                case TransformAction.Point:
                    RaisePropertyChanged(nameof(Point));
                    break;
                case TransformAction.Flip:
                    RaisePropertyChanged(nameof(IsFlipHorizontal));
                    RaisePropertyChanged(nameof(IsFlipVertical));
                    break;
            }
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            _source?.SetPoint(Point + value, span);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _source?.SetAngle(value, span);
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            _source?.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            _source?.SetFlipVertical(value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            _source?.SetPoint(value, span);
        }

        public void SetScale(double value, TimeSpan span)
        {
            _source?.SetScale(value, span);
        }

        public void SnapView()
        {
            _presenter.Stretch(false);
        }
    }





}