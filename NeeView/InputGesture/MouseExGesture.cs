using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// 拡張マウスアクション
    /// 拡張ボタン対応
    /// </summary>
    public class MouseExGesture : InputGesture
    {
        private const char _modifiersDelimiter = '+';

        // メインアクション
        public MouseExAction MouseExAction { get; private set; }

        // 修飾キー
        public ModifierKeys ModifierKeys { get; private set; }

        // 修飾マウスボタン
        public ModifierMouseButtons ModifierMouseButtons { get; private set; }

        // コンストラクタ
        public MouseExGesture(MouseExAction action, ModifierKeys modifierKeys, ModifierMouseButtons modifierMouseButtons)
        {
            this.MouseExAction = action;
            this.ModifierKeys = modifierKeys;
            this.ModifierMouseButtons = modifierMouseButtons;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is not MouseButtonEventArgs mouseEventArgs) return false;

            MouseExAction action = MouseExAction.None;

            switch (mouseEventArgs.ChangedButton)
            {
                case MouseButton.Left:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.LeftDoubleClick : MouseExAction.LeftClick;
                    break;

                case MouseButton.Right:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.RightDoubleClick : MouseExAction.RightClick;
                    break;

                case MouseButton.Middle:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.MiddleDoubleClick : MouseExAction.MiddleClick;
                    break;

                case MouseButton.XButton1:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.XButton1DoubleClick : MouseExAction.XButton1Click;
                    break;

                case MouseButton.XButton2:
                    action = mouseEventArgs.ClickCount >= 2 ? MouseExAction.XButton2DoubleClick : MouseExAction.XButton2Click;
                    break;
            }

            if (action == MouseExAction.None) return false;

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Left)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Right)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.Middle)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.XButton1)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed && mouseEventArgs.ChangedButton != MouseButton.XButton2)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.MouseExAction == action && this.ModifierMouseButtons == modifierMouseButtons && ModifierKeys == Keyboard.Modifiers;
        }


        public string GetDisplayString()
        {
            if (MouseExAction == MouseExAction.None) return "";

            string strBinding = "";
            string? strKey = MouseExAction.GetDisplayString();
            if (strKey != string.Empty)
            {
                strBinding += ModifierKeys.GetDisplayString();
                if (strBinding != string.Empty)
                {
                    strBinding += _modifiersDelimiter;
                }

                var buttons = ModifierMouseButtons.GetDisplayString();
                if (buttons != string.Empty)
                {
                    strBinding += buttons;
                    strBinding += _modifiersDelimiter;
                }

                strBinding += strKey;
            }
            return strBinding;
        }
    }
}
