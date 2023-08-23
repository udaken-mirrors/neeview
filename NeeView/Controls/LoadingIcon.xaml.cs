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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// LoadingIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class LoadingIcon : UserControl
    {
        public LoadingIcon()
        {
            InitializeComponent();

            this.NowLoadingMark.Opacity = 0.0;
            UpdateLoading();
        }


        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(LoadingIcon), new PropertyMetadata(true, IsLoadingProperty_Changed));


        private static void IsLoadingProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingIcon control)
            {
                control.UpdateLoading();
            }
        }

        private void UpdateLoading()
        {
            if (IsLoading)
            {
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                ani.BeginTime = TimeSpan.FromSeconds(1.0);
                this.NowLoadingMark.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(2.0);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
            else
            {
                var ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                this.NowLoadingMark.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 45;
                aniRotate.Duration = TimeSpan.FromSeconds(0.25);
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
        }

    }
}
