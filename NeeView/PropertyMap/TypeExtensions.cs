using System;

namespace NeeView
{
    public static class TypeExtensions
    {
        public static string ToManualString(this Type type)
        {
            if (type == typeof(void))
            {
                return "void";
            }

            if (type.IsEnum)
            {
                return "enum";
            }

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.Int32 => "int",
                TypeCode.Double => "double",
                TypeCode.String => "string",
                _ => type.ToString(),
            };
        }

        /// <summary>
        /// Get default value from Type.
        /// </summary>
        /// <param name="type">target type</param>
        /// <returns>default value</returns>
        public static object? GetDefaultValue(this Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }
    }
}
