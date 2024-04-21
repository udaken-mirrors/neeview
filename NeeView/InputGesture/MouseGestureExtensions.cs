using System.Windows.Input;

namespace NeeView
{
    public static class MouseGestureExtensions
    {
        public static string GetDisplayString(this MouseGesture gesture)
        {
            return new MouseGestureSource(MouseActionExtensions.ConvertFrom(gesture.MouseAction), gesture.Modifiers, ModifierMouseButtons.None).GetDisplayString();
        }
    }
}
