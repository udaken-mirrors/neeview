using System;
using System.Windows;

namespace NeeView
{
    public class PropertyMapSizeConverter : PropertyMapConverter<Size>
    {
        public override string GetTypeName(Type typeToConvert)
        {
            return "\"width,height\"";
        }

        public override object? Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue()?.ToString();
        }

        public override void Write(PropertyMapSource source, object? value, PropertyMapOptions options)
        {
            if (value is null) throw new NotSupportedException("Cannot convert from null");
            source.SetValue((Size?)new SizeConverter().ConvertFrom(value));
        }
    }


}
