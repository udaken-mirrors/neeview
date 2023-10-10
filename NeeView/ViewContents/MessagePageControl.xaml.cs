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
    /// MessagePageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MessagePageControl : UserControl
    {
        public MessagePageControl(FileViewData context)
        {
            InitializeComponent();

            this.FileCard.Icon = context.ImageSource;
            this.FileCard.ArchiveEntry = context.Entry;

            this.MessageTextBlock.Text = context.Message;
        }

        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register(
            "DefaultBrush",
            typeof(Brush),
            typeof(MessagePageControl),
            new FrameworkPropertyMetadata(Brushes.White, new PropertyChangedCallback(OnDefaultBrushChanged)));

        public Brush DefaultBrush
        {
            get { return (Brush)GetValue(DefaultBrushProperty); }
            set { SetValue(DefaultBrushProperty, value); }
        }

        private static void OnDefaultBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
