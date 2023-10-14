namespace NeeView
{
    public class MediaPageData : IHasPath
    {
        public MediaPageData(string path, AudioInfo? audioInfo)
        {
            Path = path;
            AudioInfo = audioInfo;
        }

        public string Path { get; }
        public AudioInfo? AudioInfo { get; }
    }
}
