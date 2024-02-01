using System;

namespace NeeView
{
    public class BaseScaleDragAction : DragAction
    {
        public BaseScaleDragAction()
        {
            Note = Properties.TextResources.GetString("DragActionType.BaseScale");
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ScaleActionControl(context, this, ScaleType.BaseScale);
        }
    }
}
