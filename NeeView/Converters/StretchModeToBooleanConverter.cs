using System;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：ストレッチモードフラグ
    [ValueConversion(typeof(PageStretchMode), typeof(bool))]
    public class StretchModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = parameter as string ?? throw new ArgumentException();
            PageStretchMode mode0 = (PageStretchMode)value;
            PageStretchMode mode1 = (PageStretchMode)Enum.Parse(typeof(PageStretchMode), s);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
