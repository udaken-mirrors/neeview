//#define LOCAL_DEBUG

using System;
using System.Diagnostics;

namespace NeeView
{
    public class PreExtractMemory
    {
        public class Key : IDisposable
        {
            private PreExtractMemory? _man;
            private readonly long _size;
            private bool _disposedValue;

            public Key(PreExtractMemory man, long size)
            {
                _man = man;
                _size = size;
            }

            public long Size => _size;

            public void Detach()
            {
                _man = null;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                    }
                    _man?.Close(this);
                    _man = null;
                    _disposedValue = true;
                }
            }

            ~Key()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }


        static PreExtractMemory() => Current = new PreExtractMemory();
        public static PreExtractMemory Current { get; }


        private long _size;


        private PreExtractMemory()
        {
        }


        public long Size => _size;
        public long Capacity => (long)Config.Current.Performance.PreExtractSolidSize * 1024 * 1024;


        public bool IsFull(long offsetSize = 0)
        {
            return Capacity < Size + offsetSize;
        }


        public Key Open(long size)
        {
            var key = new Key(this, size);
            _size += key.Size;
            Trace($"Open: {key.Size:N0}byte: {Size:N0}/{Capacity:N0}");
            return key;
        }

        public void Close(Key key)
        {
            key.Detach();
            _size -= key.Size;
            Trace($"Close: {key.Size:N0}byte: {Size:N0}/{Capacity:N0}");
        }

        #region LOCAL_DEBUG

        [Conditional("LOCAL_DEBUG")]
        private static void Trace(string message)
        {
            Debug.WriteLine($"{nameof(PreExtractMemory)}: {message}");
        }

        [Conditional("LOCAL_DEBUG")]
        private static void Trace(string format, params object[] args)
        {
            Debug.WriteLine($"{nameof(PreExtractMemory)}: {string.Format(format, args)}");
        }

        #endregion LOCAL_DEBUG
    }

}

