using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// Windows11 の SnapLayout サポート
    /// </summary>
    // from https://bitbucket.org/neelabo/neeview/issues/1183/windows-11
    // from https://stackoverflow.com/questions/69797178/support-windows-11-snap-layout-in-wpf-app
    public class SnapLayoutPresenter
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_NCLBUTTONUP = 0x00A2;
        private const int WM_NCMOUSELEAVE = 0x02A2;

        //private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;

        //private Window _window;
        private IHasMaximizeButton? _maximizeButton;
        private CaptionButtonState _activeButtonState = CaptionButtonState.MouseOver;

#if false
        public SnapLayoutPresenter(Window window)
        {
            _window = window;

            if (!Regist())
            {
                _window.SourceInitialized += (s, e) => Regist();
            }
        }

        private bool Regist()
        {
            var hWnd = new WindowInteropHelper(_window).Handle;
            if (hWnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hWnd).AddHook(new HwndSourceHook(WndProc));
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetMaximezeButtonSourcer(IHasMaximizeButton source)
        {
            _maximizeButton = source;
        }
#else

        public SnapLayoutPresenter(IHasMaximizeButton maximizeButton)
        {
            _maximizeButton = maximizeButton;
        }

        /// <summary>
        /// Attach SnapLayout
        /// </summary>
        /// <param name="window"></param>
        public void Attach(Window window)
        {
            if (window is null) return;
            if (!Windows11Tools.IsWindows11OrGreater) return;

            //var hwnd = PresentationSource.FromVisual(window) as System.Windows.Interop.HwndSource;
            //hwnd?.AddHook(WndProc);

            var source = WndProcSource.GetWndProcSource(window);
            source?.AddHook(WndProc);
        }

        /// <summary>
        /// Detach SnapLayout
        /// </summary>
        /// <param name="window"></param>
        public void Detach(Window window)
        {
            if (window is null) return;
            if (!Windows11Tools.IsWindows11OrGreater) return;

            //var hwnd = PresentationSource.FromVisual(window) as System.Windows.Interop.HwndSource;
            //hwnd?.RemoveHook(WndProc);

            var source = WndProcSource.GetWndProcSource(window);
            source?.RemoveHook(WndProc);
        }
#endif

        /// <summary>
        /// WinProc for SnapLayout
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //System.Diagnostics.Debug.WriteLine($"SL.WMSG: {msg:X04}");

            if (_maximizeButton is null) return IntPtr.Zero;

            return msg switch
            {
                WM_NCHITTEST
                    => OnNCHitTest(wParam, lParam, ref handled),
                WM_NCLBUTTONDOWN
                    => OnNCLButtonDown(wParam, lParam, ref handled),
                WM_NCLBUTTONUP
                    => OnNCLButtonUp(wParam, lParam, ref handled),
                WM_NCMOUSELEAVE
                    => OnNCMouseLeave(wParam, lParam, ref handled),
                _
                    => IntPtr.Zero,
            };
        }

