using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Utils
{
    public static class CollectionUtils
    {
        public static int IndexOf<T>(this IEnumerable<T> list, Predicate<T> test, int skip = 0, int @default = -1)
        {
            int i = 0;
            foreach (T element in list)
            {
                if (i >= skip)
                    if (test(element))
                        return i;
                ++i;
            }
            return @default;
        }

        public static int ReferenceIndexOf<T>(this IEnumerable<T> list, T needle)
        {
            if (needle == null)
                return -1;

            int i = 0;
            foreach (T element in list)
            {
                if (ReferenceEquals(element, needle))
                    return i;
                ++i;
            }

            return -1;
        }

        public static TValue TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue))
        {
            TValue result;
            if (@this.TryGetValue(key, out result))
                return result;
            @this.Add(key, @default);
            return @default;
        }

        public static TValue TryGetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue))
        {
            TValue result;
            if (@this.TryGetValue(key, out result))
                return result;
            return @default;
        }


        public static void Resize<T>(this List<T> list, int newCount, T newElement = default(T))
        {
            int oldCount = list.Count;
            if (newCount < oldCount)
                list.RemoveRange(newCount, oldCount - newCount);
            else if (newCount > oldCount)
            {
                list.Capacity = newCount;
                list.AddRange(Enumerable.Repeat(newElement, newCount - oldCount));
            }
        }

        public static IEnumerable<T> Unwrap<T>(this T[,] array)
        {
            for (int x = 0; x < array.GetLength(0); ++x)
                for (int y = 0; y < array.GetLength(1); ++y)
                    yield return array[x, y];
        }

        public static T TryGet<T>(this T[] array, int index0) where T : class
        {
            if (index0 < 0 || index0 >= array.GetLength(0))
                return null;
            return array[index0];
        }

        public static T TryGet<T>(this T[,] array, int index0, int index1) where T : class
        {
            if (index0 < 0 || index0 >= array.GetLength(0))
                return null;
            if (index1 < 0 || index1 >= array.GetLength(1))
                return null;
            return array[index0, index1];
        }

        public static T TryGet<T>(this T[,,] array, int index0, int index1, int index2) where T : class
        {
            if (index0 < 0 || index0 >= array.GetLength(0))
                return null;
            if (index1 < 0 || index1 >= array.GetLength(1))
                return null;
            if (index2 < 0 || index2 >= array.GetLength(2))
                return null;
            return array[index0, index1, index2];
        }

        public static T FindFirstAfterWithWrapAround<T>(this IEnumerable<T> list, Func<T, bool> IsPrevious, Func<T, bool> Matches) where T : class
        {
            bool ignoreMatches = true;

            // Search for matching objects after the previous one
            foreach (T obj in list)
            {
                if (ignoreMatches)
                {
                    if (IsPrevious(obj))
                        ignoreMatches = false;
                    continue;
                }

                // Does it match
                if (Matches(obj))
                    return obj;
            }

            // Search for any matching objects
            foreach (T obj in list)
                if (Matches(obj))
                    return obj;

            return null;
        }

        public static T2 AddAndReturn<T, T2>(this IList<T> list, T2 item) where T2 : T
        {
            list.Add(item);
            return item;
        }
    }
}
