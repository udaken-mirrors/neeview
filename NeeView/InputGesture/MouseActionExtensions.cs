using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    public static class MouseActionExtensions
    {
        private static readonly Dictionary<MouseAction, string> _map = new();
        private static StringConverter _displayStringConverter = StringConverter.Default;

        public static void SetDisplayStringConverter(StringConverter converter)
        {
            _displayStringConverter = converter;
        }

        public static void SetDisplayString(this MouseAction action, string value)
        {
            _map[action] = value;
        }

        public static string GetDisplayString(this MouseAction action)
        {
            return _displayStringConverter.Convert(_map.TryGetValue(action, out var s) ? s : action.ToString());
        }
    }
}
