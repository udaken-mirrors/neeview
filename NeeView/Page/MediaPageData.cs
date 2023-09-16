namespace NeeView
{
    public class MediaPageData : IHasPath
    {
        public MediaPageData(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
