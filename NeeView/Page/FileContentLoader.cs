using NeeLaboratory.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class FileContentLoader : IContentLoader
    {
        public FileContentLoader(FileContent content)
        {
        }

#pragma warning disable CS0067
        public event EventHandler? Loaded;
#pragma warning restore CS0067

        public IDisposable SubscribeLoaded(EventHandler handler)
        {
            Loaded += handler;
            return new AnonymousDisposable(() => Loaded -= handler);
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task LoadContentAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task LoadThumbnailAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void UnloadContent()
        {
        }
    }
}
