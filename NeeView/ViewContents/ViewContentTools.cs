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
            var textBlock = new TextBlock();
            //textBlock.Text = $"({range.ToDispString()})";
            textBlock.Text = $"Loading ...";
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.Margin = new Thickness(10, 10, 0, 0);

            var grid = new Grid();
            grid.Background = Brushes.DarkGray;
            grid.Children.Add(textBlock);

            return grid;
        }

        public static FrameworkElement CreateErrorContent(PageFrameElement source, string? message)
        {
            var textBlock = new TextBlock();
            textBlock.Text = message ?? "Error";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(10, 10, 0, 0);

            var grid = new Grid();
            grid.Background = Brushes.DarkGray;
            grid.Children.Add(textBlock);

            return grid;
        }

        public static FrameworkElement CreateDummyContent(PageFrameElement source)
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));

            return grid;
        }
    }
}
