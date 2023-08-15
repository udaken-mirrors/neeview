using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ブックのメモリ管理
    /// </summary>
    public class BookMemoryService : BindableBase, IBookMemoryService
    {
        private readonly MemoryPool _contentPool = new();
        private readonly MemoryPool _pictureSourcePool = new();

        public static long LimitSize => (long)Config.Current.Performance.CacheMemorySize * 1024 * 1024;

        public long TotalSize => _contentPool.TotalSize + _pictureSourcePool.TotalSize;

        public bool IsFull => TotalSize >= LimitSize;


        [Obsolete]
        public void SetReference(int index)
        {
            _contentPool.SetReference(index);
            _pictureSourcePool.SetReference(index);
        }

        public void AddPageContent(IMemoryElement content)
        {
            _contentPool.Add(content);
            Debug.WriteLine($"BookMemoryService: AddPageContent: {TotalSize / 1024 / 1024} MB");
            RaisePropertyChanged("");
        }

        public void AddPictureSource(IMemoryElement pictureSource)
        {
            _pictureSourcePool.Add(pictureSource);
            Debug.WriteLine($"BookMemoryService: AddPictureSource: {TotalSize / 1024 / 1024} MB");
            RaisePropertyChanged("");
        }

        public void Cleanup()
        {
            _contentPool.Cleanup(LimitSize - _pictureSourcePool.TotalSize);
            if (IsFull)
            {
                Debug.WriteLine($"BookMemoryService: Cleanup.PictureSource");
                _pictureSourcePool.Cleanup(0);
            }
            RaisePropertyChanged("");
        }

        /// <summary>
        /// OutOfMemory 発生時の不活性メモリ開放処理
        /// </summary>
        public void CleanupDeep()
        {
            _contentPool.Cleanup(0);
            _pictureSourcePool.Cleanup(0);
        }
    }
}
