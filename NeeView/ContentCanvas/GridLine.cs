﻿using NeeLaboratory.ComponentModel;
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
    public class GridLine : BindableBase
    {
        public GridLine()
        {
            Config.Current.ImageGrid.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(Content));
            };
        }

#if false
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (SetProperty(ref _isEnabled, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private Color _color = Color.FromArgb(0x80, 0x80, 0x80, 0x80);
        [PropertyMember("@ParamGridLineColor"), DefaultValue(typeof(Color), "#80808080")]
        public Color Color
        {
            get { return _color; }
            set { if (SetProperty(ref _color, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private int _divX = 8;
        [PropertyRange("@ParamGridLineDivX", 1, 50, TickFrequency = 1), DefaultValue(8)]
        public int DivX
        {
            get { return _divX; }
            set { if (SetProperty(ref _divX, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private int _divY = 8;
        [PropertyRange("@ParamGridLineDivY", 1, 50, TickFrequency = 1), DefaultValue(8)]
        public int DivY
        {
            get { return _divY; }
            set { if (SetProperty(ref _divY, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private bool _isSquare;
        [PropertyMember("@ParamGridLineIsSquare"), DefaultValue(false)]
        public bool IsSquare
        {
            get { return _isSquare; }
            set { if (SetProperty(ref _isSquare, value)) RaisePropertyChanged(nameof(Content)); }
        }
#endif

        private double _width;
        public double Width
        {
            get { return _width; }
            set { if (SetProperty(ref _width, value)) RaisePropertyChanged(nameof(Content)); }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set { if (SetProperty(ref _height, value)) RaisePropertyChanged(nameof(Content)); }
        }

        public UIElement Content
        {
            get { return CreatePath(); }
        }

        public void SetSize(double width, double height)
        {
            if (_width != width || _height != height)
            {
                _width = width;
                _height = height;
                RaisePropertyChanged(nameof(Width));
                RaisePropertyChanged(nameof(Height));
                RaisePropertyChanged(nameof(Content));
            }
        }

        private UIElement CreatePath()
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

        private Path CreatePath(Point startPoint, Point endPoint, Brush stroke)
        {
            var geometry = new LineGeometry(startPoint, endPoint);
            geometry.Freeze();

            return new Path()
            {
                Data = geometry,
                Stroke = stroke,
                StrokeThickness = 1
            };
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsEnabled { get; set; }

            [DataMember, DefaultValue(8)]
            public int DivX { get; set; }

            [DataMember, DefaultValue(8)]
            public int DivY { get; set; }

            [DataMember]
            public bool IsSquare { get; set; }

            [DataMember, DefaultValue(typeof(Color), "#80808080")]
            public Color Color { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.ImageGrid.DivX = DivX;
                config.ImageGrid.DivY = DivY;
                config.ImageGrid.IsSquare = IsSquare;
                config.ImageGrid.Color = Color;
                config.ImageGrid.IsEnabled = IsEnabled;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = Config.Current.ImageGrid.IsEnabled;
            memento.DivX = Config.Current.ImageGrid.DivX;
            memento.DivY = Config.Current.ImageGrid.DivY;
            memento.IsSquare = Config.Current.ImageGrid.IsSquare;
            memento.Color = Config.Current.ImageGrid.Color;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            // 更新回数を抑えるために設定前に無効にする
            //this.IsEnabled = false;

            //this.DivX = memento.DivX;
            //this.DivY = memento.DivY;
            //this.IsSquare = memento.IsSquare;
            //this.Color = memento.Color;
            //this.IsEnabled = memento.IsEnabled;
        }

        #endregion

    }
}
