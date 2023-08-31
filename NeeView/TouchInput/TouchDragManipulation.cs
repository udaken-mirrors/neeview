using NeeView.Maths;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace NeeView
{
    /// <summary>
    /// タッチ操作によるビューの変更
    /// </summary>
    public class TouchDragManipulation
    {
        private IDragTransformContextFactory _transformContextFactory;
        private DragTransformContext? _transformContext;

        private DragTransform? _transform;
        private TouchDragContext? _origin;

        private TouchDragTransform? _start;
        private TouchDragTransform? _goal;

        private bool _allowAngle;
        private bool _allowScale;

        private readonly TouchInputContext _context;

        private Speedometer _speedometer = new();


        public TouchDragManipulation(TouchInputContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (context.DragTransformContextFactory is null) throw new ArgumentException("context.DragTransformContextFactory must not be null.");

            _context = context;
            _transformContextFactory = _context.DragTransformContextFactory;
        }


        public void Initialize()
        {
            _transformContext = _transformContextFactory.CreateDragTransformContext(true, false);
            Debug.Assert(_transformContext != null);

            _transform = new DragTransform(_transformContext);
        }

        /// <summary>
        /// タッチ操作開始
        /// タッチ数が変化したときに呼ばれる
        /// </summary>
        public void Start()
        {
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            _start = GetNowTransform();
            _goal = _start.Clone();

            _allowAngle = false;
            _allowScale = false;
        }



        /// <summary>
        /// タッチ操作終了
        /// タッチ数が０になったときに呼ばれる
        /// </summary>
        public void Stop(int timestamp)
        {
            Inertia(timestamp);
        }

        /// <summary>
        /// タッチ操作情報変化
        /// </summary>
        public void Update(object? sender, StylusEventArgs e)
        {
            ControlDragTransform(e.Timestamp);
        }

        private void ControlDragTransform(int timestamp)
        {
            if (_goal is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_transform is null) return;

            _goal = GetTransform();

            var deltaAngle = Math.Abs(_goal.Angle - _transform.Angle);
            var deltaScale = Math.Abs(_goal.Scale - _transform.Scale);
            if (deltaAngle > 1.0 || deltaScale > 0.1)
            {
                _speedometer.Initialize((Point)_goal.Trans, timestamp);
            }
            else
            {
                _speedometer.Update((Point)_goal.Trans, timestamp);
            }

            var spam = TimeSpan.FromMilliseconds(100);
            _transform.SetPoint((Point)_goal.Trans, spam);
            _transform.SetAngle(_goal.Angle, spam);
            _transform.SetScale(_goal.Scale, spam);

#if false
            // speed.
            var speed = _now.Trans - old.Trans;
            var deltaAngle = Math.Abs(_now.Angle - old.Angle);
            if (deltaAngle > 1.0) speed = speed * 0.0;
            var deltaScale = Math.Abs(_now.Scale - old.Scale);
            if (deltaScale > 0.1) speed = speed * 0.0;
            _speed = VectorExtensions.Lerp(_speed, speed * 1.25, 0.25);
#endif
        }



        private void Inertia(int timestamp)
        {
            if (_transform is null) return;
            
            _goal = GetInertiaTransform(timestamp);

            var span = TimeSpan.FromMilliseconds(500);
            _transform.SetPoint((Point)_goal.Trans, span);
            _transform.SetAngle(_goal.Angle, TimeSpan.FromMilliseconds(100));
        }


        private TouchDragTransform GetNowTransform()
        {
            Debug.Assert(_transformContext != null);
            Debug.Assert(_transform != null);
            return new TouchDragTransform((Vector)_transform.Point, _transform.Angle, _transform.Scale, (Vector)_transformContext.ContentCenter);
        }


        private TouchDragTransform GetInertiaTransform(int timestamp)
        {
            if (_goal is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_transformContext is null) throw new InvalidOperationException();

            _speedometer.Update((Point)_goal.Trans, timestamp);

            // TODO: Span どこから？
            var span = TimeSpan.FromMilliseconds(500);
            // TODO: 距離倍率 0.5 を再検討
            var inertia = _speedometer.Speed * span.TotalMilliseconds * 0.5;


            var trans = inertia;
            var angle = GetSnapAngle(_goal.Angle) - _goal.Angle;
            var scale = 1.0;

            // limit move
            if (Config.Current.View.IsLimitMove)
            {
                var transformContext = _transformContextFactory.CreateDragTransformContext(_transformContext.Container, false);
                if (transformContext is not null)
                {
                    var contentRect = Rect.Offset(transformContext.ContentRect, inertia);
                    var areaLimit = new ScrollAreaLimit(contentRect, transformContext.ViewRect);
                    var p0 = transformContext.ContentCenter;
                    var p1 = areaLimit.SnapView(true);
                    trans = p1 - p0;

                    if (!Config.Current.BookSetting.PageMode.IsStaticFrame())
                    {
                        if (Config.Current.Book.Orientation == PageFrameOrientation.Horizontal)
                        {
                            trans.X = inertia.X;
                        }
                        else
                        {
                            trans.Y = inertia.Y;
                        }
                    }
                }
            }

            //Debug.WriteLine($"## SPEED: {inertia:f0}");
            //Debug.WriteLine($"Trans={trans:f0}");
            //Debug.WriteLine($"Angle={GetSnapAngle(_goal.Angle)}");

            var delta = new TouchDragTransform()
            {
                Trans = trans,
                Angle = angle,
                Scale = scale,
                Center = new Vector(0, 0),
            };

            return AddTransform(_goal, delta);
        }

        /// <summary>
        /// タッチ状態から変換情報を求める
        /// </summary>
        /// <returns></returns>
        private TouchDragTransform GetTransform()
        {
            if (_origin is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_start is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_transformContext is null) throw new InvalidOperationException();

            var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            // center
            var center = (Vector)current.Center;

            // move
            var move = current.GetMove(_origin);

            // rotate
            var angle = current.GetAngle(_origin);
            _allowAngle = Config.Current.Touch.IsAngleEnabled && (_allowAngle || (current.Radius > Config.Current.Touch.MinimumManipulationRadius && Math.Abs(current.Radius * 2.0 * Math.Sin(angle * 0.5 * Math.PI / 180)) > Config.Current.Touch.MinimumManipulationDistance));
            angle = _allowAngle ? angle : 0.0;

            //  scale
            var scale = current.GetScale(_origin);
            _allowScale = Config.Current.Touch.IsScaleEnabled && (_allowScale || (current.Radius > Config.Current.Touch.MinimumManipulationRadius && Math.Abs(current.Radius - _origin.Radius) > Config.Current.Touch.MinimumManipulationDistance));
            scale = _allowScale ? scale : 1.0;

            var delta = new TouchDragTransform(move, angle, scale, center - _start.Center);
            return AddTransform(_start, delta);
        }

        /// <summary>
        /// TouchDragTransform の合成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private TouchDragTransform AddTransform(TouchDragTransform source, TouchDragTransform delta)
        {
            if (_transformContext is null) throw new InvalidOperationException();
            var contentCenter = _transformContext.ContentCenter;

            // center
            var center = source.Center + delta.Center;

            // trans
            var trans = (Vector)contentCenter;
            trans = trans + delta.Trans;

            // rotate
            var m = new RotateTransform(delta.Angle);
            trans = center + (Vector)m.Transform((Point)(trans - center));

            // scale
            trans = trans + (trans - center) * (delta.Scale - 1.0);

            var diff = trans - (Vector)contentCenter;
            return new TouchDragTransform(source.Trans + diff, source.Angle + delta.Angle, source.Scale * delta.Scale, center);
        }


        /// <summary>
        /// スナップ角度を求める
        /// </summary>
        /// <returns></returns>
        private double GetSnapAngle(double angle)
        {
            if (_start is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            if (Config.Current.View.AngleFrequency > 0.0)
            {
                var delta = angle - _start.Angle;

                if (Math.Abs(delta) > 1.0)
                {
                    var direction = delta > 0.0 ? 1.0 : -1.0;
                    return Math.Floor((angle + Config.Current.View.AngleFrequency * (0.5 + direction * 0.25)) / Config.Current.View.AngleFrequency) * Config.Current.View.AngleFrequency;
                }
            }

            return angle;
        }


    }
}
