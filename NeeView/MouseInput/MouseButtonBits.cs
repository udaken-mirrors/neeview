using System;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// MouseButton Bitmask
    /// </summary>
    [Flags]
    public enum MouseButtonBits
    {
        None = 0,
        LeftButton = (1 << MouseButton.Left),
        MiddleButton = (1 << MouseButton.Middle),
        RightButton = (1 << MouseButton.Right),
        XButton1 = (1 << MouseButton.XButton1),
        XButton2 = (1 << MouseButton.XButton2),
        All = LeftButton | MiddleButton | RightButton | XButton1 | XButton2,
    }

    public static class MouseButtonBitsExtensions
    {
        /// <summary>
        /// なにか押されている
        /// </summary>
        public static bool Any(this MouseButtonBits bits)
        {
            return bits != MouseButtonBits.None;
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <param name="e">元になるデータ</param>
        /// <returns></returns>
        public static MouseButtonBits Create(MouseEventArgs e)
        {
            var bits = MouseButtonBits.None;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.LeftButton;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.MiddleButton;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.RightButton;
            }
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton1;
            }
            if (e.XButton2 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton2;
            }

            return bits;
        }

        /// <summary>
        /// 押されているマウスボタンのビットマスク作成
        /// </summary>
        /// <returns></returns>
        public static MouseButtonBits Create()
        {
            var bits = MouseButtonBits.None;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.LeftButton;
            }
            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.MiddleButton;
            }
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.RightButton;
            }
            if (Mouse.XButton1 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton1;
            }
            if (Mouse.XButton2 == MouseButtonState.Pressed)
            {
                bits |= MouseButtonBits.XButton2;
            }

            return bits;
        }
    }

}
