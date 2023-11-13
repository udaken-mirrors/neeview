using NeeView.Collections.Generic;
using NeeLaboratory.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using NeeLaboratory.IO.Search;

namespace NeeView
{
    public class SearchBookmarkFolderCollection : BookmarkFolderCollection
    {
        private readonly string _searchKeyword;
        private readonly bool _includeSubdirectories;
        private readonly Searcher _searcher;

        public SearchBookmarkFolderCollection(QueryPath path, bool isOverlayEnabled, bool includeSubdirectories) : base(path, isOverlayEnabled)
        {
            _searchKeyword = Place.Search ?? throw new ArgumentException("Search keywords are required");
            _includeSubdirectories = includeSubdirectories;

            var searchContext = new SearchContext()
                .AddProfile(new DateSearchProfile())
                .AddProfile(new SizeSearchProfile())
                .AddProfile(new BookSearchProfile());
            _searcher = new Searcher(searchContext);
        }

        protected override List<FolderItem> CreateFolderItemCollection(TreeListNode<IBookmarkEntry> root, CancellationToken token)
        {
            IEnumerable<TreeListNode<IBookmarkEntry>> collection = _includeSubdirectories ? root : root.Children;

            var items = collection
                .Select(e => CreateFolderItem(e))
                .WhereNotNull()
                .ToList();

            items = _searcher.Search(_searchKeyword, items, token).Cast<FolderItem>().ToList();
            return items;
        }
    }
}
