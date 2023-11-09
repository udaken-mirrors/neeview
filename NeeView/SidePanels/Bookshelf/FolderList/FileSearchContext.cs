#define LOCAL_DEBUG
using NeeLaboratory.IO.Search;
using System.Linq.Expressions;

namespace NeeView
{
    public class FileSearchContext : SearchContext
    {
        public FileSearchContext(SearchValueCache cache) : base(cache)
        {
            AddProfile(new DateSearchProfile());
            AddProfile(new FileSearchProfile());
            AddProfile(new DateSearchProfile());
        }
    }

}
