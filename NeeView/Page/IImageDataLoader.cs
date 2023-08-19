using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IImageDataLoader
    {
        Task<ImageData> LoadAsync(ArchiveEntry entry, bool createPctureInfo, CancellationToken token);
    }
}
