using System;
using System.Windows;
using System.Windows.Media.Animation;
using NeeLaboratory.ComponentModel;
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
            _container.Transform.SetPoint(_container.Transform.Point + delta, span, easeX, easeY);
        }

        public void InertiaPoint(Vector velocity)
        {
            _context.IsSnapAnchor.Reset();
            var inertiaEaseFactory = new InertiaEaseFactory(GetScrollLockHit, GetAreaLimitHit);
            var multiEaseSet = inertiaEaseFactory.Create(_container.Transform.Point, velocity);
            if (!multiEaseSet.IsValid) return;
            _container.Transform.AddPoint(multiEaseSet.Delta, TimeSpan.FromMilliseconds(multiEaseSet.Milliseconds), multiEaseSet.EaseX, multiEaseSet.EaseY);
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