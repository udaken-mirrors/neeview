using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class PageNameConverter : IValueConverter
    {
        public Style? SmartTextStyle { get; set; }
        public Style? DefaultTextStyle { get; set; }
        public Style? NameOnlyTextStyle { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var format = (PageNameFormat)value;
                return format switch
                {
                    PageNameFormat.Smart => SmartTextStyle,
                    PageNameFormat.NameOnly => NameOnlyTextStyle,
                    _ => DefaultTextStyle,
                };
            }
            catch { }

            return DefaultTextStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
