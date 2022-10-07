using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;

namespace NeeView.Windows
{
    public static class WindowChromeTools
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
            DependencyProperty.RegisterAttached("Attached", typeof(bool), typeof(WindowChromeTools), new PropertyMetadata(false, AttachedPropertyChanged));

        private static void AttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool)e.NewValue)
            {
                SetWindowChrome(window);
            }
        }


        public static void SetWindowChrome(Window window)
        {
            var chrome = new WindowChrome();
            chrome.CornerRadius = new CornerRadius();
            chrome.UseAeroCaptionButtons = false;
            chrome.CaptionHeight = 0;
            chrome.GlassFrameThickness = new Thickness(1);
            chrome.ResizeBorderThickness = new Thickness(4);

            SetWindowChrome(window, chrome);
        }

        public static void SetWindowChrome(Window window, WindowChrome chrome)
        {
            Debug.Assert(WindowChrome.GetWindowChrome(window) is null, "Already chromed");

            WindowChrome.SetWindowChrome(window, chrome);
            _ = new WindowChromePatch(window);
        }
    }
}
