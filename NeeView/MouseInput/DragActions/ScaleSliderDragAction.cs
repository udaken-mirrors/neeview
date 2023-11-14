using System;
using System.Diagnostics;

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
            return new ScaleSliderActionControl(context, this, ScaleType.TransformScale);
        }
    }
}