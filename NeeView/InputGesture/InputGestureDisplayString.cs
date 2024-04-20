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

        private static void InitializeKeyMap(TextResourceManager resource)
        {
            var prefix = nameof(Key) + ".";
            foreach (var pair in resource.Map.Where(e => e.Key.StartsWith(prefix)))
            {
                var key = (Key)Enum.Parse(typeof(Key), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }
        }

        private static void InitializeModifierKeysMap(TextResourceManager resource)
        {
            var prefix = nameof(ModifierKeys) + ".";
            foreach (var pair in resource.Map.Where(e => e.Key.StartsWith(prefix)))
            {
                var key = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }
        }

        private static void InitializeMouseButtonMap(TextResourceManager resource)
        {
            var prefix = nameof(MouseButton) + ".";
            foreach (var pair in resource.Map.Where(e => e.Key.StartsWith(prefix)))
            {
                var key = (MouseButton)Enum.Parse(typeof(MouseButton), pair.Key.AsSpan(prefix.Length), true);
                key.SetDisplayString(pair.Value.Text);
            }
        }

            private static void InitializeMouseActionMap(TextResourceManager resource)
        {
            var prefix = nameof(MouseAction) + ".";
            foreach (var pair in resource.Map.Where(e => e.Key.StartsWith(prefix)))
            {
                var name = pair.Key.AsSpan(prefix.Length);
                switch(name)
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

                    default:
                        {
                            var key = (MouseExAction)Enum.Parse(typeof(MouseExAction), name, true);
                            key.SetDisplayString(pair.Value.Text);
                        }
                        break;
                }
            }
        }


        public static string GetDisplayString(InputGesture gesture)
        {
            return InputGestureConverter.GetDisplayString(gesture);
        }
    }

}
