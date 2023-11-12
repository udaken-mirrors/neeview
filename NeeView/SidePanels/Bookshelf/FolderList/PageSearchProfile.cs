using NeeLaboratory.IO.Search;

namespace NeeView
{
    public class PageSearchProfile : SearchProfile
    {
        public PageSearchProfile()
        {
            Options.Add(PageSearchPropertyProfiles.Meta);

            Alias.Add("/tags", new() { "/p.meta.tags" });
        }
    }

    public static class PageSearchPropertyProfiles
    {
        public static SearchPropertyProfile Meta { get; } = new SearchPropertyProfile("meta", StringSearchValue.Default);
    }
}
