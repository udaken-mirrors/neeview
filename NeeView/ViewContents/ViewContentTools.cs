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
                Foreground = Brushes.LightGray,
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

        public static void SetBitmapScalingMode(UIElement element, Size imageSize, ViewContentSize contentSize, BitmapScalingMode? scalingMode)
        {
            // ScalingMode が指定されている
            if (scalingMode is not null)
            {
                //Debug.WriteLine($"XX: Force {_scalingMode.Value}: {_element.Page}: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, scalingMode.Value);
                element.SnapsToDevicePixels = scalingMode.Value == BitmapScalingMode.NearestNeighbor;
            }
            // 画像サイズがビッタリの場合はドットバイドットになるような設定
            else if (contentSize.IsRightAngle && Math.Abs(contentSize.PixelSize.Width - imageSize.Width) < 1.1 && Math.Abs(contentSize.PixelSize.Height - imageSize.Height) < 1.1)
            {
                //Debug.WriteLine($"OO: NearestNeighbor: {_element.Page}: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.NearestNeighbor);
                element.SnapsToDevicePixels = true;
            }
            // DotKeep mode
            // TODO: Config.Current参照はよろしくない
            else if (Config.Current.ImageDotKeep.IsImageDotKeep(contentSize.PixelSize, imageSize))
            {
                //Debug.WriteLine($"XX: NearestNeighbor: {_element.Page}: {imageSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.NearestNeighbor);
                element.SnapsToDevicePixels = true;
            }
            else
            {
                //Debug.WriteLine($"XX: Fantastic: {_element.Page}: {imageSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.Fant);
                element.SnapsToDevicePixels = false;
            }
        }
    }
}
