﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NeeLaboratory.Generators;
using PageRange = NeeView.PageRange;


namespace NeeView.PageFrames
{
    [NotifyPropertyChanged]
    public partial class PageFrameActivity : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isVisible = true;

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
    }



    public class PageFrameContainer : Grid, IDisposable, IComparable<PageFrameContainer>, INotifyTransformChanged
    {
        private double _ms = 0.0;

        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private bool _disposedValue;

        private PageFrameActivity _activity;
        private ContentControl _contentControl;
        private IPageFrameContent _content;

        private TextBlock _textBlock;


        public PageFrameContainer(IPageFrameContent content, PageFrameActivity activity)
        {
            _activity = activity;

            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            ClipToBounds = true;

            Canvas.SetTop(this, _x);
            Canvas.SetLeft(this, _y);
            Width = _width;
            Height = _height;

            _contentControl = new ContentControl()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Children.Add(_contentControl);

            // [DEV]
            _textBlock = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 32.0,
                Foreground = Brushes.Orange,
            };
            Children.Add(_textBlock);

            AttachContent(content);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DetachContent();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        public event TransformChangedEventHandler? TransformChanged;
        public event EventHandler? ContentSizeChanged;
        public event EventHandler? ContentChanged;


        public PageFrameActivity Activity => _activity;

        public IPageFrameTransform Transform => _content.Transform;

        public bool IsLocked => _content.IsLocked;
        public bool IsDarty => _content.DartyLevel > PageFrameDartyLevel.Clean;
        public bool IsLayouted { get; set; }

        public PageFrameDartyLevel DartyLevel
        {
            get => _content.DartyLevel;
            set => _content.DartyLevel = value;
        }


        // TODO: これどうなん？
        public PagePosition Identifier => FrameRange.Min;
        public PageRange FrameRange => _content.FrameRange;

        public bool IsFirstFrame => _content.IsFirstFrame;
        public bool IsLastFrame => _content.IsLastFrame;

        public bool IsHotizontalAnimationEnabled { get; set; }
        public bool IsVerticalAnimationEnabled { get; set; }


        public IPageFrameContent Content
        {
            get => _content;
            set => AttachContent(value);
        }

        public double Duration
        {
            get => _ms;
            set => _ms = value;
        }

        public double X
        {
            get => _x;
            set => SetX(value, IsHotizontalAnimationEnabled ? _ms : 0.0);
        }

        public double Y
        {
            get => _y;
            set => SetY(value, IsVerticalAnimationEnabled ? _ms : 0.0);
        }

        public new double Width
        {
            get => _width;
            set => SetWidth(value, IsHotizontalAnimationEnabled ? _ms : 0.0);
        }

        public new double Height
        {
            get => _height;
            set => SetHeight(value, IsVerticalAnimationEnabled ? _ms : 0.0);
        }

        public Rect Rect
        {
            get => new Rect(_x, _y, _width, _height);
        }

        public Point Center
        {
            get => new Point(_x + _width * 0.5, _y + _height * 0.5);
            set
            {
                X = value.X - _width * 0.5;
                Y = value.Y - _height * 0.5;
            }
        }


        public int CompareTo(PageFrameContainer? other)
        {
            if (other == null) return 1;

            return Identifier.CompareTo(other.Identifier);
        }

        public static int CompareTo(PageFrameContainer? x, PageFrameContainer? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;

            return x.CompareTo(y);
        }

        public override string? ToString()
        {
            return _content.ToString();
        }

        public void SetX(double x, double ms)
        {
            //if (this.FrameRange.Max == new PagePosition(0, 1))
            //{
            //    Debug.WriteLine($"#{this.FrameRange}: X={_x:f0} to {x:f0} ({Math.Abs(x - _x):f0})");
            //    if (Math.Abs(x - _x) > 100)
            //    {
            //        Debug.WriteLine("!!");
            //    }
            //}

            BeginAnimation(Canvas.LeftProperty, x, ms);
            _x = x;
        }

        public void SetY(double y, double ms)
        {
            BeginAnimation(Canvas.TopProperty, y, ms);
            _y = y;
            //Debug.WriteLine($"# {FrameRange}: Y: {_y} / {_height}");
        }

        public void SetWidth(double width, double ms)
        {
            BeginAnimation(WidthProperty, width, ms);
            _width = width;
        }

        public void SetHeight(double height, double ms)
        {
            BeginAnimation(HeightProperty, height, ms);
            _height = height;
        }

        public void FlushLayout()
        {
            BeginAnimation(Canvas.LeftProperty, _x, 0.0);
            BeginAnimation(Canvas.TopProperty, _y, 0.0);
            BeginAnimation(WidthProperty, _width, 0.0);
            BeginAnimation(HeightProperty, _height, 0.0);
        }

        private void BeginAnimation(DependencyProperty dp, double value, double ms)
        {
            if (ms <= 0.0)
            {
                BeginAnimation(dp, null);
                SetValue(dp, value);
            }
            else
            {
                var doubleAnimation = new DoubleAnimation();
                doubleAnimation.To = value;
                doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
                doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                BeginAnimation(dp, doubleAnimation);
            }
        }


        private void Transform_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            TransformChanged?.Invoke(this, e);
        }




        [MemberNotNull(nameof(_content))]
        private void AttachContent(IPageFrameContent content)
        {
            if (_content == content) return;

            Debug.Assert(content.Activity == this.Activity);

            DetachContent();

            _content = content;
            _content.TransformChanged += Content_TransformChanged;
            _content.ContentSizeChanged += Content_ContentSizeChanged;
            _contentControl.Content = _content.Content;

            _textBlock.Text = _content.FrameRange.ToString();

            UpdateFrame();

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DetachContent()
        {
            if (_content is null) return;
            _contentControl.Content = null;
            _content.TransformChanged -= Content_TransformChanged;
            _content.ContentSizeChanged -= Content_ContentSizeChanged;
            _content.Dispose();
        }

        private void Content_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            UpdateFrame();
            TransformChanged?.Invoke(this, e);
        }

        private void Content_ContentSizeChanged(object? sender, EventArgs e)
        {
            ContentSizeChanged?.Invoke(this, EventArgs.Empty);
        }


        // ##
        public Rect GetContentRect()
        {
            return _content.GetContentRect();
        }

        public Point TranslateContentToCanvasPoint(Point point)
        {
            var x = X + Width * 0.5 + point.X;
            var y = Y + Height * 0.5 + point.Y;
            return new Point(x, y);
        }

        /// <summary>
        /// フレームサイズの更新
        /// </summary>
        public void UpdateFrame()
        {
            // TODO: アンカーのみアニメ時間を適用しないようにする
            var duration = Duration; // 0.0

            if (_disposedValue) return;
            if (_content is null) return;

            var old = Rect;

            var size = _content.GetFrameSize();
            var horizontalDulation = size.Width > old.Width ? duration : 0.0;
            var verticalDulation = size.Height > old.Height ? duration : 0.0;
            SetWidth(size.Width, horizontalDulation);
            SetHeight(size.Height, verticalDulation);

            //_transformTextBlock.Text = $"{(int)_pageFrame.Angle}, {_pageFrame.Scale:f2}";
            //Debug.WriteLine($"Container.Update: {FrameRange}, {HorizontalAlignment}");

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    break;
                case HorizontalAlignment.Right:
                    SetX(old.Left - (Width - old.Width) * 1.0, horizontalDulation);
                    break;
                default:
                    SetX(old.Left - (Width - old.Width) * 0.5, horizontalDulation);
                    break;
            }

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    break;
                case VerticalAlignment.Bottom:
                    SetY(old.Top - (Height - old.Height) * 1.0, verticalDulation);
                    break;
                default:
                    SetY(old.Top - (Height - old.Height) * 0.5, verticalDulation);
                    break;
            }
        }

        public void ResetLayout()
        {
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            SetX(-Width * 0.5, 0.0);
            SetY(-Height * 0.5, 0.0);
        }
    }
}
