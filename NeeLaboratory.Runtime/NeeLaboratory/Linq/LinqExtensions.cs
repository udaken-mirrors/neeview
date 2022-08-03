using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.Linq
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
            where TSource : class
        {
            if (source is null) throw new ArgumentNullException();
            return source.Where(x => x != null)!;
        }
    }
}
