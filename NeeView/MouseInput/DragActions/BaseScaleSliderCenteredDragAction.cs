using System;

namespace NeeView
{
    public class BaseScaleSliderCenteredDragAction : DragAction
    {
        public BaseScaleSliderCenteredDragAction()
        {
            Note = Properties.Resources.DragActionType_BaseScaleSliderCentered;
            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ScaleSliderCenteredActionControl(context, this, ScaleType.BaseScale);
        }
    }

}
