using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class MouseDirectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (context != null && context.Instance != null)
                {
                    return IsDefined((MouseDirection)context.Instance);
                }
            }
            return false;
        }


        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object source)
        {
            if (source != null && source is string s)
            {
                string mouseActionToken = s.Trim();
                mouseActionToken = mouseActionToken.ToUpper(CultureInfo.InvariantCulture);
                if (mouseActionToken == String.Empty)
                {
                    return MouseDirection.None;
                }

                switch (mouseActionToken)
                {
                    case "U": return MouseDirection.Up;
                    case "D": return MouseDirection.Down;
                    case "L": return MouseDirection.Left;
                    case "R": return MouseDirection.Right;
                    case "C": return MouseDirection.Click;
                }
            }
            throw GetConvertFromException(source);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            if (destinationType == typeof(string) && value != null)
            {
                MouseDirection key = (MouseDirection)value;
                if (IsDefined(key))
                {
                    switch (key)
                    {
                        case MouseDirection.None: return string.Empty;
                        case MouseDirection.Up: return "U";
                        case MouseDirection.Down: return "D";
                        case MouseDirection.Left: return "L";
                        case MouseDirection.Right: return "R";
                        case MouseDirection.Click: return "C";
                    }
                }
                throw new InvalidEnumArgumentException(nameof(value), (int)key, typeof(MouseAction));
            }
            throw GetConvertToException(value, destinationType);
        }

        public static bool IsDefined(MouseDirection key)
        {
            return (int)key >= (int)MouseDirection.None && (int)key <= (int)MouseDirection.Click;
        }
    }
}
