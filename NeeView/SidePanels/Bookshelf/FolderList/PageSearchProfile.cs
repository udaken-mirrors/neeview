using NeeLaboratory.IO.Search;

namespace NeeView
{
    public class PageSearchProfile : SearchProfile
    {
        public PageSearchProfile()
        {
            Options.Add(PageSearchPropertyProfiles.Playlist);
            Options.Add(PageSearchPropertyProfiles.Meta);
            Options.Add(PageSearchPropertyProfiles.Rating);

            Alias.Add("/playlist", new() { "/p.playlist", "true" });
            Alias.Add("/title", new() { "/p.meta.title" });
            Alias.Add("/subject", new() { "/p.meta.subject" });
            Alias.Add("/rating", new() { "/p.rating" });
            Alias.Add("/tags", new() { "/p.meta.tags" });
            Alias.Add("/comments", new() { "/p.meta.comments" });
        }
    }

    public static class PageSearchPropertyProfiles
    {
        public static SearchPropertyProfile Playlist { get; } = new SearchPropertyProfile("playlist", BooleanSearchValue.Default);
        public static SearchPropertyProfile Meta { get; } = new SearchPropertyProfile("meta", StringSearchValue.Default);
        public static SearchPropertyProfile Rating { get; } = new SearchPropertyProfile("rating", IntegerSearchValue.Default);
    }
}
