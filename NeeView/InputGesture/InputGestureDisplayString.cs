using NeeLaboratory.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NeeView
{
    public static class InputGestureDisplayString
    {
        public static void Initialize(TextResourceManager resource)
        {
            InitializeKeyMap(resource);
            InitializeModifierKeysMap(resource);
            InitializeMouseButtonMap(resource);
            InitializeMouseActionMap(resource);
            InitializeTouchAreaMap(resource);
        }

        private static StringConverter GetDisplayStringStringConverter(TextResourceManager resource, string prefix)
        {
            return new InputGestureStringConverter(resource.GetString(prefix + "_Uppercase")?.ToUpper() == "TRUE");
        }

        private static IEnumerable<KeyValuePair<string, TextResourceItem>> CollectTextItems(TextResourceManager resource, string prefix)
        {
            return resource.Map.Where(e => e.Key.StartsWith(prefix) && e.Key.Length > prefix.Length && e.Key[prefix.Length] != '_');
        }

        private static void InitializeKeyMap(TextResourceManager resource)
        {
            var prefix = nameof(Key) + ".";
            foreach (var pair in CollectTextItems(resource, prefix))
            {
                var key = (Key)Enum.Parse(typeof(Key), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }

            KeyExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
        }

        private static void InitializeModifierKeysMap(TextResourceManager resource)
        {
            var prefix = nameof(ModifierKeys) + ".";
            foreach (var pair in CollectTextItems(resource, prefix))
            {
                var key = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }

            ModifierKeysExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
        }

        private static void InitializeMouseButtonMap(TextResourceManager resource)
        {
            var prefix = nameof(MouseButton) + ".";
            foreach (var pair in CollectTextItems(resource, prefix))
            {
                var key = (MouseButton)Enum.Parse(typeof(MouseButton), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }

            MouseButtonExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
        }

        private static void InitializeMouseActionMap(TextResourceManager resource)
        {
            var prefix = nameof(MouseAction) + ".";
            foreach (var pair in CollectTextItems(resource, prefix))
            {
                var key = (MouseAction)Enum.Parse(typeof(MouseAction), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }

            MouseActionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
        }

        private static void InitializeTouchAreaMap(TextResourceManager resource)
        {
            var prefix = nameof(TouchArea) + ".";
            foreach (var pair in CollectTextItems(resource, prefix))
            {
                var key = (TouchArea)Enum.Parse(typeof(TouchArea), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }

            TouchAreaExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
        }

        public static string GetDisplayString(InputGesture gesture)
        {
            return InputGestureConverter.GetDisplayString(gesture);
        }
    }


    public class InputGestureStringConverter : StringConverter
    {
        private readonly bool _isUppercase;

        public InputGestureStringConverter(bool isUppercase)
        {
            _isUppercase = isUppercase;
        }

        public override string Convert(string value)
        {
            return _isUppercase ? value.ToUpper() : value;
        }
    }
}
