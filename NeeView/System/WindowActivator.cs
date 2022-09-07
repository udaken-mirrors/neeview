using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ウィンドウ切り替え
    /// </summary>
    public class WindowActivator
    {
        public static void NextActivate(int direction)
        {
            var changed = NextSubWindow(direction, false);
            if (changed) return;

            var process = ProcessActivator.NextActivate(direction);
            if (process != null) return;

            if (GetSubWindows().Any())
            {
                Application.Current.MainWindow.Activate();
            }
        }

        private static IEnumerable<Window> GetSubWindows()
        {
            var viewWindow = MainViewManager.Current.Window;
            var layoutPanelWindows = CustomLayoutPanelManager.Current.Windows.Windows.Cast<Window>();

            return viewWindow != null ? layoutPanelWindows.Prepend(viewWindow) : layoutPanelWindows;
        }

        private static bool NextSubWindow(int direction, bool loopable)
        {
            var windows = new List<Window>() { Application.Current.MainWindow };
            var subWindows = GetSubWindows();
            windows.AddRange(direction > 0 ? subWindows : subWindows.Reverse());

            if (!loopable && windows.Last().IsActive) return false;

            var activeWindow = windows.FirstOrDefault(e => e.IsActive);
            if (activeWindow is null)
            {
                var isActived = ActivateSubWindow(windows.First());
                //Debug.WriteLine($"Activate: {isActived}: {windows.First().Title}");
            }
            else
            {
                var index = (windows.IndexOf(activeWindow) + 1) % windows.Count;
                var isActived = ActivateSubWindow(windows[index]);
                //Debug.WriteLine($"Activate: {isActived}: {windows[index].Title}");
            }

            return true;
        }


        private static bool ActivateSubWindow(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                SystemCommands.RestoreWindow(window);
            }

            return window.Activate();
        }


    }

}
