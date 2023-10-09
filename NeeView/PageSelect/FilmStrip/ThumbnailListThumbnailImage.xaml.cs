using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// ThumbnailListThumbnailImage.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailListThumbnailImage : UserControl
    {
        public ThumbnailListThumbnailImage()
        {
            InitializeComponent();
            this.Root.DataContext = this;

            this.Unloaded += ThumbnailListThumbnailImage_Unloaded;
        }

        private void ThumbnailListThumbnailImage_Unloaded(object sender, RoutedEventArgs e)
        {
            ThumbnailBitmap?.Dispose();
        }


        public IThumbnail Thumbnail
        {
            get { return (IThumbnail)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(ThumbnailListThumbnailImage), new PropertyMetadata(null, OnThumbnailPropertyChanged));

        private static void OnThumbnailPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThumbnailListThumbnailImage control)
            {
                control.ThumbnailBitmap = new ThumbnailBitmap(control.Thumbnail);
            }
        }


        public ThumbnailBitmap ThumbnailBitmap
        {
            get { return (ThumbnailBitmap)GetValue(ThumbnailBitmapProperty); }
            set { SetValue(ThumbnailBitmapProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailBitmapProperty =
            DependencyProperty.Register("ThumbnailBitmap", typeof(ThumbnailBitmap), typeof(ThumbnailListThumbnailImage), new PropertyMetadata(null));
    }
}
