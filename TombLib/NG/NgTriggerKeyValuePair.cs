using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NgTriggerKeyValuePair
    {
        public int Key { get; set; }
        public string Value { get; set; }

        public NgTriggerKeyValuePair(int key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
