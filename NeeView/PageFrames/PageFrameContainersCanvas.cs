using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace NeeView.PageFrames
{
    public class PageFrameContainerInitializer : IInitializable<PageFrameContainer>
    {
        private Canvas _canvas;

        public PageFrameContainerInitializer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Initialize(PageFrameContainer item)
        {
            item.Visibility = Visibility.Visible;
            _canvas.Children.Add(item);
        }

        public void Uninitialized(PageFrameContainer item)
        {
            _canvas.Children.Remove(item);
        }
    }


    /// <summary>
    /// PageFrameContainer を配置する Canvas
    /// </summary>
    public class PageFrameContainersCanvas : Canvas
    {

        private BookContext _context;
        private PageFrameContainerCollection _containers;


        public PageFrameContainersCanvas(BookContext context, PageFrameContainerCollection containers)
        {
            _context = context;
            _containers = containers;

            var containerInitializer = new PageFrameContainerInitializer(this);
            _containers.SetContainerInitializer(containerInitializer);

            // [DEV]
            Children.Add(new Rectangle()
            {
                Width = 2,
                Height = 2,
                Fill = Brushes.Red,
            });

#if false
            var grid = new Grid()
            {
                Width = 4096,
                Height = 4096,
                Background = CheckBackgroundBrush,
                Opacity = 0.75,
            };
            Canvas.SetLeft(grid, -2048);
            Canvas.SetTop(grid, -2048);
            this.Children.Insert(0, grid);
#endif
        }

    }
}


