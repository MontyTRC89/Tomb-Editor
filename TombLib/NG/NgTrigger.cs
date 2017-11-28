using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NgTrigger
    {
        public Dictionary<int, NgTriggerMainKeyValuePair> MainList { get; private set; }

        public NgTrigger()
        {
            MainList = new Dictionary<int, NgTriggerMainKeyValuePair>();
        }

        public NgTriggerMainKeyValuePair[] GetListForComboBox()
        {
            var result = new List<NgTriggerMainKeyValuePair>();
            foreach (var pair in MainList)
                result.Add(pair.Value);

            return result.ToArray();
        }
    }
}
