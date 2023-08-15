using NeeView.Windows;
using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class ScaleSliderCenteredDragAction : DragAction
    {
        public ScaleSliderCenteredDragAction()
        {
            Note = Properties.Resources.DragActionType_ScaleSliderCentered;
            //DragKey = new DragKey("Shift+RightButton");
            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }


        private class ActionControl : DragActionControl
        {
            private DragTransform _transformControl;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _transformControl = new DragTransform(context);
            }

            public override void Execute()
            {
                DragScaleSliderCentered(Context.First, Context.Last, Parameter.Cast<SensitiveDragActionParameter>().Sensitivity, TimeSpan.Zero);
            }

            // 拡縮 (スライダー、中央寄せ)
            private void DragScaleSliderCentered(Point start, Point end, double sensitivity, TimeSpan span)
            {
                var scale1 = System.Math.Pow(2, (end.X - start.X) * 0.01 * sensitivity) * Context.BaseScale;
                _transformControl.DoScale(scale1, span, false);

                var len0 = Math.Abs(end.X - start.X);
                var len1 = 200.0;
                var rate = Math.Min(len0 / len1, 1.0);

                var v0 = Context.ContentCenter - Context.ScaleCenter;
                var v1 = v0 * (Context.Transform.Scale / Context.BaseScale);
                var v2 = (Vector)Context.ContentCenter;
                var delta = v1 - VectorExtensions.Lerp(v0, v2, rate);
                var pos = Context.BasePoint + delta;
                _transformControl.DoPoint(pos, span);
            }
        }
    }
}
