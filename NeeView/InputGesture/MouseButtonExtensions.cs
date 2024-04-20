using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    public static class MouseButtonExtensions
    {
        private static readonly Dictionary<MouseButton, string> _map = new()
        {
            [MouseButton.Left] = "LeftButton",
            [MouseButton.Middle] = "MiddleButton",
            [MouseButton.Right] = "RightButton",
        };

        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this MouseButton button, string value)
        {
            _map[button] = value;
        }

        public static string GetDisplayString(this MouseButton button)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(button, out var s) ? s : button.ToString());
        }
    }

}
