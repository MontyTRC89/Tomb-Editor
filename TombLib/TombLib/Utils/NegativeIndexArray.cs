using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public class NegativeIndexArray<T>
    {
        private int _first;
        private int _last;
        private T[] _array;

        public NegativeIndexArray(int first, int last)
        {
            _first = first;
            _last = last;
            int size = -first + last + 1;
            _array = new T[size];
        }

        public int FirstIndex
        {
            get
            {
                return _first; 
            }
        }

        public int LastIndex
        {
            get
            {
                return _last;
            }
        }

        public T this[int index]
        {
            get
            {
                return _array[-_first + index];
            }
            set
            {
                _array[-_first + index] = value;
            }
        }
    }
}
