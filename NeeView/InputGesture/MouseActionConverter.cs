using System;
using System.ComponentModel;
using System.Globalization;

namespace NeeView
{
    public class MouseActionConverter : TypeConverter
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
                    return (MouseActionConverter.IsDefinedMouseAction((MouseAction)context.Instance));
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
                    return MouseAction.None;
                }

                switch (mouseActionToken)
                {
                    case "LEFTCLICK": return MouseAction.LeftClick;
                    case "RIGHTCLICK": return MouseAction.RightClick;
                    case "MIDDLECLICK": return MouseAction.MiddleClick;
                    case "WHEELCLICK": return MouseAction.WheelClick;
                    case "LEFTDOUBLECLICK": return MouseAction.LeftDoubleClick;
                    case "RIGHTDOUBLECLICK": return MouseAction.RightDoubleClick;
                    case "MIDDLEDOUBLECLICK": return MouseAction.MiddleDoubleClick;
                    case "XBUTTON1CLICK": return MouseAction.XButton1Click;
                    case "XBUTTON1DOUBLECLICK": return MouseAction.XButton1DoubleClick;
                    case "XBUTTON2CLICK": return MouseAction.XButton2Click;
                    case "XBUTTON2DOUBLECLICK": return MouseAction.XButton2DoubleClick;
                    case "WHEELUP": return MouseAction.WheelUp;
                    case "WHEELDOWN": return MouseAction.WheelDown;
                    case "WHEELLEFT": return MouseAction.WheelLeft;
                    case "WHEELRIGHT": return MouseAction.WheelRight;
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
                MouseAction key = (MouseAction)value;
                if (MouseActionConverter.IsDefinedMouseAction(key))
                {
                    switch (key)
                    {
                        case MouseAction.None: return string.Empty;
                        case MouseAction.LeftClick: return "LeftClick";
                        case MouseAction.RightClick: return "RightClick";
                        case MouseAction.MiddleClick: return "MiddleClick";
                        case MouseAction.WheelClick: return "WheelClick";
                        case MouseAction.LeftDoubleClick: return "LeftDoubleClick";
                        case MouseAction.RightDoubleClick: return "RightDoubleClick";
                        case MouseAction.MiddleDoubleClick: return "MiddleDoubleClick";
                        case MouseAction.XButton1Click: return "XButton1Click";
                        case MouseAction.XButton1DoubleClick: return "XButton1DoubleClick";
                        case MouseAction.XButton2Click: return "XButton2Click";
                        case MouseAction.XButton2DoubleClick: return "XButton2DoubleClick";
                        case MouseAction.WheelUp: return "WheelUp";
                        case MouseAction.WheelDown: return "WheelDown";
                        case MouseAction.WheelLeft: return "WheelLeft";
                        case MouseAction.WheelRight: return "WheelRight";
                    }
                }
                throw new InvalidEnumArgumentException(nameof(value), (int)key, typeof(MouseAction));
            }
            throw GetConvertToException(value, destinationType);
        }


        internal static bool IsDefinedMouseAction(MouseAction mouseAction)
        {
            return (mouseAction >= MouseAction.None && mouseAction <= MouseAction.WheelRight);
        }
    }
}
