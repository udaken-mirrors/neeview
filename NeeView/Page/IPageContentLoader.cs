using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public interface IPageContentLoader
    {
        bool IsLoaded { get; }

        Task LoadContentAsync(CancellationToken token);
    }

}
