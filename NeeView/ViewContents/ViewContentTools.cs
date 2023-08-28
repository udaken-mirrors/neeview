using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using NeeView.PageFrames;

namespace NeeView
{
    public static class ViewContentTools
    {
        public static FrameworkElement CreateLoadingContent(PageFrameElement source)
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));

            var stackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(stackPanel);

            var loading = new LoadingIcon()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            stackPanel.Children.Add(loading);

            return grid;
        }


        public static FrameworkElement CreateErrorContent(PageFrameElement source, string? message)
        {
            var pageSource = new FilePageSource(source.Page.Entry, FilePageIcon.Alert, message ?? "Error");
            return new FilePageControl(pageSource);
        }


        public static FrameworkElement CreateDummyContent(PageFrameElement source)
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0x80, 0x80));

            return grid;
        }
    }
}
