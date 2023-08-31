using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// タッチルーペ
    /// </summary>
    public class TouchInputLoupe : TouchInputBase
    {
        private readonly LoupeContext _loupe;
        private TouchContext? _touch;
        private TouchDragContext? _origin;
        private double _originScale;
        private LoupeDragTransformContext? _transformContext;


        public TouchInputLoupe(TouchInputContext context) : base(context)
        {
            if (context.Loupe is null) throw new InvalidOperationException();
            _loupe = context.Loupe;
        }


        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter">TouchContext</param>
        public override void OnOpened(FrameworkElement sender, object? parameter)
        {
            _transformContext = _context.DragTransformContextFactory?.CreateDragTransformContext(false, true) as LoupeDragTransformContext;
            if (_transformContext is null) throw new NotImplementedException(); // TODO: モード拒否

            _transformContext.AttachLoupeContext(_loupe);

            sender.Focus();
            sender.Cursor = Cursors.None;

            _touch = parameter as TouchContext ?? throw new InvalidOperationException("parameter must be TouchContext");

            _loupe.IsEnabled = true;

            if (Config.Current.Loupe.IsResetByRestart)
            {
                Config.Current.Loupe.LoupeScale = Config.Current.Loupe.DefaultScale;
            }
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        public override void OnClosed(FrameworkElement sender)
        {
            sender.Cursor = null;

            _loupe.IsEnabled = false;
        }

        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            _origin = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);
            _originScale = _loupe.Scale;
        }

        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            if (e.Handled) return;
            if (_touch is null) return;

            if (!_context.TouchMap.ContainsKey(_touch.StylusDevice))
            {
                ResetState();
            }
        }

        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            if (e.Handled) return;
            if (_touch is null) return;
            if (_transformContext is null) return;

            if (e.StylusDevice == _touch.StylusDevice)
            {
                var point = ToDragCoord(e.GetPosition(_context.Sender));

                _transformContext.Update(point, e.Timestamp);
                _transformContext.UpdateSpeed(point, e.Timestamp);
                _transformContext.Update();
            }

            if (_origin is not null && _context.TouchMap.Count >= 2)
            {
                var current = new TouchDragContext(_context.Sender, _context.TouchMap.Keys);

                var scale = current.Radius / _origin.Radius;
                _loupe.Scale = _originScale * scale;
            }

            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Config.Current.Loupe.IsWheelScalingEnabled)
            {
                if (e.Delta > 0)
                {
                    _loupe.ZoomIn();
                }
                else
                {
                    _loupe.ZoomOut();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        public override void OnKeyDown(object sender, KeyEventArgs e)
        {
            // ESC で 状態解除
            if (Config.Current.Loupe.IsEscapeKeyEnabled && e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                ResetState();
                e.Handled = true;
            }
        }

    }
}
