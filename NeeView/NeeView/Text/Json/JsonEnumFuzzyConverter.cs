﻿using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView.Text.Json
{
    /// <summary>
    /// Enumを文字列に変換する。
    /// 文字列がEnumに変換できないときはdefault値とみなす
    /// </summary>
    public class JsonEnumFuzzyConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var instance = Activator.CreateInstance(
                typeof(EnumFuzzyConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: Array.Empty<object>(),
                culture: null);

            var converter = instance as JsonConverter ?? throw new InvalidOperationException("Cannot create JsonConverter");

            return converter;
        }


        public class EnumFuzzyConverter<T> : JsonConverter<T>
            where T : struct, Enum
        {
            public override bool CanConvert(Type type)
            {
                return type.IsEnum;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return Enum.TryParse(reader.GetString(), out T value) ? value : default;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
