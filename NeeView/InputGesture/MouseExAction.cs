using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace NeeView
{
    // 拡張マウスアクション
    public enum MouseExAction
    {
        None,
        LeftClick,
        RightClick,
        MiddleClick,
        WheelClick,
        LeftDoubleClick,
        RightDoubleClick,
        MiddleDoubleClick,
        XButton1Click,
        XButton1DoubleClick,
        XButton2Click,
        XButton2DoubleClick,
    }


    public static class MouseExActionExtensions
    {
        private static readonly Dictionary<MouseExAction, string> _map = new();

        public static void SetDisplayString(this MouseExAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseExAction action)
        {
            if (_map.TryGetValue(action, out var s))
            {
                return s;
            }

            return action.ToString();
        }
    }
}
