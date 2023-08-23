using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NeeView.PageFrames;

namespace NeeView
{
    public class ViewContentControl : Grid
    {
        public string ContentType { get; }
    }


    public class LoadingViewContent : ViewContentControl
    {
        public LoadingViewContent(PageFrameElement source)
        {
            InitializeContent();
        }

        public void InitializeContent()
        { 
            this.Background = Brushes.DarkGray;

            var textBlock = new TextBlock();
            //textBlock.Text = $"({range.ToDispString()})";
            textBlock.Text = $"Loading ...";
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.Margin = new Thickness(10, 10, 0, 0);
            this.Children.Add(textBlock);
        }
    }

    public class ErrorViewContent : ViewContentControl
    {
        public ErrorViewContent(PageFrameElement source, string? message)
        {
            InitializeContent(message);
        }

        public void InitializeContent(string? message)
        {
            this.Background = Brushes.DarkGray;

            var textBlock = new TextBlock();
            textBlock.Text = message ?? "Error";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(10, 10, 0, 0);
            this.Children.Add(textBlock);
        }
    }

    public class DummyViewContent : ViewContentControl
    {
        public DummyViewContent(PageFrameElement source)
        {
            InitializeContent();
        }

        public void InitializeContent()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));
        }
    }



    public static class ViewContentTools
    {
        public static FrameworkElement CreateLoadingContent(PageFrameElement source)
        {
            var grid = new Grid();
            //grid.Background = Brushes.DarkGray;

#if false
            var textBlock = new TextBlock();
            //textBlock.Text = $"({range.ToDispString()})";
            textBlock.Text = $"Loading ...";
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.Margin = new Thickness(10, 10, 0, 0);
            grid.Children.Add(textBlock);
#endif

            var stackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(stackPanel);

            var textBlock = new TextBlock()
            {
                Text = $"Page {source.Page.Index + 1}",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
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
            var pageSource = new FilePageSource(source.Page.Entry, FilePageIcon.Alert, message ?? "Error");
            return new FilePageControl(pageSource);

#if false
            var textBlock = new TextBlock();
            textBlock.Text = message ?? "Error";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(10, 10, 0, 0);

            var grid = new Grid();
            grid.Background = Brushes.DarkGray;
            grid.Children.Add(textBlock);

            return grid;
#endif
        }

        public static FrameworkElement CreateDummyContent(PageFrameElement source)
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));


            return grid;
        }
    }
}
