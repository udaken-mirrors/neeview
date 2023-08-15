using System;
using System.Windows;

namespace NeeView
{
    public class ScaleDragAction : DragAction
    {
        public ScaleDragAction()
        {
            Note = Properties.Resources.DragActionType_Scale;
            //DragKey = new DragKey("Shift+RightButton");
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

                var scale1 = v1.Length / v0.Length * Context.BaseScale;
                _transformControl.DoScale(scale1, span);
            }

        }
    }
}
