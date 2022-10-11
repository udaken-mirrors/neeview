using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// プレビューページ用コントロール
    /// </summary>
    public class PrintPreviewControl : System.Windows.Controls.Primitives.UniformGrid
    {
        /// <summary>
        /// プレビューデータ
        /// </summary>
        public IEnumerable<FrameworkElement> ItemsSource
        {
            get { return (IEnumerable<FrameworkElement>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<FrameworkElement>), typeof(PrintPreviewControl), new PropertyMetadata(null, ItemsSource_Changed));

        //
        private static void ItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PrintPreviewControl control)
            {
                control.Refresh();
            }
        }


        /// <summary>
        /// 更新
        /// </summary>
        public void Refresh()
        {
            this.Children.Clear();

            if (ItemsSource == null) return;

            foreach (var child in ItemsSource)
            {
                var grid = new Grid();
                grid.Background = Brushes.White;
                grid.Margin = new Thickness(10);
                grid.Children.Add(child);

                this.Children.Add(grid);
            }
        }
    }
}
