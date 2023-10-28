using NeeView.Collections.Generic;
using NeeLaboratory.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace NeeView
{
    public class SearchBookmarkFolderCollection : BookmarkFolderCollection
    {
        private readonly string _searchKeyword;
        private readonly bool _includeSubdirectories;

        public SearchBookmarkFolderCollection(QueryPath path, bool isOverlayEnabled, bool includeSubdirectories) : base(path, isOverlayEnabled)
        {
            _searchKeyword = Place.Search ?? throw new ArgumentException("Search keywords are required");
            _includeSubdirectories = includeSubdirectories;
        }

        protected override List<FolderItem> CreateFolderItemCollection(TreeListNode<IBookmarkEntry> root, CancellationToken token)
        {
            IEnumerable<TreeListNode<IBookmarkEntry>> collection = _includeSubdirectories ? root : root.Children;

            var items = collection
                .Select(e => CreateFolderItem(e))
                .WhereNotNull()
                .ToList();

            var searcher = new NeeLaboratory.IO.Search.SearchCore();
            var options = new NeeLaboratory.IO.Search.SearchOption();
            items = searcher.Search(_searchKeyword, options, items, token).Cast<FolderItem>().ToList();

            return items;
        }
    }
}
