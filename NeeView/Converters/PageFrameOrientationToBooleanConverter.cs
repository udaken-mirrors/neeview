using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView
{
    [ValueConversion(typeof(PageFrameOrientation), typeof(bool))]
    public class PageFrameOrientationToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = parameter as string ?? throw new ArgumentException();
            PageFrameOrientation mode0 = (PageFrameOrientation)value;
            PageFrameOrientation mode1 = (PageFrameOrientation)Enum.Parse(typeof(PageFrameOrientation), s);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
