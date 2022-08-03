using System;
using System.Windows.Media;

namespace NeeView
{
    public class PropertyMapColorConverter : PropertyMapConverter<Color>
    {
        public override string GetTypeName(Type typeToConvert)
        {
            return "\"#AARRGGBB\"";
        }

        public override object? Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue()?.ToString();
        }

        public override void Write(PropertyMapSource source, object? value, PropertyMapOptions options)
        {
            if (value is null) throw new NotSupportedException("Cannot convert from null");
            source.SetValue((Color?)new ColorConverter().ConvertFrom(value));
        }
    }


}
