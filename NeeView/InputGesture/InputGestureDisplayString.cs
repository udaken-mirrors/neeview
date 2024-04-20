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
                var name = pair.Key.AsSpan(prefix.Length);
                switch (name)
                {
                    case nameof(MouseWheelAction.WheelUp):
                    case nameof(MouseWheelAction.WheelDown):
                        {
                            var key = (MouseWheelAction)Enum.Parse(typeof(MouseWheelAction), name, true);
                            key.SetDisplayString(pair.Value.Text);
                        }
                        break;

                    case nameof(MouseHorizontalWheelAction.WheelLeft):
                    case nameof(MouseHorizontalWheelAction.WheelRight):
                        {
                            var key = (MouseHorizontalWheelAction)Enum.Parse(typeof(MouseHorizontalWheelAction), name, true);
                            key.SetDisplayString(pair.Value.Text);
                        }
                        break;

                    case nameof(MouseAction.LeftClick):
                    case nameof(MouseAction.RightClick):
                    case nameof(MouseAction.MiddleClick):
                    case nameof(MouseAction.WheelClick):
                    case nameof(MouseAction.LeftDoubleClick):
                    case nameof(MouseAction.RightDoubleClick):
                    case nameof(MouseAction.MiddleDoubleClick):
                        {
                            var key = (MouseAction)Enum.Parse(typeof(MouseAction), name, true);
                            key.SetDisplayString(pair.Value.Text);
                        }
                        { 
                            var key = (MouseExAction)Enum.Parse(typeof(MouseExAction), name, true);
                            key.SetDisplayString(pair.Value.Text);
                        }
                        break;

                    default:
                        {
                            var key = (MouseExAction)Enum.Parse(typeof(MouseExAction), name, true);
                            key.SetDisplayString(pair.Value.Text);
                        }
                        break;
                }
            }

            var converter = GetDisplayStringStringConverter(resource, prefix);
            MouseActionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
            MouseExActionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
            MouseWheelActionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
            MouseHorizontalWheelActionExtensions.SetDisplayStringConverter(GetDisplayStringStringConverter(resource, prefix));
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
