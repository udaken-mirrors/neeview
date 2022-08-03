using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeLaboratory.Linq
{
    // NOTE: NeeLaboratory.Runtimeのものを使いたいが、そのためだけに参照を追加するのは現状では微妙なのでここで定義
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> MyWhereNotNull<TSource>(this IEnumerable<TSource?> source)
            where TSource : class
        {
            if (source is null) throw new ArgumentNullException();
            return source.Where(x => x != null)!;
        }
    }
}
