using System;
using System.Globalization;

namespace NeeView
{
    public static class DateTimeExtensions
    {
        public static string ToFormatString(this DateTime dateTime)
        {
            return dateTime.ToString(Config.Current.System.DateTimeFormat);
        }
    }

    public static class DateTimeTools
    {
        public static string DateTimePattern => Config.Current.System.DateTimeFormat;

        public static string DefaultDateTimePattern
        {
            get
            {
                var info = DateTimeFormatInfo.CurrentInfo;
                return info.ShortDatePattern + " " + info.LongTimePattern;
            }
        }

        public static string DefaultDatePattern
        {
            get { return DateTimeFormatInfo.CurrentInfo.ShortDatePattern; }
        }
    }
}
