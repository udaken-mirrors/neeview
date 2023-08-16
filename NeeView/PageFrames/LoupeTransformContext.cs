using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class LoupeTransformContext : IPointControl, IScaleControl, INotifyPropertyChanged, INotifyTransformChanged
    {
        private BookContext _context;
        private LoupeTransform _transform = new LoupeTransform();


        public LoupeTransformContext(BookContext context)
        {
            _context = context;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;


        public Transform Transform => _transform.Transform;

        public Point Point => _transform.Point;

        public double Scale => _transform.Scale;


        public Transform GetCanvasTransform()
        {
            return _context.IsStaticFrame ? Transform.Identity : _transform.Transform;
        }

        public Transform GetContentTransform()
        {
            return _context.IsStaticFrame ? _transform.Transform : Transform.Identity;
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            SetPoint(_transform.Point + value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            if (_transform.Point != value)
            {
                _transform.SetPoint(value);
                RaisePropertyChanged(nameof(Point));
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Loupe, TransformAction.Point));
            }
        }

        public void SetScale(double value, TimeSpan span)
        {
            if (_transform.Scale != value)
            {
                _transform.SetScale(value);
                RaisePropertyChanged(nameof(Scale));
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Loupe, TransformAction.Scale));
            }
        }
    }
}