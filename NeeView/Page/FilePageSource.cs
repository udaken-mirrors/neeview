using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class FilePageSource : PageSource
    {
        public FilePageSource(ArchiveEntry entry) : base(entry)
        {
        }


        public override long DataSize => 0;


        protected override async Task LoadAsyncCore(CancellationToken token)
        {
            NVDebug.AssertMTA();

            // ArchvieFileの場合はTempFile化
            var fileProxy = ArchiveEntry.ExtractToTemp(); // TODO: async化
            SetData(fileProxy.Path, null);

            await Task.CompletedTask;
        }

        protected override void UnloadCore()
        {
            //throw new NotImplementedException();
        }
    }
}
