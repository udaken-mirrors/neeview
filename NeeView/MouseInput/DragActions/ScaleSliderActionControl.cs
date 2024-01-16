using System;

namespace NeeView
{
    public class ScaleSliderActionControl : NormalDragActionControl
    {
        private readonly DragTransform _transformControl;
        private readonly SensitiveDragActionParameter _parameter;
        private readonly ScaleType _scaleType;

        public ScaleSliderActionControl(DragTransformContext context, DragAction source, ScaleType scaleType) : base(context, source)
        {
            _parameter = Parameter as SensitiveDragActionParameter ?? throw new ArgumentNullException(nameof(source));
            _transformControl = new DragTransform(Context);
            _scaleType = scaleType;
        }

        public override void Execute()
        {
            DragScaleSlider(_parameter.Sensitivity, TimeSpan.Zero);
        }

        // 拡縮 (スライダー)
        public void DragScaleSlider(double sensitivity, TimeSpan span)
        {
            var scale1 = Math.Pow(2, (Context.Last.X - Context.First.X) * 0.01 * sensitivity) * Context.GetStartScale(_scaleType);
            _transformControl.DoScale(_scaleType, scale1, span);
        }
    }
}