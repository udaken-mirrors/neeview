using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class Locker
    {
        public class Key : IDisposable
        {
            public Locker? Locker { get; set; }

            public Key(Locker locker)
            {
                this.Locker = locker;
            }

            #region IDisposable Support
            private bool _disposedValue = false;

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
            }
            #endregion
        }

        private List<Key> _keys = new List<Key>();

        public bool IsLocked => _keys.Any();

        public Key Lock()
        {
            var key = new Key(this);
            _keys.Add(key);
            return key;
        }

        public void Unlock(Key key)
        {
            if (key.Locker == this)
            {
                _keys.Remove(key);
                key.Locker = null;
            }
        }
    }


}
