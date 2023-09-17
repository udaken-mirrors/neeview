using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrameContainer を配置する Canvas
    /// </summary>
    public class PageFrameContainerCanvas : Canvas
    {
        private readonly PageFrameContext _context;
        private readonly PageFrameContainerCollection _containers;


        public PageFrameContainerCanvas(PageFrameContext context, PageFrameContainerCollection containers)
        {
            _context = context;
            _containers = containers;

            var containerInitializer = new PageFrameContainerInitializer(this);
            _containers.SetContainerInitializer(containerInitializer);

#if false
            // [DEV]
            Children.Add(new Rectangle()
            {
                Width = 2,
                Height = 2,
                Fill = Brushes.Red,
            });
#endif

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


