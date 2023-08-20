using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageThumbnail
    {
        private PageContent _content;

        public PageThumbnail(PageContent content)
        {
            _content = content;
        }

        public Thumbnail Thumbnail { get; } = new Thumbnail();

        public virtual async Task LoadThumbnailAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }



}
