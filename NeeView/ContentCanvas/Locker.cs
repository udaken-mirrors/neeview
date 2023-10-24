using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public class Locker
    {
        public class Key : IDisposable
        {
            private bool _disposedValue = false;


            public Key(Locker locker)
            {
                this.Locker = locker;
            }

            public Locker? Locker { get; set; }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        this.Locker?.Unlock(this);
                    }
                    _disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }


        private readonly object _lock = new();
        private int _lockCount;

        public bool IsLocked => _lockCount > 0;

        public Key Lock()
        {
            lock (_lock)
            {
                _lockCount++;
            }
            return new Key(this);
        }

        public void Unlock(Key key)
        {
            if (key.Locker == this)
            {
                key.Locker = null;
                lock (_lock)
                {
                    if (_lockCount > 0)
                    {
                        _lockCount--;
                    }
                }
            }
        }

        public void ForceUnlock()
        {
            lock (_lock)
            {
                _lockCount = 0;
            }
        }
    }
}
