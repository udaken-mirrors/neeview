using System;
using System.Windows.Data;

namespace NeeView
{
    // コンバータ：背景フラグ
    [ValueConversion(typeof(BackgroundType), typeof(bool))]
    public class BackgroundStyleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = parameter as string ?? throw new ArgumentException();
            BackgroundType mode0 = (BackgroundType)value;
            BackgroundType mode1 = (BackgroundType)Enum.Parse(typeof(BackgroundType), s);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
