using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// Viewbox を指定して表示領域を制限したコントロールを表示する
    /// </summary>
    public partial class CropControl : UserControl
    {
        public CropControl()
        {
            InitializeComponent();
            this.SizeChanged += CropControl_SizeChanged;
            this.IsTabStop = false;
            this.Focusable = false;
        }


        public FrameworkElement? Target
        {
            get { return (FrameworkElement?)GetValue(CroppedContentProperty); }
            set { SetValue(CroppedContentProperty, value); }
        }

        public static readonly DependencyProperty CroppedContentProperty =
            DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(CropControl), new PropertyMetadata(null, CroppedContent_Changed));

        private static void CroppedContent_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CropControl control)
            {
                control.TargetSocket.Content = (FrameworkElement?)e.NewValue;
                control.Update();
            }
        }

        public Rect Viewbox
        {
            get { return (Rect)GetValue(ViewboxProperty); }
            set { SetValue(ViewboxProperty, value); }
        }

        public static readonly DependencyProperty ViewboxProperty =
            DependencyProperty.Register(nameof(Viewbox), typeof(Rect), typeof(CropControl), new PropertyMetadata(new Rect(0,0,1,1), Viewbox_Changed));

        private static void Viewbox_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CropControl control)
            {
                control.Update();
            }
        }


        private void CropControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;

            var imageWidth = width / Viewbox.Width;
            var imageHeight = height / Viewbox.Height;
            var imageLeft = -Viewbox.Left * imageWidth;
            var imageTop = -Viewbox.Top * imageHeight;

            var element = this.TargetSocket;
            element.Width = imageWidth;
            element.Height = imageHeight;
            Canvas.SetLeft(element, imageLeft);
            Canvas.SetTop(element, imageTop);
        }
    }
}
