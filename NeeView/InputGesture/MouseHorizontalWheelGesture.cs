using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウス水平ホイールアクション
    /// </summary>
    public class MouseHorizontalWheelGesture : InputGesture
    {
        // マウスホイールアクション
        public MouseHorizontalWheelAction WheelAction { get; private set; }

        // 修飾キー
        public ModifierKeys Modifiers { get; private set; }

        // 修飾マウスボタン
        public ModifierMouseButtons ModifierButtons { get; private set; }

        // コンストラクタ
        public MouseHorizontalWheelGesture(MouseHorizontalWheelAction wheelAction, ModifierKeys modifiers, ModifierMouseButtons modifierButtons)
        {
            this.WheelAction = wheelAction;
            this.Modifiers = modifiers;
            this.ModifierButtons = modifierButtons;
        }

        // 入力判定
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is not MouseWheelEventArgs mouseEventArgs) return false;

            MouseHorizontalWheelAction wheelAction = MouseHorizontalWheelAction.None;
            if (mouseEventArgs.Delta > 0)
            {
                wheelAction = MouseHorizontalWheelAction.WheelRight;
            }
            else if (mouseEventArgs.Delta < 0)
            {
                wheelAction = MouseHorizontalWheelAction.WheelLeft;
            }
            //System.Diagnostics.Debug.WriteLine($"HorizontalWheel: {mouseEventArgs.Delta}");

            ModifierMouseButtons modifierMouseButtons = ModifierMouseButtons.None;
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.LeftButton;
            if (mouseEventArgs.RightButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.RightButton;
            if (mouseEventArgs.MiddleButton == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.MiddleButton;
            if (mouseEventArgs.XButton1 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton1;
            if (mouseEventArgs.XButton2 == MouseButtonState.Pressed)
                modifierMouseButtons |= ModifierMouseButtons.XButton2;

            return this.WheelAction == wheelAction && Modifiers == Keyboard.Modifiers && ModifierButtons == modifierMouseButtons;
        }


        public string GetDisplayString()
        {
            return new MouseGestureSource(MouseActionExtensions.ConvertFrom(WheelAction), Modifiers, ModifierButtons).GetDisplayString();
        }
    }

}
