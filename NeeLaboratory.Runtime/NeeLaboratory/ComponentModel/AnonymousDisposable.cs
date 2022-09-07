using System;
using System.Diagnostics;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// Disposeアクションのみを行うDisposableオブジェクト
    /// </summary>
    public class AnonymousDisposable : IDisposable
    {
        private readonly Action _action;
        private bool _disposedValue;

        public AnonymousDisposable(Action action)
        {
            Debug.Assert(action is not null);
            
            _action = action;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _action?.Invoke();
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
