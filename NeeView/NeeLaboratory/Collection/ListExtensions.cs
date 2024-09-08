using System.Collections.Generic;

namespace NeeLaboratory.Collection
{
    public static class ListExtensions
    {
        public static void SetSize<T>(this List<T> self, int size)
        {
            if (self.Count <= size) return;
            self.RemoveRange(size, self.Count - size);
        }

        public static void AddIfNotNull<T>(this List<T> self, T? value)
        {
            if (value is null) return;
            self.Add(value);
        }
    }
}
