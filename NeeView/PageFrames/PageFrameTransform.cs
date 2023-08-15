using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameTransform : IPageFrameTransform, INotifyPropertyChanged, INotifyTransformChanged
    {
        private double _angle;
        private double _scale = 1.0;
        private Point _point;
        public bool _isFlipHorizontal;
        public bool _isFlipVertical;
        private MultiTransform _transform;


        public PageFrameTransform()
        {
            _transform = new MultiTransform();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event TransformChangedEventHandler? TransformChanged;


        public double Scale => _scale;
        public double Angle => _angle;
        public Point Point => _point;
        public bool IsFlipHorizontal => _isFlipHorizontal;
        public bool IsFlipVertical => _isFlipVertical;

        public ITransform NormalTransform => _transform.NormalTransform;
        public ITransform AnimatableTransform => _transform.AnimatableTransform;

        // Content用
        public Transform Transform => _transform.Transform;
        public Transform TransformView => _transform.TransformView;


        public void SetScale(double value, TimeSpan span)
        {
            if (SetProperty(ref _scale, value, nameof(Scale)))
            {
                _transform.SetScale(_scale, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Content, TransformAction.Scale));
            }
        }

        public void SetAngle(double value, TimeSpan span)
        {
            if (SetProperty(ref _angle, value, nameof(Angle)))
            {
                _transform.SetAngle(_angle, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Content, TransformAction.Angle));
            }
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            if (SetProperty(ref _point, value, nameof(Point)))
            {
                _transform.SetPoint(_point, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Content, TransformAction.Point));
            }
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            if (SetProperty(ref _isFlipHorizontal, value, nameof(IsFlipHorizontal)))
            {
                _transform.SetFlipHorizontal(value, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Content, TransformAction.Flip));
            }
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            if (SetProperty(ref _isFlipVertical, value, nameof(IsFlipVertical)))
            {
                _transform.SetFlipVertical(value, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.Content, TransformAction.Flip));
            }
        }

        public void Clear()
        {
            SetAngle(0.0, TimeSpan.Zero);
            SetScale(1.0, TimeSpan.Zero);
            SetPoint(default, TimeSpan.Zero);
            SetFlipHorizontal(false, TimeSpan.Zero);
            SetFlipVertical(false, TimeSpan.Zero);
        }
    }

    public interface ITransform
    {
        public ScaleTransform FlipTransform { get; }
        public ScaleTransform ScaleTransform { get; }
        public RotateTransform RotateTransform { get; }
        public TranslateTransform TranslateTransform { get; }

        public Transform Transform { get; }
    }

    public class NormalTransform : ITransform
    {

        private ScaleTransform _flipTransform;
        private ScaleTransform _scaleTransform;
        private RotateTransform _rotateTransform;
        private TranslateTransform _translateTransform;
        private TransformGroup _transformGroup;


        public NormalTransform()
        {
            _flipTransform = new ScaleTransform();
            _scaleTransform = new ScaleTransform();
            _rotateTransform = new RotateTransform();
            _translateTransform = new TranslateTransform();
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_flipTransform);
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_rotateTransform);
            _transformGroup.Children.Add(_translateTransform);
        }


        public Transform Transform => _transformGroup;
        public ScaleTransform FlipTransform => _flipTransform;
        public ScaleTransform ScaleTransform => _scaleTransform;
        public RotateTransform RotateTransform => _rotateTransform;
        public TranslateTransform TranslateTransform => _translateTransform;


        public void SetScale(double value)
        {
            _scaleTransform.ScaleX = value;
            _scaleTransform.ScaleY = value;
        }

        public void SetAngle(double value)
        {
            _rotateTransform.Angle = value;
        }

        public void SetPoint(Point value)
        {
            _translateTransform.X = value.X;
            _translateTransform.Y = value.Y;
        }

        public void SetFlipHorizontal(bool isFlipHorizontal)
        {
            _flipTransform.ScaleX = isFlipHorizontal ? -1.0 : 1.0;
        }

        public void SetFlipVertical(bool isFlipVertical)
        {
            _flipTransform.ScaleY = isFlipVertical ? -1.0 : 1.0;
        }
    }

    public class AnimatableTransform : ITransform
    {
        private double _scale = 1.0;
        private double _angle;
        private Point _point;
        public bool _isFlipHorizontal;
        public bool _isFlipVertical;

        private ScaleTransform _flipTransform;
        private ScaleTransform _scaleTransform;
        private RotateTransform _rotateTransform;
        private TranslateTransform _translateTransform;
        private TransformGroup _transformGroup;


        public AnimatableTransform()
        {
            _flipTransform = new ScaleTransform();
            _scaleTransform = new ScaleTransform();
            _rotateTransform = new RotateTransform();
            _translateTransform = new TranslateTransform();
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_flipTransform);
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_rotateTransform);
            _transformGroup.Children.Add(_translateTransform);
        }

        public Transform Transform => _transformGroup;
        public ScaleTransform FlipTransform => _flipTransform;
        public ScaleTransform ScaleTransform => _scaleTransform;
        public RotateTransform RotateTransform => _rotateTransform;
        public TranslateTransform TranslateTransform => _translateTransform;


        public void SetScale(double value, TimeSpan span)
        {
            _scale = value;
            SetPropertyValue(_scaleTransform, ScaleTransform.ScaleXProperty, value, span);
            SetPropertyValue(_scaleTransform, ScaleTransform.ScaleYProperty, value, span);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _angle = value;
            SetPropertyValue(_rotateTransform, RotateTransform.AngleProperty, value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            _point = value;
            SetPropertyValue(_translateTransform, TranslateTransform.XProperty, value.X, span);
            SetPropertyValue(_translateTransform, TranslateTransform.YProperty, value.Y, span);
        }

        public void SetFlipHorizontal(bool isFlipHorizontal, TimeSpan span)
        {
            _isFlipHorizontal = isFlipHorizontal;
            SetPropertyValue(_flipTransform, ScaleTransform.ScaleXProperty, isFlipHorizontal ? -1.0 : 1.0, span);
        }

        public void SetFlipVertical(bool isFlipVertical, TimeSpan span)
        {
            _isFlipVertical = isFlipVertical;
            SetPropertyValue(_flipTransform, ScaleTransform.ScaleYProperty, isFlipVertical ? -1.0 : 1.0, span);
        }

        public void Flush()
        {
            SetScale(_scale, TimeSpan.Zero);
            SetAngle(_angle, TimeSpan.Zero);
            SetPoint(_point, TimeSpan.Zero);
            SetFlipHorizontal(_isFlipHorizontal, TimeSpan.Zero);
            SetFlipVertical(_isFlipVertical, TimeSpan.Zero);
        }


        private void SetPropertyValue(Animatable anime, DependencyProperty dp, double value, TimeSpan span)
        {
            if (span <= TimeSpan.Zero)
            {
                anime.BeginAnimation(dp, null);
                anime.SetValue(dp, value);
            }
            else
            {
                var animation = new DoubleAnimation();
                animation.To = value;
                animation.Duration = new Duration(span);
                // ドラッグ操作のような場合
                animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                // キーボード操作のような連続して発行される場合は以下の方が良い
                //animation.AccelerationRatio = IsScrolling() ? 0.4 : 0.0;
                //animation.DecelerationRatio = 0.4;

                anime.BeginAnimation(dp, animation, HandoffBehavior.Compose);
            }
        }

    }


    // TODO: ほとんど PageFrameTransform と同じ
    public class MultiTransform
    {
        private double _scale;
        private double _angle;
        private Point _point;
        public bool _isFlipHorizontal;
        public bool _isFlipVertical;

        private NormalTransform _transformCalc;
        private AnimatableTransform _transformView;


        public MultiTransform()
        {
            _transformCalc = new NormalTransform();
            _transformView = new AnimatableTransform();
        }


        public double Scale => _scale;
        public double Angle => _angle;
        public Point Point => _point;
        public bool IsFlipHorizontal => _isFlipHorizontal;
        public bool IsFlipVertical => _isFlipVertical;

        public ITransform NormalTransform => _transformCalc;
        public ITransform AnimatableTransform => _transformView;
        public Transform Transform => _transformCalc.Transform;
        public Transform TransformView => _transformView.Transform;
        public Transform TransformCalc => _transformCalc.Transform;

        public void SetScale(double value, TimeSpan span)
        {
            if (_scale != value)
            {
                _scale = value;
                _transformCalc.SetScale(_scale);
                _transformView.SetScale(_scale, span);
            }
        }

        public void SetAngle(double value, TimeSpan span)
        {
            if (_angle != value)
            {
                _angle = value;
                _transformCalc.SetAngle(_angle);
                _transformView.SetAngle(_angle, span);
            }
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            if (_point != value)
            {
                //Debug.WriteLine($"!! {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds})");
                _point = value;
                _transformCalc.SetPoint(_point);
                _transformView.SetPoint(_point, span);
            }
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            if (_isFlipHorizontal != value)
            {
                _isFlipHorizontal = value;
                _transformCalc.SetFlipHorizontal(_isFlipHorizontal);
                _transformView.SetFlipHorizontal(_isFlipHorizontal, span);
            }
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            if (_isFlipVertical != value)
            {
                _isFlipVertical = value;
                _transformCalc.SetFlipVertical(_isFlipVertical);
                _transformView.SetFlipVertical(_isFlipVertical, span);
            }
        }


        public void Flush()
        {
            _transformView.Flush();
        }

        public void Clear()
        {
            SetAngle(0.0, TimeSpan.Zero);
            SetScale(1.0, TimeSpan.Zero);
            SetPoint(default, TimeSpan.Zero);
            SetFlipHorizontal(false, TimeSpan.Zero);
            SetFlipVertical(false, TimeSpan.Zero);
            Flush();
        }
    }


    public class LoupeTransform
    {
        private double _scale = 1.0;
        private Point _point;
        private ScaleTransform _scaleTransform;
        private TranslateTransform _translateTransform;
        private TransformGroup _transformGroup;


        public LoupeTransform()
        {
            _scaleTransform = new ScaleTransform(1.0, 1.0);
            _translateTransform = new TranslateTransform();
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_translateTransform);
            _transformGroup.Children.Add(_scaleTransform);
        }


        public ScaleTransform ScaleTransform => _scaleTransform;
        public TranslateTransform TranslateTransform => _translateTransform;
        public Transform Transform => _transformGroup;


        public Point Point => _point;
        public double Scale => _scale;


        public void SetScale(double value)
        {
            if (_scale != value)
            {
                _scale = value;
                _scaleTransform.ScaleX = _scale;
                _scaleTransform.ScaleY = _scale;
            }
        }

        public void SetPoint(Point value)
        {
            if (_point != value)
            {
                _point = value;
                _translateTransform.X = _point.X;
                _translateTransform.Y = _point.Y;
            }
        }
    }
}
