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

        public static void SetDisplayString(this MouseHorizontalWheelAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseHorizontalWheelAction action)
        {
            if (_map.TryGetValue(action, out var s))
            {
                return s;
            }

            return action.ToString();
        }
    }
}
