using System;
using System.Globalization;

namespace NeeView
{
    public class PropertyMapDefaultConverter : PropertyMapConverter<object>
    {
        public override object? Read(PropertyMapSource source, Type typeToConvert, PropertyMapOptions options)
        {
            return source.GetValue();
        }

        public override void Write(PropertyMapSource source, object? value, PropertyMapOptions options)
        {
            // TODO: Culture はこれでいいのか？
            source.SetValue(value != null ? Convert.ChangeType(value, source.PropertyInfo.PropertyType, CultureInfo.InvariantCulture) : null);
        }
    }


}
