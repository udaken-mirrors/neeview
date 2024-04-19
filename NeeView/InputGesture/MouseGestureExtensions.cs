using System.Windows.Input;

namespace NeeView
{
    public static class MouseGestureExtensions
    {
        private const char _modifiersDelimiter = '+';

        public static string GetDisplayString(this MouseGesture gesture)
        {
            if (gesture.MouseAction == MouseAction.None) return "";

            string strBinding = "";
            string? strKey = gesture.MouseAction.GetDisplayString();
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
