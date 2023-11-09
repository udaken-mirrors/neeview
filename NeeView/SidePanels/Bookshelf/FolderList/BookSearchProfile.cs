#define LOCAL_DEBUG
using NeeLaboratory.IO.Search;

namespace NeeView
{
    public class BookSearchProfile : SearchProfile
    {
        public BookSearchProfile()
        {
            Options.Add(BookSearchPropertyProfiles.IsBookmark);
            Options.Add(BookSearchPropertyProfiles.IsHistory);

            Alias.Add("/isbookmark", new() { "/p.isbookmark" });
            Alias.Add("/ishistory", new() { "/p.ishistory" });
        }
    }

    public static class BookSearchPropertyProfiles
    {
        public static SearchPropertyProfile IsBookmark { get; } = new SearchPropertyProfile("isbookmark", BooleanSearchValue.Default);
        public static SearchPropertyProfile IsHistory { get; } = new SearchPropertyProfile("ishistory", BooleanSearchValue.Default);
    }


    public class FileSearchProfile : SearchProfile
    {
        public FileSearchProfile()
        {
            Options.Add(FileSearchPropertyProfiles.IsDirectory);
        }
    }

    public static class FileSearchPropertyProfiles
    {
        public static SearchPropertyProfile IsDirectory { get; } = new SearchPropertyProfile("isdir", BooleanSearchValue.Default);
    }

}
