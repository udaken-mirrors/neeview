using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class GridLine : ContentControl, IDisposable
    {
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();


        public GridLine()
        {
            this.Focusable = false;

            _disposables.Add(Config.Current.ImageGrid.SubscribePropertyChanged((s, e) => Update()));
            _disposables.Add(this.SubscribeSizeChanged((s, e) => Update()));
        }


        public IDisposable SubscribeSizeChanged(SizeChangedEventHandler handler)
        {
            SizeChanged += handler;
            return new AnonymousDisposable(() => SizeChanged -= handler);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Update()
        {
            if (_disposedValue) return;
            this.Content = CreatePath();

            // センタリング配置
            Canvas.SetLeft(this, -Width * 0.5);
            Canvas.SetTop(this, -Height * 0.5);
        }

        private UIElement? CreatePath()
        {
            var imageGrid = Config.Current.ImageGrid;

            if (!imageGrid.IsEnabled || Width <= 0.0 || Height <= 0.0) return null;

            double cellX = imageGrid.DivX > 0 ? Width / imageGrid.DivX : Width;
            double cellY = imageGrid.DivY > 0 ? Height / imageGrid.DivY : Height;

            if (imageGrid.IsSquare)
            {
                if (cellX < cellY)
                {
                    cellX = cellY;
                }
                else
                {
                    cellY = cellX;
                }
            }

            var canvas = new Canvas();
            canvas.Width = Width;
            canvas.Height = Height;

            var stroke = new SolidColorBrush(imageGrid.Color);

            canvas.Children.Add(CreatePath(new Point(0, 0), new Point(0, Height), stroke));
            canvas.Children.Add(CreatePath(new Point(Width, 0), new Point(Width, Height), stroke));
            canvas.Children.Add(CreatePath(new Point(0, 0), new Point(Width, 0), stroke));
            canvas.Children.Add(CreatePath(new Point(0, Height), new Point(Width, Height), stroke));

            for (double i = cellX; i < Width - 1; i += cellX)
            {
                canvas.Children.Add(CreatePath(new Point(i, 0), new Point(i, Height), stroke));
            }

            for (double i = cellY; i < Height - 1; i += cellY)
            {
                canvas.Children.Add(CreatePath(new Point(0, i), new Point(Width, i), stroke));
            }

            return canvas;
        }

        private static Path CreatePath(Point startPoint, Point endPoint, Brush stroke)
        {
            var geometry = new LineGeometry(startPoint, endPoint);
            geometry.Freeze();

            return new Path()
            {
                Data = geometry,
                Stroke = stroke,
                StrokeThickness = 1,
            };
        }

    }

}
