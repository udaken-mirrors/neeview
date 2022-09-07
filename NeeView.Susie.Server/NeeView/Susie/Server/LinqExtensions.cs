using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView.Susie.Server
{
    // NOTE: NeeLaboratory.Runtimeのものを使いたいが、そのためだけに参照を追加するのは現状では微妙なのでここで定義
    internal static class LinqExtensions
    {
        internal static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
            where TSource : class
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            return source.Where(x => x != null)!;
        }
    }
}
