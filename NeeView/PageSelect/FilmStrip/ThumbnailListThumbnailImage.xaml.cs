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
        }


        public IThumbnail Thumbnail
        {
            get { return (IThumbnail)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(ThumbnailListThumbnailImage), new PropertyMetadata(null));
    }
}
