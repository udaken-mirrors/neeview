using NeeLaboratory.IO.Search;

namespace NeeView
{
    public class FileSearchContext : SearchContext
    {
        public FileSearchContext(SearchValueCache cache) : base(cache)
        {
            AddProfile(new DateSearchProfile());
            AddProfile(new BookSearchProfile());
        }
    }

}
