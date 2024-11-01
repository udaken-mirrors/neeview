using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView.Text.Json
{
    /// <summary>
    /// TimeSpanを文字列に変換する
    /// </summary>
    public sealed class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s == null) return new TimeSpan();

            return TimeSpan.Parse(s, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
