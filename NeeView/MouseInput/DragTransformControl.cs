using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using NeeView.PageFrames;

namespace NeeView
{

    public class DragTransformControl : IDragTransformControl
    {
        private ViewConfig _viewConfig;

        private DragActionProxy _action = new DragActionProxy();

        private bool _isMouseButtonDown;

        private DragActionTable _dragActionTable;
        private DragActionFactory _dragActionFactory;
        private IDragTransformContextFactory _transformContextFactory;


        
        public DragTransformControl(IDragTransformContextFactory transformContextFactory, DragActionTable dragActionTable, ViewConfig viewConfig)
        {
            _dragActionTable = dragActionTable;
            _viewConfig = viewConfig;
            _transformContextFactory = transformContextFactory;

            _dragActionFactory = new DragActionFactory(_dragActionTable, _transformContextFactory);
        }


        public void ResetState()
        {
            _isMouseButtonDown = false;
            _action.SetAction(null);
        }

        /// <summary>
        /// Change State
        /// </summary>
        /// <param name="buttons">マウスボタンの状態</param>
        /// <param name="keys">装飾キーの状態</param>
        /// <param name="pos">マウス座標</param>
        public void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp)
        {
            if (_isMouseButtonDown)
            {
                StateDrag(buttons, keys, point, timestamp);
            }
            else
            {
                StateIdle(buttons, keys, point, timestamp);
            }
        }

        public void MouseWheel(MouseButtonBits buttons, ModifierKeys keys, MouseWheelEventArgs e)
        {
            _action.MouseWheel(e);
        }


        private void StateIdle(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp)
        {
            if (buttons != MouseButtonBits.None)
            {
                _isMouseButtonDown = true;
                StateDrag(buttons, keys, point, timestamp);
            }
        }

        private void StateDrag(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp)
        {
            if (buttons == MouseButtonBits.None)
            {
                _action.ExecuteEnd(point, timestamp, false);
                _action.SetAction(null);
                _isMouseButtonDown = false;
                return;
            }

            // change action
            var dragKey = new DragKey(buttons, keys);
            if (!_action.DragKeyEquals(dragKey))
            {
                var action = _dragActionFactory.Create(dragKey);
                if (action is not null)
                {
                    _action.ExecuteEnd(point, timestamp, true);
                    _action.SetAction(action);
                    _action.ExecuteBegin(point, timestamp);
                }
            }

            // exec action
            _action.Execute(point, timestamp);
        }

    }
}