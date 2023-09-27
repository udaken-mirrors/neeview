using System;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class MoveDragAction : DragAction
    {
        public MoveDragAction()
        {
            Note = Properties.Resources.DragActionType_Move;
            DragKey = new DragKey("LeftButton");
            DragActionCategory = DragActionCategory.Point;
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
                var delta = Context.Last - Context.Old;
                _transformControl.DoMove(delta, TimeSpan.Zero);
            }

            public override void ExecuteEnd(bool continued)
            {
                if (continued) return;

                var inertia = Context.Speedometer.GetInertia();
                _transformControl.DoMove(inertia.Delta, inertia.Span);
            }
        }
    }


}