using System;
using System.Collections;
using System.Collections.Generic;

namespace NeeView
{
    public class RingList<T> : IList<T>
    {
        private readonly T[] _items;
        private readonly int _capacity;
        private int _top;
        private int _count;

        public RingList(int capacity)
        {
            _capacity = capacity;
            _items = new T[capacity];
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || _count <= index) throw new IndexOutOfRangeException();
                var i = (_top + index) % _capacity;
                return _items[i];
            }
            set
            {
                if (index < 0 || _count <= index) throw new IndexOutOfRangeException();
                var i = (_top + index) % _capacity;
                _items[i] = value;
            }
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            var index = (_top + _count) % _capacity;

            _items[index] = item;

            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                _top = (_top + 1) % _capacity;
            }
        }

        public void Clear()
        {
            _top = 0;
            _count = 0;
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _count; i++)
            {
                array[arrayIndex + i] = _items[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_items[i], item)) return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void RemoveLast()
        {
            _count = Math.Max(_count - 1, 0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

