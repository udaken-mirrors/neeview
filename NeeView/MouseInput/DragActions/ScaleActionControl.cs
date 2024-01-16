using System;
using System.Windows;

namespace NeeView
{
    public class ScaleActionControl : NormalDragActionControl
    {
        private readonly DragTransform _transformControl;
        private readonly ScaleType _scaleType;

        public ScaleActionControl(DragTransformContext context, DragAction source, ScaleType scaleType) : base(context, source)
        {
            _transformControl = new DragTransform(Context);
            _scaleType = scaleType;
        }

        public override void Execute()
        {
            DragScale(Context.First, Context.Last, TimeSpan.Zero);
        }

        private void DragScale(Point start, Point end, TimeSpan span)
        {
            var v0 = start - Context.ScaleCenter;
            var v1 = end - Context.ScaleCenter;

            // 拡縮の基準となるベクトルが得られるまで処理を進めない
            const double minLength = 32.0;
            if (v0.Length < minLength)
            {
                Context.First = Context.Last;
                return;
            }

            var scale1 = v1.Length / v0.Length * Context.GetStartScale(_scaleType);
            _transformControl.DoScale(_scaleType, scale1, span);
        }
    }
}
