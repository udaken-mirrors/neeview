namespace NeeView
{
    public class AnimatedPageData : IHasPath
    {
        public AnimatedPageData(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }

}