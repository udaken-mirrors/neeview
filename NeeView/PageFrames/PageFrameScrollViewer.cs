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



namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameScrollViewer : Grid, IPointControl, INotifyPropertyChanged
    {
        private BookContext _context;
        private Canvas _rootCanvas;
        private PageFrameContainersCanvas _canvas;

        private PageFrameViewTransform _transform;


        public PageFrameScrollViewer(BookContext context, PageFrameContainersCanvas canvas, PageFrameViewTransform transform)
        {
            MinWidth = 32;
            MinHeight = 32;

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
                Children.Add(textBox);
            }
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


        public event PropertyChangedEventHandler? PropertyChanged;


        public PageFrameViewTransform Transform => _transform;


        public Point Point
        {
            get { return _transform.Point; }
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

        public void AddPoint(Vector value, TimeSpan span)
        {
            SetPoint(Point + value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            if (Point == value) return;

            //Debug.WriteLine($"## {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds})");
            _transform.SetPoint(value, span);
            RaisePropertyChanged(nameof(Point));
        }

        public void FlushScroll()
        {
            _transform.Flush();
        }
    }
}