#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

        /// <summary>
        /// WM_NCHITTEST
        /// </summary>
        private IntPtr OnNCHitTest(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //System.Diagnostics.Debug.WriteLine("*");
            if (_maximizeButton is null) return IntPtr.Zero;

            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            bool isHit;
            try
            {
                isHit = HitTest(button, lParam);
            }
            catch (OverflowException)
            {
                // NOTE: WindowChromePatch で処理しているが、念の為にここでも例外処理を行う。
                // This prevents a crash in WindowChromeWorker._HandleNCHitTest
                // https://developercommunity.visualstudio.com/content/problem/167357/overflow-exception-in-windowchrome.html?childToView=1209945#comment-1209945 
                handled = true;
                return IntPtr.Zero;
            }

            if (isHit)
            {
                _maximizeButton.SetMaximizeButtonBackground(_activeButtonState);
                //System.Diagnostics.Debug.WriteLine("# IN");
                handled = true;
                return new IntPtr(HTMAXBUTTON);
            }
            else
            {
                _maximizeButton.SetMaximizeButtonBackground(CaptionButtonState.Default);
                //System.Diagnostics.Debug.WriteLine("# OUT");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// WM_NCLBUTTONDOWN
        /// </summary>
        private IntPtr OnNCLButtonDown(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Debug.WriteLine($"NCLButtonDown: {(int)wParam}");

            if (_maximizeButton is null) return IntPtr.Zero;

            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            if ((int)wParam == HTMAXBUTTON)
            {
                _activeButtonState = CaptionButtonState.Pressed;
                _maximizeButton.SetMaximizeButtonBackground(_activeButtonState);
                //Debug.WriteLine("# DOWN");
                handled = true;
            }

            return IntPtr.Zero;
        }


        /// <summary>
        /// WM_NCLBUTTONUP
        /// </summary>
        private IntPtr OnNCLButtonUp(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Debug.WriteLine($"NCLButtonUp: {(int)wParam}");

            if (_maximizeButton is null) return IntPtr.Zero;

            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            if ((int)wParam == HTMAXBUTTON)
            {
                _activeButtonState = CaptionButtonState.MouseOver;
                _maximizeButton.SetMaximizeButtonBackground(_activeButtonState);
                Debug.WriteLine("# UP");
                handled = true;
                IInvokeProvider? invokeProv = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv?.Invoke();
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// WM_NCMOUSELEAVE
        /// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private IntPtr OnNCMouseLeave(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_maximizeButton is null) return IntPtr.Zero;

            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            _activeButtonState = CaptionButtonState.MouseOver;
            _maximizeButton.SetMaximizeButtonBackground(CaptionButtonState.Default);
            Debug.WriteLine("# LEAVE");
            return IntPtr.Zero;
        }

#pragma warning restore IDE0060 // 未使用のパラメーターを削除します

        /// <summary>
        /// ウィンドウメッセージ座標でのコントロール当たり判定
        /// </summary>
        /// <param name="element">コントロール</param>
        /// <param name="lParam">ウィンドウメッセージのLPARAM(座標)</param>
        /// <returns></returns>
        private static bool HitTest(FrameworkElement element, IntPtr lParam)
        {
            if (element is null || !element.IsVisible)
            {
                return false;
            }

            var dpi = VisualTreeHelper.GetDpi(element);
            var rect = new Rect(element.PointToScreen(new Point()), new Size(element.ActualWidth * dpi.DpiScaleX, element.ActualHeight * dpi.DpiScaleY));
            short x = GET_X_LPARAM(lParam);
            short y = GET_Y_LPARAM(lParam);
            return rect.Contains(x, y);

            short GET_X_LPARAM(IntPtr lp)
            {
                return (short)(ushort)((uint)lp.ToInt32() & 0xffff);
            }

            short GET_Y_LPARAM(IntPtr lp)
            {
                return (short)(ushort)((uint)lp.ToInt32() >> 16);
            }
        }
    }


    // NOTE: WndProc をまとめたもの。これは一時的なものです。
    public class WndProcSource
    {
        public static bool GetAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedProperty);
        }

        public static void SetAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedProperty, value);
        }

        public static readonly DependencyProperty AttachedProperty =
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(WndProcSource), new PropertyMetadata(false, AttachedPropertyChanged));

        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                SetWndProcSource(window, new WndProcSource(window));
            }
        }

        public static WndProcSource? GetWndProcSource(DependencyObject obj)
        {
            return (WndProcSource?)obj.GetValue(WndProcSourceProperty);
        }

        public static void SetWndProcSource(DependencyObject obj, WndProcSource value)
        {
            obj.SetValue(WndProcSourceProperty, value);
        }

        public static readonly DependencyProperty WndProcSourceProperty =
            DependencyProperty.RegisterAttached("WndProcSource", typeof(WndProcSource), typeof(WndProcSource), new PropertyMetadata(null, WndProcSourcePropertyChanged));

        private static void WndProcSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }



        private readonly Window _window;
        private readonly List<HwndSourceHook> _hooks = new();


        public WndProcSource(Window window)
        {
            _window = window;

            if (!Regist())
            {
                _window.SourceInitialized += (s, e) => Regist();
            }
        }
        private bool Regist()
        {
            var hWnd = new WindowInteropHelper(_window).Handle;
            if (hWnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(hWnd).AddHook(new HwndSourceHook(WndProc));
                return true;
            }
            else
            {
                return false;
            }
        }


        public void AddHook(HwndSourceHook hook)
        {
            if (!_hooks.Contains(hook))
            {
                _hooks.Add(hook);
            }
        }

        public void RemoveHook(HwndSourceHook hook)
        {
            _hooks.Remove(hook);
        }



        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            foreach (var hook in _hooks)
            {
                IntPtr result = hook.Invoke(hwnd, msg, wParam, lParam, ref handled);
                if (handled)
                {
                    return result;
                }
            }

            return IntPtr.Zero;
        }

    }
}
