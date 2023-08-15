using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography.Pkcs;
using System.Windows.Documents;
using System.Windows.Media;

namespace NeeView
{
    public class ScaleSliderDragAction : DragAction
    {
        public ScaleSliderDragAction()
        {
            Note = Properties.Resources.DragActionType_ScaleSlider;
            DragKey = new DragKey("Ctrl+LeftButton");

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
            private SensitiveDragActionParameter _parameter;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _parameter = Parameter as SensitiveDragActionParameter ?? throw new ArgumentNullException(nameof(source));
                _transformControl = new DragTransform(context);
            }

            public override void Execute()
            {
                DragScaleSlider(_parameter.Sensitivity, TimeSpan.Zero);
            }

            // 拡縮 (スライダー)
            public void DragScaleSlider(double sensitivity, TimeSpan span)
            {
                var scale1 = Math.Pow(2, (Context.Last.X - Context.First.X) * 0.01 * sensitivity) * Context.BaseScale;
                _transformControl.DoScale(scale1, span);
            }
        }
    }
}