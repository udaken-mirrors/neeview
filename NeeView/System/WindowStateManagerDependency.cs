using NeeView.ComponentModel;
using NeeView.Windows;
using System.ComponentModel;

namespace NeeView
{
    public class WindowStateManagerDependency : IWindowStateManagerDependency
    {
        private readonly WindowChromeAccessor _chrome;
        private readonly TabletModeWatcher _tabletModeWatcher;

        // NOTE: インスタンス保持用
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:読み取られていないプライベート メンバーを削除", Justification = "<保留中>")]
        private readonly WeakBindableBase<WindowConfig> _windowConfig;


        public WindowStateManagerDependency(WindowChromeAccessor chrome, TabletModeWatcher tabletModeWatcher)
        {
            _chrome = chrome;
            _tabletModeWatcher = tabletModeWatcher;

            _windowConfig = new WeakBindableBase<WindowConfig>(Config.Current.Window);
        }

        public bool IsTabletMode => _tabletModeWatcher.IsTabletMode;

        public double MaximizedWindowThickness => Config.Current.Window.MaximizeWindowGapWidth;


        public void ResumeWindowChrome()
        {
            _chrome.IsSuspended = false;
        }

        public void SuspendWindowChrome()
        {
            _chrome.IsSuspended = true;
        }
    }
}
