using NeeView.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;

namespace NeeView.Windows
{
    /// <summary>
    /// WindowChrome 用
    /// </summary>
    public static class WindowChromeTools
    {
        // TODO: 添付プロパティとユーティリティ機能がごっちゃになっているので整備する

        public static bool GetAttached(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedProperty);
        }

        public static void SetAttached(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedProperty, value);
        }

        public static readonly DependencyProperty AttachedProperty =
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(WindowChromeTools), new PropertyMetadata(false, AttachedPropertyChanged));

        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                SetWindowChrome(window);
            }
        }

        public static SnapLayoutPresenter? GetSnapLayoutPresenter(DependencyObject obj)
        {
            return (SnapLayoutPresenter?)obj.GetValue(SnapLayoutPresenterProperty);
        }

        public static void SetSnapLayoutPresenter(DependencyObject obj, SnapLayoutPresenter? value)
        {
            obj.SetValue(SnapLayoutPresenterProperty, value);
        }

        public static readonly DependencyProperty SnapLayoutPresenterProperty =
            DependencyProperty.RegisterAttached("SnapLayoutPresenter", typeof(SnapLayoutPresenter), typeof(WindowChromeTools), new PropertyMetadata(null));


        public static void SetWindowChrome(Window window)
        {
            var chrome = new WindowChrome();
            chrome.CornerRadius = new CornerRadius();
            chrome.UseAeroCaptionButtons = false;
            chrome.CaptionHeight = 0;
            chrome.GlassFrameThickness = new Thickness(1, 30, 1, 1);
            chrome.ResizeBorderThickness = new Thickness(4);

            SetWindowChrome(window, chrome);
        }

        public static void SetWindowChrome(Window window, WindowChrome chrome)
        {
            Debug.Assert(WindowChrome.GetWindowChrome(window) is null, "Already chromed");

            WindowChrome.SetWindowChrome(window, chrome);
            _ = new WindowChromePatch(window);

            // NOTE: SnapLayoutPresenter の WndProc をここで登録。順番によっては WM_NCHITTEST 等のメッセージが受信できなくなるため。
            SetSnapLayoutPresenter(window, new SnapLayoutPresenter(window));
        }
    }
}
