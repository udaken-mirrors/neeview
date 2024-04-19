using System.Windows.Input;

namespace NeeView
{
    public static class KeyGestureExtensions
    {
        private const char _modifiersDelimiter = '+';

        public static string GetDisplayString(this KeyGesture gesture)
        {
            if (gesture.Key == Key.None) return "";

            string strBinding = "";
            string? strKey = gesture.Key.GetDisplayString();
            if (strKey != string.Empty)
            {
                strBinding += gesture.Modifiers.GetDisplayString();
                if (strBinding != string.Empty)
                {
                    strBinding += _modifiersDelimiter;
                }
                strBinding += strKey;
            }
            return strBinding;
        }
    }
}
