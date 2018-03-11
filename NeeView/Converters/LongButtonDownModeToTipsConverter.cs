﻿using System;
using System.Windows.Data;

namespace NeeView
{
    //  長押し操作Tips表示用コンバータ
    [ValueConversion(typeof(LongButtonDownMode), typeof(string))]
    public class LongButtonDownModeToTipsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is LongButtonDownMode ? ((LongButtonDownMode)value).ToTips() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
