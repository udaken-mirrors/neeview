using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public interface IContentLoader : IDisposable
    {
        event EventHandler? Loaded;
        
        IDisposable SubscribeLoaded(EventHandler handler);

        Task LoadContentAsync(CancellationToken token);

        void UnloadContent();

        Task LoadThumbnailAsync(CancellationToken token);
    }

}
