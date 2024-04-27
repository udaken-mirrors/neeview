using System.Windows.Input;

namespace NeeView
{
    public static class CommandTools
    {
        /// <summary>
        /// コマンドの ToolTip 文字列を作成
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        public static string CreateToolTipText(string name, Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            var text = ResourceService.GetString(name);
            var gesture = new KeyGestureSource(key, modifiers)?.GetDisplayString();
            return string.IsNullOrEmpty(gesture) ? text : $"{text} ({gesture})";
        }
    }

}
