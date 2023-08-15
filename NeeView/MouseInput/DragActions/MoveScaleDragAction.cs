using System;
using System.Windows;

namespace NeeView
{
    public class MoveScaleDragAction : DragAction
    {
        public MoveScaleDragAction()
        {
            Note = Properties.Resources.DragActionType_MoveScale;
            DragActionCategory = DragActionCategory.Point;

            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ActionControl(context, this);
        }

        public class ActionControl : DragActionControl
        {
            private DragTransform _transformControl;
            private SensitiveDragActionParameter _parameter;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _transformControl = new DragTransform(context);
                _parameter = Parameter as SensitiveDragActionParameter ?? throw new ArgumentNullException(nameof(source));
            }

            // 移動(速度スケール依存)
            public override void Execute()
            {
                var delta = Context.Last - Context.Old;
                var scale = GetScale();
                _transformControl.DoMove(delta * scale, TimeSpan.Zero);
            }

            public override void ExecuteEnd(bool continued)
            {
                if (continued) return;

                var span = TimeSpan.FromMilliseconds(500);
                var delta = Context.Speed * span.TotalMilliseconds * 0.5;
                var scale = GetScale();
                _transformControl.DoMove(delta * scale, span);
            }

            private double GetScale()
            {
                // TODO: 連結モードでは３倍固定にしたい
                var scaleX = Context.ContentRect.Width / Context.ViewRect.Width;
                var scaleY = Context.ContentRect.Height / Context.ViewRect.Height;
                var scale = (scaleX > scaleY ? scaleX : scaleY) * _parameter.Sensitivity;
                scale = scale < 1.0 ? 1.0 : scale;
                return scale;
            }

        }
    }


}