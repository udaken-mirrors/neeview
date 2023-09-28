using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameScrollViewer : Grid, IPointControl, INotifyPropertyChanged
    {
        private readonly PageFrameContext _context;
        private readonly Canvas _rootCanvas;
        private readonly PageFrameContainerCanvas _canvas;
        private readonly PageFrameViewTransform _transform;


        public PageFrameScrollViewer(PageFrameContext context, PageFrameContainerCanvas canvas, PageFrameViewTransform transform)
        {
            this.MinWidth = PageFrameProfile.MinWidth;
            this.MinHeight = PageFrameProfile.MinHeight;
            this.ClipToBounds = true;

            _context = context;
            _canvas = canvas;

            _transform = transform;
            _canvas.RenderTransform = transform.TransformView;


            _rootCanvas = new Canvas()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Children.Add(_rootCanvas);

            _rootCanvas.Children.Add(_canvas);

#if DEBUG
            // [DEV]
            {
                var textBox = new TextBox();
                textBox.HorizontalAlignment = HorizontalAlignment.Left;
                textBox.VerticalAlignment = VerticalAlignment.Bottom;
                var binding = new MultiBinding();
                binding.Bindings.Add(new Binding("Point.X") { Source = this });
                binding.Bindings.Add(new Binding("Point.Y") { Source = this });
                binding.Converter = new CanvasPositionConverter();
                textBox.SetBinding(TextBox.TextProperty, binding);
                textBox.Visibility = PageFrameDebug.Visibility;
                Children.Add(textBox);
            }
#endif
        }

        private class CanvasPositionConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var x = (double)values[0];
                var y = (double)values[1];
                return $"Canvas: {x:f0},{y:f0}";
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public PageFrameViewTransform Transform => _transform;


        public Point Point
        {
            get { return _transform.Point; }
        }



        public IDisposable SubscribeSizeChanged(SizeChangedEventHandler handler)
        {
            SizeChanged += handler;
            return new AnonymousDisposable(() => SizeChanged -= handler);
        }

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            SetPoint(default, TimeSpan.Zero);
        }

        public Point GetRealPoint()
        {
            return _transform.TransformView.Transform(new Point(0, 0));
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            if (Point == value) return;

            Debug.WriteLine($"## {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds})");
            _transform.SetPoint(value, span, easeX, easeY);
            RaisePropertyChanged(nameof(Point));
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            SetPoint(Point + value, span, easeX, easeY);
        }

        public void FlushScroll()
        {
            _transform.Flush();
        }
    }
}
