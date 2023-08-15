using NeeLaboratory;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// タッチ操作によるビューの変更
    /// </summary>
    public class TouchDragManipulation
    {
        private interface IState
        {
            void Initialize(TouchDragManipulation context);
            void Execute(TouchDragManipulation context);
        }


        private class StateMachine
        {
            public TouchDragManipulation _context;
            private IState? _state;

            //
            public StateMachine(TouchDragManipulation context)
            {
                _context = context;
            }

            //
            public void SetState(IState? state)
            {
                if (_state?.GetType() == state?.GetType()) return;

                _state = state;
                _state?.Initialize(_context);
            }

            //
            public void Execute()
            {
                _state?.Execute(_context);
            }
        }


        // Fields

        private readonly DragTransform _transform;
        private TouchDragContext? _origin;

        private TouchDragTransform? _base;
        private TouchDragTransform? _start;
        private TouchDragTransform? _goal;
        private TouchDragTransform? _now;

        private Vector _speed;

        private Vector _snapCenter;
        private double _snapAngle;

        private bool _ticking;
        private bool _darty;

        private bool _allowAngle;
        private bool _allowScale;

        private readonly TouchInputContext _context;

        private readonly StateMachine _stateMachine;


        // Constructors

        public TouchDragManipulation(TouchInputContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (context.Target is null) throw new ArgumentException("context.Target must not be null.");
            if (context.DragTransform is null) throw new ArgumentException("context.DragTransform must not be null.");

            _context = context;
            _transform = context.DragTransform;

            _stateMachine = new StateMachine(this);
        }


        // Methods

        /// <summary>
        /// タッチ操作開始
        /// タッチ数が変化したときに呼ばれる
        /// </summary>
        public void Start()
        {
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            _start = new TouchDragTransform()
            {
                Trans = (Vector)_transform.Point,
                Angle = _transform.Angle,
                Scale = _transform.Scale,
            };

            _goal = _start.Clone();
            _base = _start.Clone();
            _now = _start.Clone();

            _darty = true;

            _allowAngle = false;
            _allowScale = false;

            _stateMachine.SetState(new StateControl());
        }

        /// <summary>
        /// タッチ操作終了
        /// タッチ数が０になったときに呼ばれる
        /// </summary>
        public void Stop()
        {
            _stateMachine.SetState(new StateIntertia());
        }

        /// <summary>
        /// タッチ操作情報変化
        /// </summary>
        public void Update()
        {
            _darty = true;
        }


        //
        private void StartTicking()
        {
            if (!_ticking)
            {
                _ticking = true;
                CompositionTarget.Rendering += OnRendering;
            }
        }

        //
        private void StopTicking()
        {
            if (_ticking)
            {
                CompositionTarget.Rendering -= OnRendering;
                _ticking = false;
            }
        }

        //
        private void OnRendering(object? sender, EventArgs e)
        {
            _stateMachine.Execute();
        }


        /// <summary>
        /// フレーム状態：制御
        /// </summary>
        private class StateControl : IState
        {
            public void Execute(TouchDragManipulation context)
            {
                context.StateControl_Execute();
            }

            public void Initialize(TouchDragManipulation context)
            {
                context.StateControl_Initialize();
            }
        }

        //
        private void StateControl_Initialize()
        {
            if (_start is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            _base = _start.Clone();
            _now = _start.Clone();

            _snapCenter = default;
            _snapAngle = _now.Angle;

            StartTicking();
        }

        //
        private void StateControl_Execute()
        {
            if (_now is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_goal is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            if (_darty)
            {
                _darty = false;
                _goal = GetTransform();

                if (_goal.IsValidCenter)
                {
                    _snapCenter = _goal.Center;
                    _snapAngle = GetSnapAngle();
                }
            }

            var old = _now;

            _now = TouchDragTransform.Lerp(_now, _goal, 0.5);

            _transform.SetPoint((Point)_now.Trans, TimeSpan.Zero);
            _transform.SetAngle(_now.Angle, TimeSpan.Zero); // TransformActionType.Touch);
            _transform.SetScale(_now.Scale, TimeSpan.Zero); // TransformActionType.Touch);

            // speed.
            var speed = _now.Trans - old.Trans;
            var deltaAngle = Math.Abs(_now.Angle - old.Angle);
            if (deltaAngle > 1.0) speed = speed * 0.0;
            var deltaScale = Math.Abs(_now.Scale - old.Scale);
            if (deltaScale > 0.1) speed = speed * 0.0;
            _speed = VectorExtensions.Lerp(_speed, speed * 1.25, 0.25);
        }


        /// <summary>
        /// フレーム状態：慣性
        /// </summary>
        private class StateIntertia : IState
        {
            public void Execute(TouchDragManipulation context)
            {
                context.StateIntertia_Execute();
            }

            public void Initialize(TouchDragManipulation context)
            {
                // nop.
            }
        }

        private void StateIntertia_Execute()
        {
            if (_now is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            // trans
            _speed *= 0.9;
            _now.Trans += _speed;

            // snap angle
            if (_now.Angle != _snapAngle)
            {
                var oldAngle = _now.Angle;
                _now.Angle = MathUtility.Lerp(_now.Angle, _snapAngle, 0.5);

                var m = new RotateTransform(_now.Angle - oldAngle);
                var v = _snapCenter - _now.Trans;
                _now.Trans += v - (Vector)m.Transform((Point)v);
            }

            // snap trans
            if (Config.Current.View.IsLimitMove)
            {
                // レイアウト更新
                _context.Sender.UpdateLayout();
                var area = _context.GetArea();

                _now.Trans = VectorExtensions.Lerp(_now.Trans, area.SnapView(_now.Trans, true), 0.5);
            }

            //
            _transform.SetPoint((Point)_now.Trans, TimeSpan.Zero);
            _transform.SetAngle(_now.Angle, TimeSpan.Zero); //, TransformActionType.Touch);

            // 終了チェック
            if (_speed.LengthSquared < 4.0 && Math.Abs(_now.Angle - _snapAngle) < 1.0)
            {
                _transform.SetAngle(_snapAngle, TimeSpan.Zero); // TransformActionType.Touch);

                if (Config.Current.View.IsLimitMove)
                {
                    var area = _context.GetArea();
                    _transform.SetPoint((Point)area.SnapView(_now.Trans, true), TimeSpan.Zero);
                }

                StopTicking();
                _stateMachine.SetState(null);
            }
        }



        /// <summary>
        /// タッチ状態から変換情報を求める
        /// </summary>
        /// <returns></returns>
        private TouchDragTransform GetTransform()
        {
            if (_origin is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_start is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);
            var area = _context.GetArea();

            // center
            var center = current.Center - new Point(area.ViewRect.Width * 0.5, area.ViewRect.Height * 0.5);

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


            // trans
            var trans = _start.Trans;
            trans = trans + move;

            // rotate
            var m = new RotateTransform(angle);
            trans = center + (Vector)m.Transform((Point)(trans - center));

            // scale
            trans = trans + (trans - center) * (scale - 1.0);


            //
            return new TouchDragTransform
            {
                Trans = trans,
                Angle = _start.Angle + angle,
                Scale = _start.Scale * scale,

                IsValidCenter = (angle != 0.0 || scale != 1.0),
                Center = center,
            };
        }



        /// <summary>
        /// スナップ角度を求める
        /// </summary>
        /// <returns></returns>
        private double GetSnapAngle()
        {
            if (_goal is null) throw new InvalidOperationException("TouchDragManipulation must be started");
            if (_base is null) throw new InvalidOperationException("TouchDragManipulation must be started");

            if (Config.Current.View.AngleFrequency > 0.0)
            {
                var delta = _goal.Angle - _base.Angle;

                if (Math.Abs(delta) > 1.0)
                {
                    var direction = delta > 0.0 ? 1.0 : -1.0;
                    return Math.Floor((_goal.Angle + Config.Current.View.AngleFrequency * (0.5 + direction * 0.25)) / Config.Current.View.AngleFrequency) * Config.Current.View.AngleFrequency;
                }
            }

            return _goal.Angle;
        }

    }
}
