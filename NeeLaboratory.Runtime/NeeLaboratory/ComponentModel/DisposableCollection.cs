using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// DisposableオブジェクトをまとめてDisposeする
    /// </summary>
    public class DisposableCollection : IDisposable
    {
        private List<IDisposable> _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            Debug.Assert(disposable is not null);

            if (disposable is null) return;

            ThrowIfDisposed();

            _disposables.Add(disposable);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

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
                    foreach (var disposable in _disposables.Reverse<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
