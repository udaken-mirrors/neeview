using System;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// Viewのスクロール操作
    /// あれ、これいる？
    /// </summary>
    public class ViewTransformContext : IPointControl
    {
        private PageFrameContext _context;
        private PageFrameContainersViewBox _viewBox;
        private PageFrameContainersCollectionRectMath _rectMath;
        private PageFrameScrollViewer _scrollViewer;


        public ViewTransformContext(PageFrameContext context, PageFrameContainersViewBox viewBox, PageFrameContainersCollectionRectMath rectMath, PageFrameScrollViewer scrollViewer)
        {
            _context = context;
            _viewBox = viewBox;
            _rectMath = rectMath;
            _scrollViewer = scrollViewer;
        }


        public Point Point => _scrollViewer.Point;

        public Rect ViewRect => _viewBox.Rect;
        public Rect CanvasRect => _rectMath.GetContainersRect(_viewBox.Rect);


        public void SetPoint(Point value, TimeSpan span)
        {
            AddPoint(value - Point, span);
        }


        public void AddPoint(Vector value, TimeSpan span)
        {
            _scrollViewer.AddPoint(value, span);
        }
    }

}