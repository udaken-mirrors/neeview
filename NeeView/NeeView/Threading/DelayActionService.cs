using System;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView.Threading
{
    public class DelayActionService : IDisposable
    {
        public static DelayActionService Current { get; } = new();

        private readonly CancellationTokenSource _tokenSource = new();
        private bool _disposedValue;

        private DelayActionService()
        {
            ApplicationDisposer.Current.Add(this);
        }

        public void DelayAction(int delay, Action action)
        {
            if (_disposedValue) return;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, _tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }
                action.Invoke();
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tokenSource.Cancel();
                }
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
