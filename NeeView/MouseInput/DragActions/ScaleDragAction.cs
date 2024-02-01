using System;

namespace NeeView
{
    public class ScaleDragAction : DragAction
    {
        public ScaleDragAction()
        {
            Note = Properties.TextResources.GetString("DragActionType.Scale");
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ScaleActionControl(context, this, ScaleType.TransformScale);
        }
    }
}
