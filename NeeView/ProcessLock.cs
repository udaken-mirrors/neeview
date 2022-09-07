using NeeLaboratory.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// プロセス間セマフォ
    /// </summary>
    public static class ProcessLock
    {
        private const string _semaphoreLabel = "NeeView.s0001";
        private static readonly Semaphore _semaphore;

        static ProcessLock()
        {
            _semaphore = new Semaphore(1, 1, _semaphoreLabel, out bool isCreateNew);
            Debug.WriteLine($"Process Semaphore isCreateNew: {isCreateNew}");
        }

        public static IDisposable Lock()
        {
            // 10秒待っても取得できないときは例外
            if (_semaphore.WaitOne(1000 * 10) != true)
            {
                throw new TimeoutException("Cannot sync with other NeeViews. There may be a problem with NeeView already running.");
            }

            return new Handler(_semaphore);
        }

        public static async Task<IDisposable> LockAsync(int timeout)
        {
            await _semaphore.AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeout));
            return new Handler(_semaphore);
        }


        private sealed class Handler : IDisposable
        {
            private readonly Semaphore _semaphore;
            private bool _disposedValue;

            public Handler(Semaphore semaphore)
            {
                _semaphore = semaphore;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }

                    _semaphore.Release();

                    _disposedValue = true;
                }
            }

            ~Handler()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
