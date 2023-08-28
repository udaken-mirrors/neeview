using NeeView.PageFrames;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class DummyViewContent : ViewContent
    {
        public DummyViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity) : base(element, scale, viewSource, activity)
        {
        }

        protected override  FrameworkElement CreateLoadedContent(Size size, object data)
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Element.Page.Content.PictureInfo?.Color ?? Colors.White);
            return grid;
        }
    }
}
