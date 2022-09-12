using NeeLaboratory.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryからファイルに展開する。キャンセル可。
    /// </summary>
    public class ArchiveEntryExtractor : IDisposable
    {
        private readonly ArchiveEntry _entry;
        private readonly string _extractFileName;
        private readonly Task _task;
        private TempFile? _tempFile;
        private bool _disposedValue;


        public ArchiveEntryExtractor(ArchiveEntry entry)
        {
            _entry = entry;
            _extractFileName = Temporary.Current.CreateCountedTempFileName("arcv", Path.GetExtension(entry.EntryName));

            _task = Task.Run(() =>
            {
                //Debug.WriteLine($"## Start: {_extractFileName}");
                _entry.ExtractToFile(_extractFileName, false);
                //Debug.WriteLine($"## Completed: {_extractFileName}");
                _tempFile = new TempFile(_extractFileName);
            });

            _task.ContinueWith(async (t) =>
            {
                await Task.Delay(1000);
                //Debug.WriteLine($"## Expired.{(_tempFile is null ? 'o' : 'x')}: {_extractFileName}");
                Expired?.Invoke(this, _entry);
            });
        }


        public event EventHandler<ArchiveEntry>? Expired;


        public async Task<TempFile> WaitAsync(CancellationToken token)
        {
            ThrowIfDisposed();

            await _task.WaitAsync(token);

            var tempFile = Interlocked.Exchange(ref _tempFile, null);
            if (tempFile is null) throw new InvalidOperationException();

            //Debug.WriteLine($"## Export: {_extractFileName}");
            return tempFile;
        }


        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    var tempFile = Interlocked.Exchange(ref _tempFile, null);
                    tempFile?.Dispose();
                }

                //Debug.WriteLine($"## Disposed: {_extractFileName}");
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
