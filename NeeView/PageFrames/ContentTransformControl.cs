using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using NeeLaboratory.ComponentModel;
using NeeView.ComponentModel;
using NeeView.Interop;
using NeeView.Maths;

namespace NeeView.PageFrames
{

    // TODO: ページ移動とPointの初期化問題
    public class ContentTransformControl : ITransformControl, IRevisePositionDelta
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainer _container;
        private readonly Rect _containerRect;
        private readonly ScrollLock _scrollLock;

        public ContentTransformControl(PageFrameContext context, PageFrameContainer container, Rect viewRect, ScrollLock scrollLock)
        {
            _context = context;
            _container = container;
            _containerRect = viewRect;
            _scrollLock = scrollLock;
        }


        public double Scale => _container.Transform.Scale;
        public double Angle => _container.Transform.Angle;
        public Point Point => _container.Transform.Point;
        public bool IsFlipHorizontal => _container.Transform.IsFlipHorizontal;
        public bool IsFlipVertical => _container.Transform.IsFlipVertical;


        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            _container.Transform.SetFlipHorizontal(value, span);
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            _container.Transform.SetFlipVertical(value, span);
        }

        public void SetScale(double value, TimeSpan span)
        {
            _container.Transform.SetScale(value, span);
            _scrollLock.Unlock();
        }

        public void SetAngle(double value, TimeSpan span)
        {
            _container.Transform.SetAngle(value, span);
            _scrollLock.Unlock();
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.IsSnapAnchor.Reset();
            _container.Transform.SetPoint(value, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _context.IsSnapAnchor.Reset();
            var delta = RevisePositionDelta(value);
            //var delta = DetectCollision(value);

            _container.Transform.SetPoint(_container.Transform.Point + delta, span, easeX, easeY);
        }

        delegate HitData GeHitFunc(Point start, Vector delta);

        public void InertiaPoint(Vector velocity)
        {
            if (velocity.LengthSquared < 0.01) return;

            if (velocity.LengthSquared > 40.0 * 40.0)
            {
                velocity = velocity * (40.0 / velocity.Length);
            }

            _context.IsSnapAnchor.Reset();

            var inertiaMath = new InertiaMath();
            var factory = new MyEaseFactory();

            MultiEaseSet multiEaseSet = new MultiEaseSet();
            var pos = _container.Transform.Point;


            {
                var easeSet = factory.CreateEase(velocity, 1.0);
                var hit = GetScrollLockHit(pos, easeSet.Delta);

                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = factory.CreateEase(velocity, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    Debug.WriteLine($"## Add.LockHit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                }
            }

            while (!velocity.NearZero(0.1))
            {
                var easeSet = factory.CreateEase(velocity, 1.0);

                var hit = GetAreaLimitHit(pos, easeSet.Delta);
                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = factory.CreateEase(velocity, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    Debug.WriteLine($"## Add.Hit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                    continue;
                }
                else
                {
                    multiEaseSet.Add(easeSet);
                    Debug.WriteLine($"## Add.End: Delta={easeSet.Delta:f2}, Rate={1}, V1={easeSet.V1:f2}");
                    break;
                }
            }

            _container.Transform.SetPoint(_container.Transform.Point + multiEaseSet.Delta, multiEaseSet.Span, multiEaseSet.EaseX, multiEaseSet.EaseY);

#if false
            EaseSet easeSet = factory.CreateEase(velocity, 0.5);

            //multiEaseSet.Add(easeSet);

            var easeSet1 = factory.CreateEase(velocity, 0.5);
            multiEaseSet.Add(easeSet1);

            if (!easeSet1.V1.NearZero(0.1))
            {
                var easeSet2 = factory.CreateEase(easeSet1.V1, 1.0);
                multiEaseSet.Add(easeSet2);
                Debug.Assert((velocity.TransNormal() - easeSet1.V1.TransNormal()).NearZero(0.0001));
            }

            _container.Transform.SetPoint(_container.Transform.Point + multiEaseSet.Delta, multiEaseSet.Span, multiEaseSet.EaseX, multiEaseSet.EaseY);
            //_container.Transform.SetPoint(_container.Transform.Point + easeSet.Delta, easeSet.Span, easeSet.EaseX, easeSet.EaseY);
#endif

#if false
            while (velocity.LengthSquared > 0.001)
            {
                var inertia = inertiaMath.CalcInertia(velocity);

                var hit = GetAreaLimitHit(pos, inertia.Delta);
                if (hit.IsHit)
                {
                    easeSet = factory.CreateEase(velocity, hit.Rate);

                }
                else
                {
                    easeSet = factory.CreateEase(velocity);
                    velocity = default;
                }
            }

            _container.Transform.SetPoint(_container.Transform.Point + easeSet.Delta, easeSet.Span, easeSet.EaseX, easeSet.EaseY);
#endif
        }


        // 範囲内になるよう移動量補正
        public Vector RevisePositionDelta(Vector delta)
        {
            var contentRect = _container.GetContentRect();

            if (_context.ViewConfig.IsLimitMove)
            {
                // scroll lock
                _scrollLock.Update(contentRect, _containerRect);
                delta = _scrollLock.Limit(delta);

                // scroll area limit
                var areaLimit = new ScrollAreaLimit(contentRect, _containerRect);
                delta = areaLimit.GetLimitContentMove(delta);
            }

            return delta;
        }

#if false
        public Vector DetectCollision(Vector delta)
        {
            var pos = _container.Transform.Point;

            var hit = GetScrollLockHit(pos, delta);
            if (hit.XHit)
            {
                delta = hit.Delta;
                //delta += hit.Reflect;
            }

            hit = GetAreaLimitHit(pos, delta);
            if (hit.IsHit)
            {
                //delta += hit.Reflect;
                delta = hit.Delta;
            }

            return delta;
        }
#endif


        private HitData GetScrollLockHit(Point start, Vector delta)
        {
            if (!_context.ViewConfig.IsLimitMove) return new HitData(start, delta);

            var contentRect = _container.GetContentRect(start);
            _scrollLock.Update(contentRect, _containerRect);
            return _scrollLock.HitTest(start, delta);
        }

        private HitData GetAreaLimitHit(Point start, Vector delta)
        {
            if (!_context.ViewConfig.IsLimitMove) return new HitData(start, delta);

            var contentRect = _container.GetContentRect(start);
            var areaLimit = new ScrollAreaLimit(contentRect, _containerRect);
            return areaLimit.HitTest(start, delta);
        }


        public void SnapView()
        {
            //if (!Config.Current.View.IsLimitMove) return;

            var contentRect = _container.GetContentRect();

            var areaLimit = new ScrollAreaLimit(contentRect, _containerRect);
            _container.Transform.SetPoint(areaLimit.SnapView(false), TimeSpan.Zero);
        }
    }

    /// <summary>
    /// 衝突判定用座標
    /// </summary>
    public class HitData
    {
        public HitData(Point startPoint, Vector delta)
        {
            StartPoint = startPoint;
            Delta = delta;
        }


        public Point StartPoint { get; init; }
        public Vector Delta { get; init; }

        public bool IsHit { get; init; }
        public double Rate { get; init; }
        public Vector Reflect { get; init; }
        //public Point HitPoint { get; init; }
        public Point HitPoint => StartPoint + Delta * Rate;

        public bool XHit { get; init; }
        public bool YHit { get; init; }
    }

    public static class Kinematics
    {
        /// <summary>
        /// 等加速度運動(UAM)：初速度を求める
        /// </summary>
        public static double GetFirstVelocity(double a, double s, double t)
        {
            Debug.Assert(t > 0.0);
            return (s / t) - (a * t * 0.5);
        }

        /// <summary>
        /// 等加速度運動：到達速度を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GetVelocity(double v, double a, double t)
        {
            return v + a * t;
        }

        /// <summary>
        /// 等加速度運動：距離を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GetSpan(double v, double a, double t)
        {
            return v * t + 0.5 * a * t * t;
        }

        /// <summary>
        /// 等加速度運動：到達時間を求める
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double GetTime(double v, double a, double s)
        {
            Debug.Assert(a != 0.0);

            var x = v * v + 2.0 * a * s;
            if (Math.Abs(x) < 0.001) x = 0.0;

            var y = Math.Sqrt(x);
            var t1 = (-v + y) / a;
            var t2 = (-v - y) / a;
            if (t1 < 0) return t2;
            if (t2 < 0) return t1;
            return Math.Min(t1, t2);
        }

        public static double GetStopTime(double v, double a)
        {
            Debug.Assert(a < 0.0);
            return v / (-a);
        }
    }

    public class MyEaseFactory
    {
        private double _a = -0.01; // _a is negative

        public Vector GetFirstVelocity(Vector delta, TimeSpan span)
        {
            var t = span.TotalMilliseconds;
            var s = delta.Length;
            var v = Kinematics.GetFirstVelocity(_a, s, t);
            return delta * (v / s);
        }

#if false
        /// <summary>
        /// 加速度と距離と時間から初速度を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns>v0</returns>
        private double GetFirstVelocity(double a, double s, double t)
        {
            return (s / t) - (a * t * 0.5);
        }

        public double GetReachTime(double v, double a, double s)
        {
            var x = Math.Sqrt(v * v - 2.0 * a * s);
            var t1 = (v + x) / a;
            var t2 = (v - x) / a;
            if (t1 < 0) return t2;
            if (t2 < 0) return t1;
            return Math.Min(t1, t2);
        }

        public double GetLastVelocity(double v, double a, double t)
        {
            return v + a * t;
        }
#endif

        public EaseSet CreateEase(Vector delta, TimeSpan span)
        {
            var v = GetFirstVelocity(delta, span);
            return CreateEase(v);
        }

        public EaseSet CreateEase(Vector v)
        {
            var inertiaMath = new InertiaMath();
            var inertia = inertiaMath.CalcInertia(v);
            var easeX = new MyEase(inertia.Velocity.X, -0.01);
            var easeY = new MyEase(inertia.Velocity.Y, -0.01);

            return new EaseSet(inertia.Delta, inertia.Span, v, new Vector(), easeX, easeY);
        }

        public EaseSet CreateEase(Vector velocity, double sRate)
        {
            var inertiaMath = new InertiaMath()
            {
                MaxSpeed = double.PositiveInfinity
            };

            var v0 = velocity.Length;

            var inertiaT = Kinematics.GetStopTime(v0, _a);
            var inertiaS = Kinematics.GetSpan(v0, _a, inertiaT);

            //var inertia = inertiaMath.CalcInertia(velocity);

            if (Math.Abs(inertiaS) < 0.1)
            {
                return new EaseSet(VectorExtensions.Zero, TimeSpan.Zero, velocity, velocity, new QuarticEase(), new QuarticEase());
            }

            //var vc = v0;
            var s = inertiaS * sRate;
            var t = Kinematics.GetTime(v0, _a, s);
            if (double.IsNaN(t))
            {
                Debug.Assert(!double.IsNaN(t));
                t = inertiaT * sRate; // ここにきてはいけないが、もしものための回避値を設定
            }

            var delta = velocity.TransScalar(s);
            var span = TimeSpan.FromMilliseconds(t);
            var tRate = t / inertiaT;

            var easeX = new MyEase(v0, -0.01) { MaxRate = tRate };
            var easeY = new MyEase(v0, -0.01) { MaxRate = tRate };

            var testX = easeX.Ease(1.0) * delta.X;
            Debug.Assert(testX == delta.X);
            var testY = easeY.Ease(1.0) * delta.Y;
            Debug.Assert(testY == delta.Y);

            var v1 = Kinematics.GetVelocity(v0, _a, t);
            //var vRate = v1 / v0;
            // Debug.Assert(!double.IsNaN(vRate));
            var lastVelocity = velocity.TransScalar(v1); // * vRate;

            Debug.WriteLine($"EaseSet: v0={velocity:f2}({v0:f2}), v1={lastVelocity:f2}({v1:f2}), delta={delta:f2}, ms={t:f0}");

            return new EaseSet(delta, span, velocity, lastVelocity, easeX, easeY);
        }
    }

    public class MultiEaseSet
    {
        private Vector _delta;
        private double _time;
        private MultiEase _easeX = new();
        private MultiEase _easeY = new();

        public void Add(EaseSet easeSet)
        {
            var t = easeSet.Span.TotalMilliseconds;

            _easeX.Add(easeSet.EaseX, easeSet.Delta.X, t);
            _easeY.Add(easeSet.EaseY, easeSet.Delta.Y, t);

            _delta += easeSet.Delta;
            _time += t;
        }

        public Vector Delta => _delta;
        public TimeSpan Span => TimeSpan.FromMilliseconds(_time);
        public MultiEase EaseX => _easeX;
        public MultiEase EaseY => _easeY;

    }

    public class MultiEase : EasingFunctionBase
    {
        private List<EaseItem> _items = new();

        public MultiEase()
        {
            EasingMode = EasingMode.EaseIn;
        }

        public MultiEase(IEnumerable<EaseItem> items)
        {
            _items = new List<EaseItem>(items);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MultiEase(_items)
            {
                EasingMode = EasingMode
            };
        }

        public void Add(IEasingFunction ease, double s, double t)
        {
            if (!_items.Any())
            {
                _items.Add(new EaseItem(ease, s, t, 0, 0));
            }
            else
            {
                var last = _items.Last();
                _items.Add(new EaseItem(ease, s, t, last.S1, last.T1));
            }
        }



        protected override double EaseInCore(double normalizedTime)
        {
            if (!_items.Any()) return normalizedTime;

            var totalS = _items.Last().S1;
            var totalT = _items.Last().T1;

            if (Math.Abs(totalS) < 0.001)
            {
                Debug.WriteLine($"{normalizedTime:f2}: [] s={1.0:f2} (no span)");
                return 1.0;
            }

            // select
            var item = _items.FirstOrDefault(e => normalizedTime * totalT <= e.T1) ?? _items.Last();
            var index = _items.IndexOf(item);

            // calc
            var t = (normalizedTime * totalT - item.T0) / (item.T1 - item.T0);
            if (double.IsNaN(t)) t = 1.0;
            var s = (item.S0 + item.Ease.Ease(t) * (item.S1 - item.S0)) / totalS;
            if (double.IsNaN(s)) s = 1.0;
            Debug.WriteLine($"{normalizedTime:f2}: [{index}] t={t:f2} s={s:f2}");
            return s;
        }
    }

    public record class EaseItem(IEasingFunction Ease, double S, double T, double S0, double T0)
    {
        public double S1 => S0 + S;
        public double T1 => T0 + T;
    }


    public class EaseSet
    {
        public EaseSet(Vector delta, TimeSpan span, Vector v0, Vector v1, IEasingFunction easeX, IEasingFunction easeY)
        {
            Delta = delta;
            Span = span;
            EaseX = easeX;
            EaseY = easeY;
            V0 = v0;
            V1 = v1;
        }

        public Vector Delta { get; }
        public TimeSpan Span { get; }
        public IEasingFunction EaseX { get; }
        public IEasingFunction EaseY { get; }

        public Vector V0 { get; }
        public Vector V1 { get; }
    }

    /// <summary>
    /// 減速曲線
    /// </summary>
    public class MyEase : EasingFunctionBase
    {
        private double _v;
        private double _a; // negative
        private double _tmax;
        private double _smax;


        public MyEase() : this(1.0, -0.01)
        {
        }

        public MyEase(double v, double a)
        {
            Debug.Assert(a < 0.0);

            _v = v;
            _a = a;
            var t = Math.Abs(v / a);
            var s = Kinematics.GetSpan(v, a, t);
            _tmax = t;
            _smax = s;

            EasingMode = EasingMode.EaseIn;
        }

        //public double Velocity { get; set; }

        // sRate
        public double MinRate { get; set; } = 0.0;
        public double MaxRate { get; set; } = 1.0;


        protected override Freezable CreateInstanceCore()
        {
            return (MyEase)MemberwiseClone();
#if false
            return new MyEase()
            {
                EasingMode = EasingMode,
                MinRate = MinRate,
                MaxRate = MaxRate,
            };
#endif
        }

        protected override double EaseInCore(double normalizedTime)
        {
            var nt = MinRate + (MaxRate - MinRate) * normalizedTime;
            var t = nt * _tmax;
            var s = Kinematics.GetSpan(_v, _a, t);

            var s0 = Kinematics.GetSpan(_v, _a, MinRate * _tmax);
            var s1 = Kinematics.GetSpan(_v, _a, MaxRate * _tmax);

            var result = (s - s0) / (s1 - s0);

            Debug.Assert(!double.IsNaN(result));
            //var result = s / _smax;
            Debug.WriteLine($"MyEase: {normalizedTime:f2} -> {result:f2}");
            return result;
        }
    }




}