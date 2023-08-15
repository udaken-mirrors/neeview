using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class LoupeDragAction : DragAction
    {
        public LoupeDragAction()
        {
            DragKey = new DragKey("RightButton");
            DragActionCategory = DragActionCategory.None;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            Debug.Assert(context is LoupeDragTransformContext);
            return new ActionControl(context, this);
        }


        private class ActionControl : DragActionControl
        {
            private LoupeDragTransformContext _context;

            public ActionControl(DragTransformContext context, DragAction source) : base(context, source)
            {
                _context = context as LoupeDragTransformContext ?? throw new ArgumentException("need LoupeDragTransformContext", nameof(context));
            }

            public override void ExecuteBegin()
            {
            }

            public override void Execute()
            {
                DoLoupe(_context.LoupeScale, TimeSpan.Zero);
            }

            public override void ExecuteEnd(bool continued)
            {
                _context.Transform.SetPoint(new Point(0.0, 0.0), TimeSpan.Zero);
                _context.Transform.SetScale(1.0, TimeSpan.Zero);
            }

            public override void MouseWheel(MouseWheelEventArgs e)
            {
                // TODO: Delta量に応じた回数
                var scale = e.Delta > 0 ? ZoomIn() : ZoomOut();
                DoLoupe(scale, TimeSpan.Zero);
                e.Handled = true;
            }

            private double ZoomIn()
            {
                return Math.Min(_context.LoupeScale + _context.Loupe.ScaleStep, _context.Loupe.MaximumScale);
            }

            private double ZoomOut()
            {
                return Math.Max(_context.LoupeScale - _context.Loupe.ScaleStep, _context.Loupe.MinimumScale);
            }

            private void DoLoupe(double scale, TimeSpan span)
            {
                var point = _context.LoupeBasePoint - (_context.Last - _context.First) * _context.LoupeSpeed;
                _context.Transform.SetPoint(point, span);

                _context.LoupeScale = scale;
                _context.Transform.SetScale(_context.LoupeScale, span);
            }
        }
    }
}