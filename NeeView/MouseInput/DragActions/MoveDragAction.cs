using NeeView.PageFrames;
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

            ParameterSource = new DragActionParameterSource(typeof(MoveDragActionParameter));
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }

        private class ActionControl : DragActionControl
        {
            private readonly DragTransform _transformControl;
            private readonly MoveDragActionParameter _parameter;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _transformControl = new DragTransform(context);
                _parameter = Parameter as MoveDragActionParameter ?? throw new ArgumentNullException(nameof(source));
            }

            public override void ExecuteBegin()
            {
                _transformControl.ResetInertia();
            }

            public override void Execute()
            {
                var delta = Context.Last - Context.Old;
                _transformControl.DoMove(delta, TimeSpan.Zero);
            }

            public override void ExecuteEnd(ISpeedometer? speedometer, bool continued)
            {
                Debug.Assert(speedometer is not null);
                if (continued) return;
                if (!_parameter.IsInertiaEnabled) return;

                _transformControl.DoInertia(speedometer.GetVelocity(), InertiaTools.GetAcceleration(Config.Current.Mouse.InertiaSensitivity));
            }
        }
    }


}