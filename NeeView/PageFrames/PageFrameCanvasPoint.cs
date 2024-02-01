using System.Collections.Generic;
using System.Windows;

namespace NeeView.PageFrames
{
    public class PageFrameCanvasPoint
    {
        public PageFrameCanvasPoint(LinkedListNode<PageFrameContainer> node, Vector pointRate)
        {
            Node = node;
            PointRate = pointRate;
        }

        public LinkedListNode<PageFrameContainer> Node { get; }
        public Vector PointRate { get; }

        public override string ToString()
        {
            return $"{Node.Value}, ({PointRate:f2})";
        }
    }


}
