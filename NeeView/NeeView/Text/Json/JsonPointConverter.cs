using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView.Text.Json
{
    /// <summary>
    /// Pointを文字列に変換する
    /// </summary>
    public sealed class JsonPointConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s == null) return new Point();

            var instance = new PointConverter().ConvertFromInvariantString(s) as Point?;
            if (instance == null) throw new InvalidCastException();

            return instance.Value;
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
