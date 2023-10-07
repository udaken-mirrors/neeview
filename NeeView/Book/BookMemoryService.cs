//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ブックのメモリ管理
    /// </summary>
    public class BookMemoryService : BindableBase, IBookMemoryService, IDisposable
    {
        private readonly MemoryPool _contentPool = new();
        private readonly MemoryPool _pictureSourcePool = new();
        private bool _disposedValue;

        public static long LimitSize => (long)Config.Current.Performance.CacheMemorySize * 1024 * 1024;

        public long TotalSize => _contentPool.TotalSize + _pictureSourcePool.TotalSize;

        public long ContentSize => _contentPool.TotalSize;
        public long ViewSize => _pictureSourcePool.TotalSize;

        public bool IsFull => TotalSize >= LimitSize;


        public BookMemoryService()
        {
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _contentPool.Dispose();
                    _pictureSourcePool.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void AddPageContent(IMemoryElement content)
        {
            if (_disposedValue) return;

            _contentPool.Add(content);
            Trace($"AddPageContent: {TotalSize / 1024 / 1024} MB");
            RaisePropertyChanged("");
        }

        public void AddPictureSource(IMemoryElement pictureSource)
        {
            if (_disposedValue) return;

            _pictureSourcePool.Add(pictureSource);
            Trace($"AddPictureSource: {TotalSize / 1024 / 1024} MB");
            RaisePropertyChanged("");
        }

        public void Cleanup()
        {
            if (_disposedValue) return;

            _contentPool.Cleanup(LimitSize - _pictureSourcePool.TotalSize);
            if (IsFull)
            {
                Trace($"Cleanup.PictureSource");
                _pictureSourcePool.Cleanup(LimitSize / 2);
            }
            RaisePropertyChanged("");
        }

        /// <summary>
        /// OutOfMemory 発生時の不活性メモリ開放処理
        /// </summary>
        public void CleanupDeep()
        {
            if (_disposedValue) return;

            _contentPool.Cleanup(0);
            _pictureSourcePool.Cleanup(0);
            RaisePropertyChanged("");
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(s, args)}");
        }
    }
}
