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

        /// <summary>
        /// Checks if a sample of elements from the list satisfies a given predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The source list to sample from.</param>
        /// <param name="sampleSize">The maximum number of elements to sample.</param>
        /// <param name="predicate">The condition to check for each sampled element.</param>
        /// <returns>
        /// <see langword="true" /> if all sampled elements satisfy the predicate; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// This is a less accurate alternative to LINQ's <c>.All()</c> method, but much faster
        /// for large collections when checking only a representative sample is sufficient.<br />
        /// It samples elements evenly across the entire list rather than checking every element.
        /// </remarks>
        public static bool SampleSatisfies<T>(this IList<T> list, int sampleSize, Predicate<T> predicate)
        {
            bool result = true;
            int samplesToCheck = Math.Min(sampleSize, list.Count);

            // Sample evenly from the list to get a representative subset
            for (int i = 0; i < samplesToCheck; i++)
            {
                int index = i * list.Count / samplesToCheck;
                T item = list[index];

                if (!predicate(item))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
