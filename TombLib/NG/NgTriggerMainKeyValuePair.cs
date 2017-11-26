using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NgTriggerMainKeyValuePair
    {
        public int Key { get; set; }
        public string Value { get; set; }

        public Dictionary<int, NgTriggerKeyValuePair> ObjectList { get; set; }
        public Dictionary<int, NgTriggerKeyValuePair> TimerList { get; set; }
        public Dictionary<int, NgTriggerKeyValuePair> ExtraList { get; set; }

        public NgListKind ObjectListKind { get; set; }
        public NgListKind TimerListKind { get; set; }
        public NgListKind ExtraListKind { get; set; }

        public NgTriggerMainKeyValuePair(int key, string value)
        {
            this.Key = key;
            this.Value = value;

            ObjectList = new Dictionary<int, NgTriggerKeyValuePair>();
            TimerList = new Dictionary<int, NgTriggerKeyValuePair>();
            ExtraList = new Dictionary<int, NgTriggerKeyValuePair>();
        }

        public List<string> GetListForComboBox(NgParameterType param)
        {
            var list = new Dictionary<int, NgTriggerKeyValuePair>();
            switch (param)
            {
                case NgParameterType.Object: list = ObjectList; break;
                case NgParameterType.Timer: list = TimerList; break;
                case NgParameterType.Extra: list = ExtraList; break;
            }

            var result = new List<string>();
            foreach (var pair in list)
                result.Add(pair.Key + ": " + pair.Value);

            return result;
        }
    }
}
