﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{

    /// <summary>
    /// Simple StateMahchine
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StateMachine<T>
    {
        public interface IState
        {
            void Initialize(T context);
            void Execute(T context);
        }

        //
        public T _context;

        //
        private IState _state;

        //
        public StateMachine(T context)
        {
            _context = context;
        }

        //
        public void SetState(IState state)
        {
            if (_state == state) return;

            _state = state;
            _state?.Initialize(_context);
        }

        //
        public void Execute()
        {
            _state?.Execute(_context);
        }
    }


    /// <summary>
    /// タッチ通常ドラッグ状態
    /// </summary>
    public class TouchInputDrag : TouchInputBase
    {
        private TouchDragManipulation _manipulation;

        /*
        private Dictionary<TouchDevice, TouchContext> _touchMap;

        //
        private DragTransform _transform;

        //
        private TouchDragContext _origin;
        */

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputDrag(TouchInputContext context) : base(context)
        {
            _manipulation = new TouchDragManipulation(context);
            //_transform = context.DragTransform;
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            Debug.WriteLine("TouchState: Drag");

            //InitializeTouchMap();
            _manipulation.Start();
        }


        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            _manipulation.Stop();
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            _manipulation.Start();
            //InitializeTouchMap();
            //_now.Add(e.TouchDevice, e.GetTouchPoint(_context.Sender).Position);
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            // タッチされなくなったら解除
            if (_context.TouchMap.Count < 1)
            {
                ResetState();
            }
            else
            {
                _manipulation.Start();
                //InitializeTouchMap();
                // _now.Remove(e.TouchDevice);
            }
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            _manipulation.Darty();
        }



    }

    //
    public class TouchDragTransform
    {
        public Vector Trans { get; set; }
        public double Angle { get; set; }
        public double Scale { get; set; }

        public TouchDragTransform Clone()
        {
            return (TouchDragTransform)this.MemberwiseClone();
        }


        internal void Add(TouchDragTransform m)
        {
            this.Trans += m.Trans;
            this.Angle += m.Angle;
            this.Scale += m.Scale;
        }

        internal void Sub(TouchDragTransform m)
        {
            this.Trans -= m.Trans;
            this.Angle -= m.Angle;
            this.Scale -= m.Scale;
        }

        internal void Multiply(double v)
        {
            this.Trans *= v; // (Point)(v * (Vector)this.Trans);
            this.Angle *= v;
            this.Scale *= v;
        }

        public bool IsNearZero()
        {
            return ((Vector)this.Trans).LengthSquared < 1.0 && this.Angle < 1.0 && this.Scale < 0.01;
        }

        public static TouchDragTransform Sub(TouchDragTransform m0, TouchDragTransform m1)
        {
            var m = m0.Clone();
            m.Sub(m1);
            return m;
        }

        public static TouchDragTransform Lerp(TouchDragTransform m0, TouchDragTransform m1, double t)
        {
            t = NVUtility.Clamp(t, 0.0, 1.0);

            return new TouchDragTransform()
            {
                Trans = m0.Trans + (m1.Trans - m0.Trans) * t,
                Angle = m0.Angle + (m1.Angle - m0.Angle) * t,
                Scale = m0.Scale + (m1.Scale - m0.Scale) * t,
            };
        }

    }

    //
    public class TouchDragManipulation
    {
        private Dictionary<StylusDevice, TouchContext> _touchMap;
        private DragTransform _transform;
        private TouchDragContext _origin;


        TouchDragTransform _start;
        TouchDragTransform _goal;
        TouchDragTransform _now;


        private bool _ticking;
        private bool _darty;

        private bool _allowAngle;
        private bool _allowScale;

        private TouchInputContext _context;

        private StateMachine<TouchDragManipulation> _stateMachine;


        //
        public TouchDragManipulation(TouchInputContext context)
        {
            _context = context;
            _transform = context.DragTransform;

            _stateMachine = new StateMachine<TouchDragManipulation>(this);
        }


        //
        public void Start()
        {
            Debug.WriteLine($"Drag: reset");

            // clone touch map
            _touchMap = new Dictionary<StylusDevice, TouchContext>(_context.TouchMap);

            // get origin
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

            // 
            _start = new TouchDragTransform()
            {
                Trans = (Vector)_transform.Position,
                Angle = _transform.Angle,
                Scale = _transform.Scale,
            };

            _prev = _start.Clone();
            _goal = _start.Clone();
            _now = _ticking ? _now : _start.Clone();

            _darty = true;

            _allowAngle = false;
            _allowScale = false;

            //_controled = true;

            _stateMachine.SetState(new StateNormal());

            //
            StartTicking();
        }

        //
        public void Stop()
        {
            //StopTicking();

            //_delta = TouchDragTransform.Sub(_goal, _prev);
            //_controled = false;
            _stateMachine.SetState(new StateIntertia());

            Debug.WriteLine($"{_speed}");
        }

        //
        public void Darty()
        {
            _darty = true;
        }

        private void StartTicking()
        {
            if (!_ticking)
            {
                _ticking = true;
                CompositionTarget.Rendering += new EventHandler(OnRendering);

                _snapAngle = _transform.Angle;
                _snapCenter = default(Vector);
            }
        }

        private void StopTicking()
        {
            if (_ticking)
            {
                CompositionTarget.Rendering -= new EventHandler(OnRendering);
                _ticking = false;
            }
        }




        private void OnRendering(object sender, EventArgs e)
        {
            ReportFrame();
        }

        private void ReportFrame()
        {
            _stateMachine.Execute();
            /*
                        if (_controled)
                        {
                            ControlFrame();
                        }
                        else
                        {
                            IntertiaFrame();
                        }
                */
        }

        //private bool _controled;
        private TouchDragTransform _prev;
        //private TouchDragTransform _delta;

        private Vector _speed;
        private double _snapAngle;
        private Vector _snapCenter;


        private void ControlFrame()
        {
            if (_darty)
            {
                _darty = false;

                _prev = _goal.Clone();
                _goal = GetTransform();
                _snapCenter = _controlCenter;
            }

            var old = _now;

            _now = TouchDragTransform.Lerp(_now, _goal, 0.5);

            _transform.Position = (Point)_now.Trans;
            _transform.Angle = _now.Angle;
            _transform.Scale = _now.Scale;

            // ready 慣性
            var speed = _now.Trans - old.Trans;
            _speed = NVUtility.Lerp(_speed, speed, 0.25);


            if (_transform.AngleFrequency > 0.0)
            {
                var delta = _now.Angle - _start.Angle;
                var direction = delta > 0.0 ? 1.0 : -1.0;

                if (Math.Abs(delta) > 1.0)
                {
                    _snapAngle = Math.Floor((_goal.Angle + _transform.AngleFrequency * (0.5 + direction * 0.25)) / _transform.AngleFrequency) * _transform.AngleFrequency;
                }
            }
            else
            {
                _snapAngle = _goal.Angle;
            }
        }

        //
        private void IntertiaFrame()
        {
            // trans
            _speed *= 0.9;
            _now.Trans += _speed;


            // angle
            if (_now.Angle != _snapAngle)
            {
                var oldAngle = _now.Angle;
                _now.Angle = NVUtility.Lerp(_now.Angle, _snapAngle, 0.5);

                var m = new RotateTransform(_now.Angle - oldAngle);
                var v = _snapCenter - _now.Trans;
               _now.Trans += v - (Vector)m.Transform((Point)v);
            }


            // snap
            if (_transform.IsLimitMove)
            {
                // レイアウト更新
                _context.Sender.UpdateLayout();
                var area = _context.GetArea();

                _now.Trans = (_now.Trans + area.SnapView(_now.Trans)) * 0.5;
            }




            //
            _transform.Position = (Point)_now.Trans;
            _transform.Angle = _now.Angle;



            if (_speed.LengthSquared < 4.0 && Math.Abs(_now.Angle - _snapAngle) < 1.0)
            {
                _transform.Angle = _snapAngle;

                if (_transform.IsLimitMove)
                {
                    var area = _context.GetArea();
                    _transform.Position = (Point)area.SnapView(_now.Trans);
                }

                StopTicking();
                _stateMachine.SetState(null);
            }
        }


        //
        private class StateNormal : StateMachine<TouchDragManipulation>.IState
        {
            public void Execute(TouchDragManipulation context)
            {
                context.ControlFrame();
            }

            public void Initialize(TouchDragManipulation context)
            {
            }
        }


        //
        private class StateIntertia : StateMachine<TouchDragManipulation>.IState
        {
            public void Execute(TouchDragManipulation context)
            {
                context.IntertiaFrame();
            }

            public void Initialize(TouchDragManipulation context)
            {
                Debug.WriteLine($"Speed: {context._speed}");
            }
        }




        // TODO: この変数の使い方はおかしい
        private Vector _controlCenter;


        private TouchDragTransform GetTransform()
        {
            var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);
            var area = _context.GetArea();


            // transform
            var move = current.GetMove(_origin);

            // ターゲット座標系での操作系中心
            var center = current.Center - new Point(area.View.Width * 0.5, area.View.Height * 0.5); // - (Vector)position;
                                                                                                    //Debug.WriteLine($"center: {(int)center.X,3}, {(int)center.Y,3}: {move}");




            // rotate
            var angle = current.GetAngle(_origin);

            _allowAngle = _allowAngle || (current.Radius > 80.0 && Math.Abs(current.Radius * 2.0 * Math.Sin(angle * 0.5 * Math.PI / 180)) > 30.0);
            angle = _allowAngle ? angle : 0.0;


            //  scale
            var scale = current.GetScale(_origin);

            _allowScale = _allowScale || (current.Radius > 80.0 && Math.Abs(current.Radius - _origin.Radius) > 30.0);
            scale = _allowScale ? scale : 1.0;


            var p = _start.Trans;


            // TODO: この変数の使い方はおかしい
            _controlCenter = center; // - p;


            // move
            p = p + move;

            // rotate
            var m = new RotateTransform(angle);
            var v = (Point)(p - center);
            p = center + (Vector)m.Transform(v);

            // scale
            var rate = scale; // _transform.Scale / _baseScale;
            p = p - (center - p) * (rate - 1.0);




            // _transform.Position = p;

            return new TouchDragTransform
            {
                Trans = p,
                Angle = _start.Angle + angle,
                Scale = _start.Scale * scale
            };
        }

    }

    //
    public class TouchDragUnit
    {
        //public TouchDevice TouchDevice { get; private set; }
        public Point Position { get; set; }
        public double Length { get; set; }
    }

    //
    public class TouchDragContext
    {
        private FrameworkElement _sender;

        private Dictionary<StylusDevice, TouchDragUnit> _touches;

        public Point Center { get; private set; }

        public double Radius { get; private set; }


        //
        public TouchDragContext(FrameworkElement sender, IEnumerable<StylusDevice> touchDevices)
        {
            _sender = sender;

            _touches = touchDevices.ToDictionary(e => e, e => new TouchDragUnit() { Position = e.GetPosition(sender) });

            var positions = _touches.Values.Select(e => e.Position);
            this.Center = new Point(positions.Average(e => e.X), positions.Average(e => e.Y));

            foreach (var touch in this._touches.Values)
            {
                touch.Length = (touch.Position - this.Center).Length;
            }

            this.Radius = _touches.Values.Select(e => e.Length).Max();
        }

        //
        public Vector GetMove(TouchDragContext source)
        {
            return this.Center - source.Center;
        }

        //
        public double GetScale(TouchDragContext source)
        {
            if (_touches.Count < 2) return 1.0;
            return _touches.Select(e => e.Value.Length / source._touches[e.Key].Length).Average();
        }

        //
        public double GetAngle(TouchDragContext source)
        {
            if (_touches.Count < 2) return 0.0;

            var v1 = source.GetVector();
            var v2 = this.GetVector();
            return Vector.AngleBetween(v1, v2);
        }

        //
        public Vector GetVector()
        {
            return _touches.Last().Value.Position - _touches.First().Value.Position;
        }

    }
}
