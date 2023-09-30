using System;
using System.Diagnostics;
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

            MultiEaseSet multiEaseSet = new MultiEaseSet();
            var pos = _container.Transform.Point;

            // scroll lock
            {
                var easeSet = DecelerationEaseSetFactory.Create(velocity, 1.0);
                var hit = GetScrollLockHit(pos, easeSet.Delta);

                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = DecelerationEaseSetFactory.Create(velocity, hit.Rate);
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

            // area limit
            while (!velocity.NearZero(0.1))
            {
                var easeSet = DecelerationEaseSetFactory.Create(velocity, 1.0);

                var hit = GetAreaLimitHit(pos, easeSet.Delta);
                if (hit.IsHit)
                {
                    if (0.001 < hit.Rate)
                    {
                        easeSet = DecelerationEaseSetFactory.Create(velocity, hit.Rate);
                        multiEaseSet.Add(easeSet);
                        pos += easeSet.Delta;
                        velocity = easeSet.V1;
                    }
                    var vx = hit.XHit ? 0.0 : velocity.X;
                    var vy = hit.YHit ? 0.0 : velocity.Y;
                    velocity = new Vector(vx, vy);
                    Debug.WriteLine($"## Add.Hit: Delta={easeSet.Delta:f2}, Rate={hit.Rate:f2}, V1={velocity:f2}");
                }
                else
                {
                    multiEaseSet.Add(easeSet);
                    Debug.WriteLine($"## Add.End: Delta={easeSet.Delta:f2}, Rate={1}, V1={easeSet.V1:f2}");
                    break;
                }
            }

            _container.Transform.SetPoint(_container.Transform.Point + multiEaseSet.Delta, TimeSpan.FromMilliseconds(multiEaseSet.Milliseconds), multiEaseSet.EaseX, multiEaseSet.EaseY);
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




}