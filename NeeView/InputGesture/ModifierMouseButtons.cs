using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    // 修飾マウスボタン
    [Flags]
    public enum ModifierMouseButtons
    {
        None = 0,
        LeftButton = (1 << 0),
        MiddleButton = (1 << 1),
        RightButton = (1 << 2),
        XButton1 = (1 << 3),
        XButton2 = (1 << 4),
    }


    public static class ModifierMouseButtonsExtensions 
    {
        private const char _modifierDelimiter = '+';

        private static readonly Dictionary<ModifierMouseButtons, string> _map = new();


        public static void SetDisplayString(this ModifierMouseButtons modifiers, string value)
        {
            _map[modifiers] = value;
        }

        public static string GetDisplayString(this ModifierMouseButtons modifiers)
        {
            if (!ModifierMouseButtonsConverter.IsDefinedModifierMouseButtons(modifiers))
            {
                throw new InvalidEnumArgumentException(nameof(modifiers), (int)modifiers, typeof(ModifierMouseButtons));
            }

            string strModifiers = "";

            if ((modifiers & ModifierMouseButtons.LeftButton) == ModifierMouseButtons.LeftButton)
            {
                strModifiers += MatchModifiers(ModifierMouseButtons.LeftButton);
            }

            if ((modifiers & ModifierMouseButtons.MiddleButton) == ModifierMouseButtons.MiddleButton)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierMouseButtons.MiddleButton);
            }

            if ((modifiers & ModifierMouseButtons.RightButton) == ModifierMouseButtons.RightButton)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierMouseButtons.RightButton);
            }

            if ((modifiers & ModifierMouseButtons.XButton1) == ModifierMouseButtons.XButton1)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierMouseButtons.XButton1);
            }

            if ((modifiers & ModifierMouseButtons.XButton2) == ModifierMouseButtons.XButton2)
            {
                if (strModifiers.Length > 0) strModifiers += _modifierDelimiter;
                strModifiers += MatchModifiers(ModifierMouseButtons.XButton2);
            }

            return strModifiers;
        }

        private static string MatchModifiers(ModifierMouseButtons modifiers)
        {
            if (_map.TryGetValue(modifiers, out var s))
            {
                return s;
            }
            return modifiers.ToString();
        }
    }
}
