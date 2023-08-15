using System;
using System.Windows;
using System.Windows.Media;
using NeeView.Maths;

namespace NeeView
{
    public class AngleDragAction : DragAction
    {
        public AngleDragAction()
        {
            Note = Properties.Resources.DragActionType_Angle;
            DragKey = new DragKey("Shift+LeftButton");
            DragActionCategory = DragActionCategory.Angle;
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
                DragAngle();
            }


            public void DragAngle()
            {
                var v0 = Context.First - Context.RotateCenter;
                var v1 = Context.Last - Context.RotateCenter;

                // 回転の基準となるベクトルが得られるまで処理を進めない
                const double minLength = 10.0;
                if (v0.Length < minLength)
                {
                    Context.First = Context.Last;
                    return;
                }

                double angle = TransformMath.NormalizeLoopRange(Context.BaseAngle + Vector.AngleBetween(v0, v1), -180, 180);

                _transformControl.DoRotate(angle, TimeSpan.Zero);
            }
        }
    }
}