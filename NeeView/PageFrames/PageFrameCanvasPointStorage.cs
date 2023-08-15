using System;
using System.Collections.Generic;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    public class PageFrameCanvasPointStorage
    {
        private PageFrameContainerCollection _containers;
        private ViewTransformContext _transform;


        public PageFrameCanvasPointStorage(PageFrameContainerCollection containers, ViewTransformContext transform)
        {
            _containers = containers;
            _transform = transform;
        }


        public PageFrameCanvasPoint? Store(LinkedListNode<PageFrameContainer>? node)
        {
            //var node = _rectMath.GetViewCenterContainer(_viewBox.Rect);
            if (node is null) return null;
            if (node.Value.Rect.LessThanZero()) return null;

            var v0 = _transform.Point;
            var v1 = CanvasToViewPoint(node.Value.Rect.Center());
            //Debug.WriteLine($"({v0:f0}), ({v1:f0})");

            var v = v0 - v1;
            var xrate = v.X / node.Value.Rect.Width;
            var yrate = v.Y / node.Value.Rect.Height;

            var pointRate = new Vector(xrate, yrate);

            return new PageFrameCanvasPoint(node, pointRate);
        }

        public void Restore(PageFrameCanvasPoint? pos)
        {
            if (pos is null) return;

            if (!_containers.ContainsNode(pos.Node)) return;

            var v = new Vector(pos.Node.Value.Rect.Width * pos.PointRate.X, pos.Node.Value.Rect.Height * pos.PointRate.Y);
            var point = CanvasToViewPoint(pos.Node.Value.Rect.Center()) + v;

            _transform.SetPoint(point, TimeSpan.Zero);
        }

        private Point CanvasToViewPoint(Point point)
        {
            return new Point(-point.X, -point.Y);
        }
    }


}