using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TombLib.Utils
{
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(T left, T right)
        {
            return ReferenceEquals(left, right);
        }
    }
}
