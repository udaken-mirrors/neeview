using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IBitmapPageSourceLoader
    {
        Task<BitmapPageSource> LoadAsync(ArchiveEntry entry, bool createPctureInfo, CancellationToken token);
    }
}
