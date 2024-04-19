using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    public static class MouseActionExtensions
    {
        private static readonly Dictionary<MouseAction, string> _map = new();

        public static void SetDisplayString(this MouseAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseAction action)
        {
            if (_map.TryGetValue(action, out var s))
            {
                return s;
            }

            return action.ToString();
        }
    }
}
