using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;

namespace NeeView
{
    public interface IPageThumbnailLoader
    {
        bool IsThumbnailValid { get; }

        Task<ImageSource?> LoadThumbnailAsync(CancellationToken token);
    }

}
