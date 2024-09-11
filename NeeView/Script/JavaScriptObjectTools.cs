using System.Collections.Generic;

namespace NeeView
{
    public static class JavaScriptObjectTools
    {
        public static T? GetValue<T>(IDictionary<string, object?>? obj, string key)
            where T : class
        {
            if (obj is null) return null;
            return obj.TryGetValue(key, out var value) ? value as T : null;
        }
    }
}
