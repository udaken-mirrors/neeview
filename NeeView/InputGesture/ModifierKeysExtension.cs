using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace NeeView
{
    public static class ModifierKeysExtension
    {
        private const char _modifierDelimiter = '+';

        private static readonly Dictionary<ModifierKeys, string> _map = new()
        {
            [ModifierKeys.Control] = "Ctrl",
        };


        public static void SetDisplayString(this ModifierKeys modifiers, string value)
        {
            _map[modifiers] = value;
        }

        public static string GetDisplayString(this ModifierKeys modifiers)
        {
            if (!ModifierKeysConverter.IsDefinedModifierKeys(modifiers))
            {
                throw new InvalidEnumArgumentException(nameof(modifiers), (int)modifiers, typeof(ModifierKeys));
            }

            string strModifiers = "";

            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                strModifiers += MatchModifiers(ModifierKeys.Control);
            }

            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierKeys.Alt);
            }

            if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierKeys.Windows);
            }

            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierKeys.Shift);
            }

            return strModifiers;
        }

        private static string MatchModifiers(ModifierKeys modifierKeys)
        {
            if (_map.TryGetValue(modifierKeys, out var s))
            {
                return s;
            }
            return modifierKeys.ToString();
        }
    }

}
