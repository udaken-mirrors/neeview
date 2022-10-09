using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using NeeView.Interop;


namespace NeeView.Windows
{
    /// <summary>
    /// WindowChrome が適用されたウィンドウ挙動の問題を補正する
    /// </summary>
    public class WindowChromePatch
    {
        private readonly Window _window;
        private readonly WindowChrome _windowChrome;
        private readonly Thickness _resizeBorderThickness;

        public WindowChromePatch(Window window, WindowChrome windowChrome)
        {
            if (window is null) throw new ArgumentNullException(nameof(window));
            if (windowChrome is null) throw new ArgumentNullException(nameof(windowChrome));

            Debug.Assert(WindowChrome.GetWindowChrome(window) is null, "Already chromed");

            _window = window;
            _windowChrome = windowChrome;
            _resizeBorderThickness = windowChrome.ResizeBorderThickness;

            WindowChrome.SetWindowChrome(_window, _windowChrome);
            _window.StateChanged += Window_StateChanged;

            if (!AddHook())
            {
                _window.SourceInitialized += (s, e) => AddHook();
            }
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            // NOTE: タブレットモードでのフルスクリーン時に画面端のカーソル座標が取得できなくなる現象の対策として ResizeBorderThickness を 0 にする。
            _windowChrome.ResizeBorderThickness = _window.WindowState == WindowState.Maximized ? new Thickness(0) : _resizeBorderThickness;
        }

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

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WindowMessages)msg)
            {
                case WindowMessages.WM_NCCALCSIZE:
                    // Adjust client area
                    // https://mntone.hateblo.jp/entry/2020/08/02/111309
                    if (wParam != IntPtr.Zero)
                    {
                        var result = this.CalcNonClientSize(hwnd, lParam, ref handled);
                        if (handled) return result;
                    }
                    break;

                case WindowMessages.WM_NCHITTEST:
                    // This prevents a crash in WindowChromeWorker._HandleNCHitTest
                    // https://developercommunity.visualstudio.com/content/problem/167357/overflow-exception-in-windowchrome.html?childToView=1209945#comment-1209945 
                    try
                    {
                        var x = lParam.ToInt32();
                        ////DebugInfo.Current?.SetMessage($"WM_NCHITTEST.LPARAM: {x:#,0}");
                        ////Debug.WriteLine($"{x:#,0}");
                    }
                    catch (OverflowException)
                    {
                        handled = true;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// 最大化時のクライアントエリアの調整
        /// </summary>
        /// <remarks>
        /// The MIT License<br/>
        /// Copyright 2020 mntone<br/>
        /// https://mntone.hateblo.jp/entry/2020/08/02/111309
        /// </remarks>
        /// <returns></returns>
        private IntPtr CalcNonClientSize(IntPtr hWnd, IntPtr lParam, ref bool handled)
        {
            if (!NativeMethods.IsZoomed(hWnd)) return IntPtr.Zero;

            var rcsize = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);
            //Debug.WriteLine("CalcNonClientSize {0}", rcsize.lppos.flags);
            if (rcsize.lppos.flags.HasFlag(SetWindowPosFlags.SWP_NOSIZE)) return IntPtr.Zero;

            var hMonitor = NativeMethods.MonitorFromWindow(hWnd, (int)MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            if (hMonitor == IntPtr.Zero) return IntPtr.Zero;

            var monitorInfo = new MONITORINFO()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO))
            };
            if (!NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo)) return IntPtr.Zero;

            RECT workArea;
            if (_window.WindowStyle == WindowStyle.None)
            {
                // フルスクリーン時(None)はモニターサイズにする。
                workArea = monitorInfo.rcMonitor;
            }
            else
            {
                // 通常時はモニターのワークサイズを指定。タスクバーが自動非表示の場合のマージンも加味する。
                workArea = monitorInfo.rcWork;
                AppBar.ApplyAppbarSpace(hMonitor, monitorInfo.rcMonitor, ref workArea);
            }
            //Debug.WriteLine("CalcNonClientSize {0} {1} {2} {3}", workArea.left, workArea.top, workArea.right, workArea.bottom);

            rcsize.rgrc[0] = workArea;
            rcsize.rgrc[1] = workArea;
            rcsize.rgrc[2] = workArea;
            Marshal.StructureToPtr(rcsize, lParam, true);

            handled = true;
            return (IntPtr)(WindowValidRects.WVR_ALIGNTOP | WindowValidRects.WVR_ALIGNLEFT | WindowValidRects.WVR_VALIDRECTS);
        }
    }
}
