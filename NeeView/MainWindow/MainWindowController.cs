using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// MainWindowに特化したウィンドウ制御
    /// </summary>
    public class MainWindowController : WindowController, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        private readonly Window _window;
        private readonly WindowStateManager _manager;
        private bool _autoHideMode;


        public MainWindowController(Window window, WindowStateManager manager) : base(window, manager)
        {
            _window = window;

            _manager = manager;
            _manager.StateChanged += WindowStateManager_StateChanged;

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsTopmost),
                (s, e) => RaisePropertyChanged(nameof(IsTopmost)));

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.State),
                (s, e) => _manager.SetWindowState(Config.Current.Window.State));

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsAutoHideInNormal),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsAutoHidInMaximized),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsAutoHideInFullScreen),
                (s, e) => UpdatePanelHideMode());
        }


        public override bool IsTopmost
        {
            get { return Config.Current.Window.IsTopmost; }
            set { Config.Current.Window.IsTopmost = value; }
        }

        public bool AutoHideMode
        {
            get { return _autoHideMode; }
            set { SetProperty(ref _autoHideMode, value); }
        }


        private void WindowStateManager_StateChanged(object? sender, WindowStateExChangedEventArgs e)
        {
            Config.Current.Window.State = e.NewState;
            UpdatePanelHideMode();
        }

        public void UpdatePanelHideMode()
        {
            AutoHideMode = _manager.CurrentState switch
            {
                WindowStateEx.Normal => Config.Current.Window.IsAutoHideInNormal,
                WindowStateEx.Maximized => Config.Current.Window.IsAutoHidInMaximized,
                WindowStateEx.FullScreen => Config.Current.Window.IsAutoHideInFullScreen,
                _ => false,
            };
        }

        private void ValidateWindowState()
        {
            if (Config.Current.Window.State != WindowStateEx.None) return;

            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    Config.Current.Window.State = WindowStateEx.Normal;
                    break;
                case WindowState.Minimized:
                    Config.Current.Window.State = WindowStateEx.Minimized;
                    break;
                case WindowState.Maximized:
                    Config.Current.Window.State = WindowStateEx.Maximized;
                    break;
            }
        }

        /// <summary>
        /// 状態を最新にする
        /// </summary>
        public void Refresh()
        {
            ValidateWindowState();
            UpdatePanelHideMode();
            _manager.SetWindowState(Config.Current.Window.State);
            RaisePropertyChanged(null);
        }
    }
}
