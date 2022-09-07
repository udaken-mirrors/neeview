using System;
using System.Globalization;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public class SafeValueConverter<T> : IValueConverter
    {
        private readonly IValueConverter _converter;

        public SafeValueConverter()
        {
            _converter = Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Int32 => new SafeIntegerValueConverter(),
                TypeCode.Double => new SafeDoubleValueConverter(),
                _ => throw new NotSupportedException(),
            };
        }


        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_converter is null) throw new InvalidOperationException();

            return _converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
