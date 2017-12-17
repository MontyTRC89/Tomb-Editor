using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public static class Swap
    {
        public static void Do<T>(ref T first, ref T second)
        {
            T temp = first;
            first = second;
            second = temp;
        }
    }
}
