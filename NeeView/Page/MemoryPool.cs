using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public interface IMemoryElement
    {
        int Index { get; }
        bool IsMemoryLocked { get; }
        long GetMemorySize();
        void Unload();
    }

    public class MemoryPool
    {
        private List<IMemoryElement> _collection = new();
        private readonly object _lock = new();
        private int _referenceIndex;


        public long TotalSize { get; private set; }


        public void SetReference(int index)
        {
            _referenceIndex = index;
        }

        public void Add(IMemoryElement element)
        {
            lock (_lock)
            {
                ////Debug.WriteLine($"Add: {page}");
                _collection.Add(element);
                TotalSize = TotalSize + element.GetMemorySize();
            }
        }

        public void Cleanup(long limitSize)
        {
            List<IMemoryElement>? elements = null;
            List<IMemoryElement>? removes = null;

            lock (_lock)
            {
                long totalMemory = 0;

                elements = _collection
                    .Distinct()
                    .OrderByDescending(e => e.IsMemoryLocked)
                    .ThenBy(e => Math.Abs(e.Index - _referenceIndex))
                    .ToList();

                foreach (var (element, index) in elements.ToTuples())
                {
                    var size = element.GetMemorySize();
                    if (totalMemory + size > limitSize && !element.IsMemoryLocked)
                    {
                        removes = elements.Skip(index).ToList();
                        elements = elements.Take(index).ToList();
                        break;
                    }

                    totalMemory += size;
                }

                ////var removeCount = removes != null ? removes.Count : 0;
                ////var contentCount = elements.Count;
                ////Debug.WriteLine($"Cleanup1: {totalMemory / 1024 / 1024}MB, {removeCount}/{contentCount}");

                _collection = elements;
                TotalSize = totalMemory;
            }

            if (removes != null)
            {
                foreach (var element in removes)
                {
                    if (!element.IsMemoryLocked)
                    {
                        element.Unload();
                    }
                }
            }
        }

#if false
        public void Clear()
        {
            Cleanup(0);
        }
#endif
    }

}
