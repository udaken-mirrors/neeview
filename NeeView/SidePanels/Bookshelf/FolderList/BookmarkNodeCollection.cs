using NeeView.Collections.Generic;
using System.Collections.Generic;
using System.Globalization;

namespace NeeView
{
    /// <summary>
    /// BookmarkNode collection for DataObject
    /// </summary>
    public class BookmarkNodeCollection : List<TreeListNode<IBookmarkEntry>>
    {
        public static readonly string Format = FormatVersion.CreateFormatName(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), nameof(BookmarkNodeCollection));

        public BookmarkNodeCollection()
        {
        }

        public BookmarkNodeCollection(IEnumerable<TreeListNode<IBookmarkEntry>> collection) : base(collection)
        {
        }
    }

    public static class BookmarkNodeCollectionExtensions
    {
        public static BookmarkNodeCollection ToBookmarkNodeCollection(this IEnumerable<TreeListNode<IBookmarkEntry>> collection)
        {
            return new BookmarkNodeCollection(collection);
        }
    }
}
