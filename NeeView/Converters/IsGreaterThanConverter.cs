using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：より大きい値ならTrue
    public class IsGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = parameter as string ?? throw new ArgumentException();
            var v = System.Convert.ToDouble(value);
            var compareValue = double.Parse(s, CultureInfo.InvariantCulture);
            return v > compareValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
