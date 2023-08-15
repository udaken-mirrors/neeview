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

                // TODO: Span どこから？
                var span = TimeSpan.FromMilliseconds(500);
                // TODO: 距離倍率 0.5 を再検討
                var delta = Context.Speed * span.TotalMilliseconds * 0.5;
                _transformControl.DoMove(delta, span);

            }
        }
    }


}