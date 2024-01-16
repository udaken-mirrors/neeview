using NeeView.Windows;
using System;
using System.Windows;

namespace NeeView
{
    public class ScaleSliderCenteredActionControl : NormalDragActionControl
    {
        private readonly DragTransform _transformControl;
        private readonly ScaleType _scaleType;

        public ScaleSliderCenteredActionControl(DragTransformContext context, DragAction source, ScaleType scaleType) : base(context, source)
        {
            _transformControl = new DragTransform(Context);
            _scaleType = scaleType;
        }

        public override void Execute()
        {
            DragScaleSliderCentered(Context.First, Context.Last, Parameter.Cast<SensitiveDragActionParameter>().Sensitivity, TimeSpan.Zero);
        }

        // 拡縮 (スライダー、中央寄せ)
        private void DragScaleSliderCentered(Point start, Point end, double sensitivity, TimeSpan span)
        {
            var startScale = Context.GetStartScale(_scaleType);
            var scale1 = System.Math.Pow(2, (end.X - start.X) * 0.01 * sensitivity) * startScale;
            _transformControl.DoScale(_scaleType, scale1, span, false);

            var len0 = Math.Abs(end.X - start.X);
            var len1 = 200.0;
            var rate = Math.Min(len0 / len1, 1.0);

            var v0 = Context.ContentCenter - Context.ScaleCenter;
            //var v1 = v0 * (Context.Transform.Scale / Context.BaseScale);
            var v1 = v0 * (scale1 / startScale);
            var v2 = (Vector)Context.ContentCenter;
            var delta = v1 - VectorExtensions.Lerp(v0, v2, rate);
            var pos = Context.StartPoint + delta;
            _transformControl.DoPoint(pos, span);
        }
    }

}
