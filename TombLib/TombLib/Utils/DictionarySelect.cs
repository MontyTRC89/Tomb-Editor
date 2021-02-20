using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Utils
{
    public class DictionarySelect<K, V, V2> : IDictionary<K, V2>, IReadOnlyDictionary<K, V2>
    {
        public IDictionary<K, V> BaseDictionary { get; }
        public Func<KeyValuePair<K, V>, V2> Selector { get; }
        public DictionarySelect(IDictionary<K, V> baseDictionary, Func<KeyValuePair<K, V>, V2> selector)
        {
            BaseDictionary = baseDictionary;
            Selector = selector;
        }

        public V2 this[K key]
        {
            get { return Selector(new KeyValuePair<K, V>(key, BaseDictionary[key])); }
            set { throw new NotSupportedException(); }
        }

        public int Count => BaseDictionary.Count;

        public bool IsReadOnly => true;

        public ICollection<K> Keys => BaseDictionary.Keys;
        IEnumerable<K> IReadOnlyDictionary<K, V2>.Keys => Keys;

        private struct ValueCollection : ICollection<V2>
        {
            private readonly DictionarySelect<K, V, V2> _parent;
            public ValueCollection(DictionarySelect<K, V, V2> parent) { _parent = parent; }
            public int Count => _parent.Count;
            public bool IsReadOnly => true;
            public void Add(V2 item) { throw new NotSupportedException(); }
            public void Clear() { throw new NotSupportedException(); }
            public bool Contains(V2 item)
            {
                foreach (KeyValuePair<K, V2> entry in _parent)
                    if (entry.Value.Equals(item))
                        return true;
                return false;
            }
            public IEnumerator<V2> GetEnumerator() => _parent.Select(entry => entry.Value).GetEnumerator();
            public bool Remove(V2 item) { throw new NotSupportedException(); }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void CopyTo(V2[] array, int arrayIndex)
            {
                foreach (KeyValuePair<K, V2> entry in _parent)
                    array[arrayIndex++] = entry.Value;
            }
        }
        public ICollection<V2> Values => new ValueCollection(this);
        IEnumerable<V2> IReadOnlyDictionary<K, V2>.Values => Values;

        public void Add(KeyValuePair<K, V2> item)
        {
            throw new NotSupportedException();
        }

        public void Add(K key, V2 value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<K, V2> item)
        {
            V2 value;
            if (!TryGetValue(item.Key, out value))
                return false;
            return value.Equals(item);
        }

        public bool ContainsKey(K key) => BaseDictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<K, V2>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<K, V> entry in BaseDictionary)
                array[arrayIndex++] = new KeyValuePair<K, V2>(entry.Key, Selector(entry));
        }

        public IEnumerator<KeyValuePair<K, V2>> GetEnumerator()
        {
            foreach (KeyValuePair<K, V> entry in BaseDictionary)
                yield return new KeyValuePair<K, V2>(entry.Key, Selector(entry));
        }

        public bool Remove(KeyValuePair<K, V2> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(K key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(K key, out V2 value)
        {
            V tempValue;
            if (!BaseDictionary.TryGetValue(key, out tempValue))
            {
                value = default(V2);
                return false;
            }
            value = Selector(new KeyValuePair<K, V>(key, tempValue));
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class DictionarySelectExtensions
    {
        public static DictionarySelect<K, V, V2> DicSelect<V2, K, V>(this IDictionary<K, V> baseDictionary, Func<KeyValuePair<K, V>, V2> select)
        {
            return new DictionarySelect<K, V, V2>(baseDictionary, select);
        }
    }
}
