using System;
using System.Collections;
using System.Collections.Generic;
using TimestampType = System.UInt32;

namespace TombLib.Utils
{
    public class Cache<KeyT, ValueT> : IEnumerable<KeyValuePair<KeyT, ValueT>>, IDisposable
    {
        private const int cleanupFactorDiv32 = 25;
        private TimestampType useCounter;
        private class Entry
        {
            public TimestampType _lastUsedTimeStamp;
            public ValueT _value;
        }
        private readonly Dictionary<KeyT, Entry> _availableItems;

        public Func<KeyT, ValueT> GenerateValue { get; set; }
        public Action<ValueT> DisposeValue { get; set; }
        public int MaxCachedCount { get; set; }

        public Cache(int maxCachedCount, Func<KeyT, ValueT> generateValue)
            : this(maxCachedCount, generateValue,
                  typeof(IDisposable).IsAssignableFrom(typeof(ValueT)) ?
                  (Action<ValueT>)(value => { ((IDisposable)value).Dispose(); }) : null)
        {}

        public Cache(int maxCachedCount, Func<KeyT, ValueT> generateValue, Action<ValueT> disposeValue)
        {
            _availableItems = new Dictionary<KeyT, Entry>(maxCachedCount);
            MaxCachedCount = maxCachedCount;
            GenerateValue = generateValue;
            DisposeValue = disposeValue;
        }

        public ValueT this[KeyT key]
        {
            get
            {
                // Try finding existing value
                Entry result;
                if (_availableItems.TryGetValue(key, out result))
                {
                    result._lastUsedTimeStamp = useCounter++;
                    return result._value;
                }

                // Clean up if necessary
                if (_availableItems.Count + 1 >= MaxCachedCount)
                    Cleanup();

                // Add value
                result = new Entry { _lastUsedTimeStamp = useCounter++, _value = GenerateValue(key) };
                _availableItems.Add(key, result);
                return result._value;
            }
        }

        public void Dispose()
        {
            if (DisposeValue != null)
                foreach (var item in _availableItems)
                    DisposeValue(item.Value._value);
        }

        public void Reset()
        {
            Dispose();
            _availableItems.Clear();
        }

        private void Cleanup()
        {
            int itemCount = _availableItems.Count;
            int reducedCount = itemCount * cleanupFactorDiv32 / 64 + 1;
            if (reducedCount >= _availableItems.Count)
            {
                Reset();
                return;
            }

            // Sort entries after elapsed time
            TimestampType[] elapsedTimes = new TimestampType[itemCount];
            KeyT[] keys = new KeyT[itemCount];
            {
                int i = 0;
                foreach (var item in _availableItems)
                {
                    elapsedTimes[i] = useCounter - item.Value._lastUsedTimeStamp;
                    keys[i++] = item.Key;
                }
            }
            Array.Sort(elapsedTimes, keys);

            // Remove first entries
            if (DisposeValue != null)
                for (int i = 0; i < reducedCount; ++i)
                    DisposeValue(_availableItems[keys[i]]._value);
            for (int i = 0; i < reducedCount; ++i)
                _availableItems.Remove(keys[i]);
        }

        public bool Remove(KeyT key)
        {
            Entry entry;
            if (!_availableItems.TryGetValue(key, out entry))
                return false;
            DisposeValue(entry._value);
            return _availableItems.Remove(key);
        }

        public void Clear()
        {
            foreach (Entry entry in _availableItems.Values)
                DisposeValue(entry._value);
            _availableItems.Clear();
        }

        public IEnumerator<KeyValuePair<KeyT, ValueT>> GetEnumerator()
        {
            foreach (var availableItem in _availableItems)
                yield return new KeyValuePair<KeyT, ValueT>(availableItem.Key, availableItem.Value._value);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
