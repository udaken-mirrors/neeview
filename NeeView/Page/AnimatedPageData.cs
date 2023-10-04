namespace NeeView
{
    public class AnimatedPageData : IHasCache
    {
        public AnimatedPageData(MediaSource mediaSource)
        {
            MediaSource = mediaSource;
        }

        public MediaSource MediaSource { get; }

        public long CacheSize => MediaSource.CacheSize;

        public void ClearCache()
        {
            MediaSource.ClearCache();
        }
    }

}