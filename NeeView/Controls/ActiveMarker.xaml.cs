using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// 描写フレーム頻度を上げるためのダミーアニメーション再生処理
    /// </summary>
    public partial class ActiveMarker : UserControl
    {
        public ActiveMarker()
        {
            InitializeComponent();

            // NOTE: 描写更新処理が実行されればよいので表示はしなくて良い
            this.Marker.Visibility = Visibility.Hidden;

            this.Loaded += (s, e) => UpdateActivity();
            this.IsVisibleChanged += (s, e) => UpdateActivity();
        }


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ActiveMarker), new PropertyMetadata(false, IsActiveProperty_Changed));


        private static void IsActiveProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActiveMarker control)
            {
                //Debug.WriteLine($"ActiveMarker.IsActive: {control.IsActive}");
                control.UpdateActivity();
            }
        }

        private void UpdateActivity()
        {
            if (IsActive && IsVisible)
            {
                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(2.0);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                this.MarkerRotate.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
            else
            {
                this.MarkerRotate.BeginAnimation(RotateTransform.AngleProperty, null, HandoffBehavior.SnapshotAndReplace);
            }
        }
    }

}
