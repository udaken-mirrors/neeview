using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.Linq
{
    static class LinqExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            if (source is null) throw new ArgumentNullException();
            return source.Where(x => x != null)!;
        }
    }
}
