using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    internal class NgVectorPair
    {
        public object Key { get; set; }
        public string Value { get; set; }

        public NgVectorPair(object key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
