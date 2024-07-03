using System.Collections.Generic;

namespace TombLib.Utils
{
    public static class Swap
    {
        public static void Do<T>(ref T first, ref T second)
            => (second, first) = (first, second);

        public static void TrySwap<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key1, TKey key2)
        {
            if (dictionary.ContainsKey(key1) && dictionary.ContainsKey(key2))
                (dictionary[key2], dictionary[key1]) = (dictionary[key1], dictionary[key2]);
            else if (dictionary.ContainsKey(key1) && !dictionary.ContainsKey(key2))
            {
                dictionary[key2] = dictionary[key1];
                dictionary.Remove(key1);
            }
            else if (!dictionary.ContainsKey(key1) && dictionary.ContainsKey(key2))
            {
                dictionary.Add(key1, dictionary[key2]);
                dictionary.Remove(key2);
            }
        }
    }
}