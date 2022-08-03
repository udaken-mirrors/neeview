using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView.Text.Json
{
    /// <summary>
    /// Sizeを文字列に変換する
    /// </summary>
    public sealed class JsonSizeConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s == null) return new Size();

            var instance = new SizeConverter().ConvertFromInvariantString(s) as Size?;
            if (instance == null) throw new InvalidCastException();

            return instance.Value;
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
