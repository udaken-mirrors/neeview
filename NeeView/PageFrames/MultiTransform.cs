using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
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
}
