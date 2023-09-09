using System;
using System.Windows;
using NeeView.Maths;

namespace NeeView.PageFrames
{
    public class ViewTransformControl : ITransformControl
    {
        private readonly PageFrameContext _context;
        private readonly ViewTransformContext _viewContext;
        private readonly PageFrameContainer _container;
        private readonly ScrollLock _scrollLock;

        public ViewTransformControl(PageFrameContext context, PageFrameContainer container, ViewTransformContext viewContext, ScrollLock scrollLock)
        {
            _context = context;
            _viewContext = viewContext;
            _container = container;
            _scrollLock = scrollLock;
        }

        public double Scale => _container.Transform.Scale;
        public double Angle => _container.Transform.Angle;
        public Point Point => _viewContext.Point;
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
            _context.IsSnapAnchor.Reset();
            _viewContext.SetPoint(value, span);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            var canvasRect = _viewContext.CanvasRect;

            // scroll lock
            _scrollLock.Update(canvasRect, _viewContext.ViewRect);
            var delta = _scrollLock.Limit(value);

            // scroll area limit
            var areaLimit = new ScrollAreaLimit(canvasRect, _viewContext.ViewRect);
            delta = areaLimit.GetLimitContentMove(delta);

            _context.IsSnapAnchor.Reset();
            _viewContext.AddPoint(delta, span);
        }

        public void SnapView()
        {
            //throw new NotImplementedException();
        }
    }

}