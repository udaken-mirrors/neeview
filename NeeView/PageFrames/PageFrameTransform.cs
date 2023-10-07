﻿using NeeLaboratory.Generators;
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
        private readonly MultiTransform _transform;


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
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Scale));
            }
        }

        public void SetAngle(double value, TimeSpan span)
        {
            if (SetProperty(ref _angle, value, nameof(Angle)))
            {
                _transform.SetAngle(_angle, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Angle));
            }
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (SetProperty(ref _point, value, nameof(Point)))
            {
                _transform.SetPoint(_point, span, easeX, easeY);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.Point));
            }
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            if (SetProperty(ref _isFlipHorizontal, value, nameof(IsFlipHorizontal)))
            {
                _transform.SetFlipHorizontal(value, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.FlipHorizontal));
            }
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            if (SetProperty(ref _isFlipVertical, value, nameof(IsFlipVertical)))
            {
                _transform.SetFlipVertical(value, span);
                TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.Content, TransformAction.FlipVertical));
            }
        }

        public Vector GetVelocity()
        {
            return _transform.GetVelocity();
        }

        public void ResetVelocity()
        {
            _transform.ResetVelocity();
        }

        public void Clear()
        {
            Clear(TransformMask.All);
        }

        public void Clear(TransformMask mask)
        {
            if (mask.HasFlag(TransformMask.Flip))
            {
                SetFlipHorizontal(false, TimeSpan.Zero);
                SetFlipVertical(false, TimeSpan.Zero);
            }
            if (mask.HasFlag(TransformMask.Scale))
            {
                SetScale(1.0, TimeSpan.Zero);
            }
            if (mask.HasFlag(TransformMask.Angle))
            {
                SetAngle(0.0, TimeSpan.Zero);
            }
            if (mask.HasFlag(TransformMask.Point))
            {
                SetPoint(default, TimeSpan.Zero);
            }
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

        private readonly ScaleTransform _flipTransform;
        private readonly ScaleTransform _scaleTransform;
        private readonly RotateTransform _rotateTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;


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

        private readonly ScaleTransform _flipTransform;
        private readonly ScaleTransform _scaleTransform;
        private readonly RotateTransform _rotateTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;


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
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _point = value;
            SetPropertyValue(_translateTransform, TranslateTransform.XProperty, value.X, span, easeX);
            SetPropertyValue(_translateTransform, TranslateTransform.YProperty, value.Y, span, easeY);
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


        private void SetPropertyValue(Animatable anime, DependencyProperty dp, double value, TimeSpan span, IEasingFunction? ease = null)
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
                animation.EasingFunction = ease ?? EaseTools.DefaultEase;
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
        private bool _isFlipHorizontal;
        private bool _isFlipVertical;
        private readonly NormalTransform _transformCalc;
        private readonly AnimatableTransform _transformView;
        private readonly Speedometer _speedometer = new();
        private bool _isSpeedometerEnabled;

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

        public bool IsSpeedometerEnabled
        {
            get { return _isSpeedometerEnabled; }
            set
            {
                if (_isSpeedometerEnabled != value)
                {
                    _isSpeedometerEnabled = value;
                    if (_isSpeedometerEnabled)
                    {
                        _transformView.Transform.Changed += ViewTransform_Changed;
                    }
                    else
                    {
                        _transformView.Transform.Changed -= ViewTransform_Changed;
                    }
                }
            }
        }


        private void ViewTransform_Changed(object? sender, EventArgs e)
        {
            if (!IsSpeedometerEnabled) return;
            _speedometer.Add(GetViewPoint());
        }

        private Point GetViewPoint()
        {
            var m = _transformView.Transform.Value;
            return new Point(m.OffsetX, m.OffsetY);
        }

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
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (_point != value)
            {
                //Debug.WriteLine($"!! {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds})");
                _point = value;
                _transformCalc.SetPoint(_point);
                _transformView.SetPoint(_point, span, easeX, easeY);
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

        public Vector GetVelocity()
        {
            if (!IsSpeedometerEnabled) return default;
            _speedometer.Touch();
            return _speedometer.GetVelocity();
        }

        public void ResetVelocity()
        {
            if (!IsSpeedometerEnabled) return;
            _speedometer.Reset();
            _speedometer.Add(GetViewPoint());
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
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;


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

    public static class EaseTools
    {
        //public static IEasingFunction DefaultEase { get; } = new CubicEase() { EasingMode = EasingMode.EaseOut };
        public static IEasingFunction DefaultEase { get; } = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
    }
}