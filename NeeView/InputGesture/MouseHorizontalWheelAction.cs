using System.Collections.Generic;

namespace NeeView
{
    // 水平ホイールアクション
    public enum MouseHorizontalWheelAction
    {
        None,
        WheelLeft,
        WheelRight,
    }


    public static class MouseHorizontalWheelActionExtensions
    {
        private static readonly Dictionary<MouseHorizontalWheelAction, string> _map = new();
        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this MouseHorizontalWheelAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseHorizontalWheelAction action)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(action, out var s) ? s : action.ToString());
        }
    }
}
