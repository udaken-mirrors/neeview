using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{

    public class MemoryPool
    {
        private record MemoryUnit(IMemoryElement Key, long Size) : IHasKey<IMemoryElement>;

        private LinkedDicionary<IMemoryElement, MemoryUnit> _collection = new();
        private readonly object _lock = new();


        public long TotalSize { get; private set; }


        [Obsolete]
        public void SetReference(int index)
        {
            // nop
        }

        public void Add(IMemoryElement element)
        {
            lock (_lock)
            {
                //Debug.WriteLine($"MemoryPool.Add: {element.Index}: {element.GetMemorySize()} Byte");
                Debug.Assert(element.GetMemorySize() > 0);

                var unit = _collection.Remove(element);
                if (unit is not null)
                {
                    TotalSize -= unit.Size;
                }

                unit = new MemoryUnit(element, element.GetMemorySize());
                _collection.AddLast(element, unit);
                TotalSize += unit.Size;
                AssertTotalSize();
            }
        }


        public void Cleanup(long limitSize)
        {
            lock (_lock)
            {
                int removeCount = 0;

                while (limitSize < TotalSize)
                {
                    // 古いものから削除を試みる。ロックされていたらそこで終了
                    var node = _collection.First;
                    if (node is null) break;

                    if (node.Value.Key.IsMemoryLocked)
                    {
                        break;
                    }
                    else
                    {
                        Remove(node);
                        removeCount++;
                    }
                }

                // [DEV]
                if (removeCount > 0)
                {
                    AssertTotalSize();
                }
            }
        }

        private void Remove(LinkedListNode<MemoryUnit> node)
        {
            lock (_lock)
            {
                _collection.Remove(node);
                TotalSize -= node.Value.Size;
                AssertTotalSize();

                node.Value.Key.Unload();
                //Debug.WriteLine($"MemoryPool.Remove: {node.Value.Key.Index}");
            }
        }



        [Conditional("DEBUG")]
        private void AssertTotalSize()
        {
            lock (_lock)
            {
                var totalSize = _collection.Select(e => e.Size).Sum();
                Debug.Assert(totalSize == TotalSize);
            }
        }
    }

}
