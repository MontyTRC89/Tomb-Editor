using System.Collections.Generic;
using System.Linq;

namespace TombEditor.Compilers
{
    public static class Extensions
    {
        public static int ReferenceIndexOf<T>(this IEnumerable<T> enumerable, T needle)
        {
            if (needle == null)
                return -1;
            
            int i = 0;
            foreach (var t in enumerable.Where(e => e != null))
            {
                if (ReferenceEquals(t, needle))
                    return i;

                ++i;
            }

            return -1;
        }
    }
}
