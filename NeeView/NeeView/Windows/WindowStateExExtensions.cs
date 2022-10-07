using System.Windows;

namespace NeeView.Windows
{
    public static class WindowStateExExtensions
    {
        public static WindowState ToWindowState(this WindowStateEx self)
        {
            return self switch
            {
                WindowStateEx.Minimized
                    => WindowState.Minimized,
                WindowStateEx.Maximized or WindowStateEx.FullScreen
                    => WindowState.Maximized,
                _
                    => WindowState.Normal,
            };
        }

        public static bool IsFullScreen(this WindowStateEx self)
        {
            return self == WindowStateEx.FullScreen;
        }
    }
}
