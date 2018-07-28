using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
