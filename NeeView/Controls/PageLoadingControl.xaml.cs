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
    /// PageLoadingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageLoadingControl : UserControl
    {
        public PageLoadingControl()
        {
            InitializeComponent();
            this.SetBinding(IsActiveProperty, new Binding(nameof(PageLoadingViewModel.IsActive)));
            this.MessageTextBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(PageLoadingViewModel.Message)));
            Update();
        }


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(PageLoadingControl), new PropertyMetadata(false, IsActiveProperty_Changed));


        private static void IsActiveProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageLoadingControl control)
            {
                control.Update();
            }
        }


        private void Update()
        {
            if (IsActive)
            {
                this.Root.Opacity = 0;
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)) { BeginTime = TimeSpan.FromSeconds(0.2) };
                this.Root.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);
                this.Root.Visibility = Visibility.Visible;

                this.ProgressRing.IsActive = true;
            }
            else
            {
                this.Root.BeginAnimation(UIElement.OpacityProperty, null, HandoffBehavior.SnapshotAndReplace);
                this.Root.Visibility = Visibility.Collapsed;

                this.ProgressRing.IsActive = false;
            }
        }
    }
}
