using System.Collections.Generic;

namespace NeeView
{
    // ホイールアクション
    public enum MouseWheelAction
    {
        None,
        WheelUp,
        WheelDown,
    }

    public static class MouseWheelActionExtensions
    {
        private static readonly Dictionary<MouseWheelAction, string> _map = new();

        public static void SetDisplayString(this MouseWheelAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseWheelAction action)
        {
            if (_map.TryGetValue(action, out var s))
            {
                return s;
            }

            return action.ToString();
        }
    }
}
