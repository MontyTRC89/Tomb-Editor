using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Utils
{
    public class DictionaryWhere<K, V> : IDictionary<K, V>, IReadOnlyDictionary<K, V>
    {
        public IDictionary<K, V> BaseDictionary { get; }
        public Func<KeyValuePair<K, V>, bool> Filter { get; }
        public DictionaryWhere(IDictionary<K, V> baseDictionary, Func<KeyValuePair<K, V>, bool> filter)
        {
            BaseDictionary = baseDictionary;
            Filter = filter;
        }

        public V this[K key]
        {
            get
            {
                V value = BaseDictionary[key];
                if (!Filter(new KeyValuePair<K, V>(key, value)))
                    throw new KeyNotFoundException("Key does not match the filter criterium.");
                return value;
            }
            set
            {
                if (!Filter(new KeyValuePair<K, V>(key, value)))
                    throw new KeyNotFoundException("Key does not match the filter criterium.");
                BaseDictionary[key] = value;
            }
        }

        public int Count
        {
            get
            {
                int result = 0;
                foreach (KeyValuePair<K, V> entry in BaseDictionary)
                    result += Filter(entry) ? 1 : 0;
                return result;
            }
        }

        public bool IsReadOnly => BaseDictionary.IsReadOnly;

        private struct KeyCollection : ICollection<K>
        {
            private readonly DictionaryWhere<K, V> _parent;
            public KeyCollection(DictionaryWhere<K, V> parent) { _parent = parent; }
            public int Count => _parent.Count;
            public bool IsReadOnly => true;
            public void Add(K item) { throw new NotSupportedException(); }
            public void Clear() { throw new NotSupportedException(); }
            public bool Contains(K item) => _parent.ContainsKey(item);
            public IEnumerator<K> GetEnumerator() => _parent.Select(entry => entry.Key).GetEnumerator();
            public bool Remove(K item) { throw new NotSupportedException(); }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void CopyTo(K[] array, int arrayIndex)
            {
                foreach (KeyValuePair<K, V> entry in _parent)
                    array[arrayIndex++] = entry.Key;
            }
        }
        public ICollection<K> Keys => new KeyCollection(this);
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;

        private struct ValueCollection : ICollection<V>
        {
            private readonly DictionaryWhere<K, V> _parent;
            public ValueCollection(DictionaryWhere<K, V> parent) { _parent = parent; }
            public int Count => _parent.Count;
            public bool IsReadOnly => true;
            public void Add(V item) { throw new NotSupportedException(); }
            public void Clear() { throw new NotSupportedException(); }
            public bool Contains(V item)
            {
                foreach (KeyValuePair<K, V> entry in _parent)
                    if (entry.Value.Equals(item))
                        return true;
                return false;
            }
            public IEnumerator<V> GetEnumerator() => _parent.Select(entry => entry.Value).GetEnumerator();
            public bool Remove(V item) { throw new NotSupportedException(); }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void CopyTo(V[] array, int arrayIndex)
            {
                foreach (KeyValuePair<K, V> entry in _parent)
                    array[arrayIndex++] = entry.Value;
            }
        }
        public ICollection<V> Values => new ValueCollection(this);
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

        public void Add(KeyValuePair<K, V> item)
        {
            if (!Filter(item))
                throw new KeyNotFoundException("Key does not match the filter criterium.");
            BaseDictionary.Add(item);
        }

        public void Add(K key, V value)
        {
            if (!Filter(new KeyValuePair<K, V>(key, value)))
                throw new KeyNotFoundException("Key does not match the filter criterium.");
            BaseDictionary.Add(key, value);
        }

        public void Clear()
        {
            foreach (KeyValuePair<K, V> entry in this.ToList())
                BaseDictionary.Remove(entry);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            if (!Filter(item))
                return false;
            return BaseDictionary.Contains(item);
        }

        public bool ContainsKey(K key)
        {
            V value;
            if (!BaseDictionary.TryGetValue(key, out value))
                return false;
            return Filter(new KeyValuePair<K, V>(key, value));
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<K, V> entry in this.ToList())
                array[arrayIndex++] = entry;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => BaseDictionary.Where(Filter).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(KeyValuePair<K, V> item)
        {
            if (!Filter(item))
                return false;
            return BaseDictionary.Remove(item);
        }

        public bool Remove(K key)
        {
            if (ContainsKey(key))
                return false;
            return BaseDictionary.Remove(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            if (!BaseDictionary.TryGetValue(key, out value))
                return false;
            if (!Filter(new KeyValuePair<K, V>(key, value)))
            {
                value = default(V);
                return false;
            }
            return true;
        }
    }

    public static class DictionaryWhereExtensions
    {
        public static DictionaryWhere<K, V> DicWhere<K, V>(this IDictionary<K, V> baseDictionary, Func<KeyValuePair<K, V>, bool> filter)
        {
            return new DictionaryWhere<K, V>(baseDictionary, filter);
        }
    }
}
