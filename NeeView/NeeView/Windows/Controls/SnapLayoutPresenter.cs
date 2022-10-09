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

        private readonly Window _window;
        private IMaximizeButtonSource? _maximizeButton;
        private CaptionButtonState _activeButtonState = CaptionButtonState.MouseOver;


        public SnapLayoutPresenter(Window window)
        {
            _window = window;

            if (!AddHook())
            {
                _window.SourceInitialized += (s, e) => AddHook();
            }
        }


        /// <summary>
        /// MaximizeButton を登録することで処理が機能します。
        /// </summary>
        public void SetMaximezeButtonSourcer(IMaximizeButtonSource? source)
        {
            _maximizeButton = source;
        }

        /// <summary>
        /// regist WndProc
        /// </summary>
        private bool AddHook()
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

        /// <summary>
        /// WinProc for SnapLayout
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
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
                //System.Diagnostics.Debug.WriteLine("# IN");
                _maximizeButton.SetMaximizeButtonBackground(_activeButtonState);
                handled = true;
                return new IntPtr(HTMAXBUTTON);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("# OUT");
                _maximizeButton.SetMaximizeButtonBackground(CaptionButtonState.Default);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// WM_NCLBUTTONDOWN
        /// </summary>
        private IntPtr OnNCLButtonDown(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_maximizeButton is null) return IntPtr.Zero;
            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            if ((int)wParam == HTMAXBUTTON)
            {
                //Debug.WriteLine("# DOWN");
                _activeButtonState = CaptionButtonState.Pressed;
                _maximizeButton.SetMaximizeButtonBackground(_activeButtonState);
                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// WM_NCLBUTTONUP
        /// </summary>
        private IntPtr OnNCLButtonUp(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_maximizeButton is null) return IntPtr.Zero;
            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            if ((int)wParam == HTMAXBUTTON)
            {
                //Debug.WriteLine("# UP");
                _activeButtonState = CaptionButtonState.MouseOver;
                _maximizeButton.SetMaximizeButtonBackground(_activeButtonState);
                handled = true;
                IInvokeProvider? invokeProv = new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv?.Invoke();
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// WM_NCMOUSELEAVE
        /// </summary>
        private IntPtr OnNCMouseLeave(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_maximizeButton is null) return IntPtr.Zero;
            var button = _maximizeButton.GetMaximizeButton();
            if (button is null) return IntPtr.Zero;

            //Debug.WriteLine("# LEAVE");
            _activeButtonState = CaptionButtonState.MouseOver;
            _maximizeButton.SetMaximizeButtonBackground(CaptionButtonState.Default);
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
}
