using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// PanelListThumbnailImage.xaml の相互作用ロジック
    /// </summary>
    public partial class PanelListThumbnailImage : UserControl
    {
        public PanelListThumbnailImage()
        {
            InitializeComponent();
            this.Root.DataContext = this;

            this.Unloaded += PanelListThumbnailImage_Unloaded;
        }

        private void PanelListThumbnailImage_Unloaded(object sender, RoutedEventArgs e)
        {
            ThumbnailBitmap?.Dispose();
        }


        public IThumbnail Thumbnail
        {
            get { return (IThumbnail)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(PanelListThumbnailImage), new PropertyMetadata(null, OnThumbnailPropertyChanged));

        private static void OnThumbnailPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PanelListThumbnailImage control)
            {
                control.UpdateThumbnailBitmap();
            }
        }


        public ThumbnailBitmap? ThumbnailBitmap
        {
            get { return (ThumbnailBitmap)GetValue(ThumbnailBitmapProperty); }
            set { SetValue(ThumbnailBitmapProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailBitmapProperty =
            DependencyProperty.Register("ThumbnailBitmap", typeof(ThumbnailBitmap), typeof(PanelListThumbnailImage), new PropertyMetadata(null));


        private void UpdateThumbnailBitmap()
        {
            var thumbnail = Thumbnail;

            if (ThumbnailBitmap?.Thumbnail == thumbnail) return;

            ThumbnailBitmap?.Dispose();
            ThumbnailBitmap = null;

            if (thumbnail is null) return;
            ThumbnailBitmap = new ThumbnailBitmap(thumbnail);
        }
    }



    public class BooleanToThumbnailStretchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.ThumbnailItemProfile.ImageStretch;
            }
            else
            {
                return System.Windows.Media.Stretch.Uniform;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BooleanToThumbnailViewboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.ThumbnailItemProfile.Viewbox;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BooleanToThumbnailAlignmentYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.ThumbnailItemProfile.AlignmentY;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ThumbnaiBackgroundBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Brush brush)
            {
                if (brush is SolidColorBrush solidColorBrush && solidColorBrush.Color.A != 0)
                {
                    return brush;
                }
                else
                {
                    return Config.Current.Panels.ThumbnailItemProfile.Background;
                }
            }
              
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThumbnailProfileToolTopEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Config.Current.Panels.ThumbnailItemProfile.IsImagePopupEnabled;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
