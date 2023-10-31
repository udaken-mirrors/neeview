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
    /// SimpleProgressBar.xaml の相互作用ロジック
    /// </summary>
    public partial class SimpleProgressBar : UserControl
    {
        public SimpleProgressBar()
        {
            InitializeComponent();
            PART_Root.SizeChanged += (s, e) => Update();
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SimpleProgressBar), new PropertyMetadata(0.0, ValueProperty_Changed));

        private static void ValueProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SimpleProgressBar control)
            {
                control.Update();
            }
        }


        private void Update()
        {
            PART_Bar.Width = PART_Root.ActualWidth * Value;
        }
    }
}
