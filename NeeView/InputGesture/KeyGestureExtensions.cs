using System.Windows.Input;

namespace NeeView
{
    public static class KeyGestureExtensions
    {
        public static string GetDisplayString(this KeyGesture gesture)
        {
            return new KeyGestureSource(gesture.Key, gesture.Modifiers).GetDisplayString();
        }
    }
}
