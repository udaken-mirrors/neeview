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

        public static void SetDisplayString(this MouseButton button, string value)
        {
            _map[button] = value;
        }

        public static string GetDisplayString(this MouseButton button)
        {
            if (_map.TryGetValue(button, out var s))
            {
                return s;
            }

            return button.ToString();
        }
    }

}
