using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// キーによる高速検索に対応したコレクション
    /// </summary>
    public class MappedCollection<TKey, TValue> : IEnumerable<TValue>
        where TKey : notnull
        where TValue : IHasKey<TKey>
    {
        private readonly ObservableCollection<TValue> _collection;
        private readonly Dictionary<TKey, TValue> _map;


        public MappedCollection()
        {
            _collection = new ObservableCollection<TValue>();
            _map = new Dictionary<TKey, TValue>();
        }

        public MappedCollection(IEnumerable<TValue> collection)
        {
            _collection = new ObservableCollection<TValue>(collection);
            _map = _collection.ToDictionary(e => e.Key, e => e);
        }


        /// <summary>
        /// ObservableCollectionとして使用したいときのプロパティ
        /// </summary>
        public ObservableCollection<TValue> Collection => _collection;

        public Dictionary<TKey, TValue>.KeyCollection Keys => _map.Keys;


        public TValue this[TKey key]
        {
            get
            {
                return _map[key];
            }
            set
            {
                if (_map.TryGetValue(key, out var item))
                {
                    if (EqualityComparer<TValue>.Default.Equals(item, value)) return;
                    _map[key] = value;
                    _collection.Remove(item);
                    _collection.Add(value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey(key);
        }

        public void Add(TKey key, TValue item)
        {
            if (_map.ContainsKey(key)) throw new ArgumentException($"The new key '{key}' is already in the dictionary.");

            _map.Add(key, item);
            _collection.Add(item);
        }

        public bool Remove(TKey key)
        {
            if (_map.TryGetValue(key, out var item))
            {
                _map.Remove(key);
                _collection.Remove(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Add(TValue item)
        {
            Add(item.Key, item);
        }

        public bool Remove(TValue item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
