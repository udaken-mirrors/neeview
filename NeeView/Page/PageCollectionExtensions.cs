using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
#if false
    public class SelectedItemChangedEventArgs : EventArgs
    {
        public SelectedItemChangedEventArgs(bool fromOutsize)
        {
            // 外界から
            FromOutsize = fromOutsize;
        }

        public bool FromOutsize { get; }
    }
#endif

    public static class PageCollectionExtensions
    {
        public static List<Page> PageRangeToPages(this IReadOnlyList<Page> pages, PageRange range)
        {
            var indexs = Enumerable.Range(range.Min.Index, range.Max.Index - range.Min.Index + 1);
            return indexs.Where(e => pages.IsValidIndex(e)).Select(e => pages[e]).ToList();
        }

        public static bool IsValidIndex(this IReadOnlyList<Page> pages, int index)
        {
            return 0 <= index && index < pages.Count;
        }
    }
}
