using System;
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
            grid.Background = Brushes.White; // new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));

            var stackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(stackPanel);

            var textBlock = new TextBlock()
            {
                Text = source.Page.EntryLastName,
                Foreground = Brushes.LightGray,
                FontSize = 20,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            stackPanel.Children.Add(textBlock);

            var loading = new LoadingIcon()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            stackPanel.Children.Add(loading);

            return grid;
        }


        public static FrameworkElement CreateErrorContent(PageFrameElement source, string? message)
        {
            var viewData = new FileViewData(source.Page.ArchiveEntry, FilePageIcon.Alert, message ?? "Error");
            return new FilePageControl(viewData);
        }

    }
}
