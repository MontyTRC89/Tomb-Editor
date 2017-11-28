using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NgTriggerMainKeyValuePair : NgTriggerKeyValuePair
    {
        public Dictionary<int, NgTriggerKeyValuePair> ObjectList { get; set; }
        public Dictionary<int, NgTriggerKeyValuePair> TimerList { get; set; }
        public Dictionary<int, NgTriggerKeyValuePair> ExtraList { get; set; }

        public NgListKind ObjectListKind { get; set; }
        public NgListKind TimerListKind { get; set; }
        public NgListKind ExtraListKind { get; set; }

        public bool HasObjectList { get { return ObjectListKind != NgListKind.Empty || ObjectList.Count != 0; } }
        public bool HasTimerList { get { return TimerListKind != NgListKind.Empty || TimerList.Count != 0; } }
        public bool HasExtraList { get { return ExtraListKind != NgListKind.Empty || ExtraList.Count != 0; } }

        public NgTriggerMainKeyValuePair(int key, string value)
            : base(key, value)
        {
            ObjectList = new Dictionary<int, NgTriggerKeyValuePair>();
            TimerList = new Dictionary<int, NgTriggerKeyValuePair>();
            ExtraList = new Dictionary<int, NgTriggerKeyValuePair>();
        }

        public NgTriggerKeyValuePair[] GetListForComboBox(NgParameterType param)
        {
            var list = new Dictionary<int, NgTriggerKeyValuePair>();
            switch (param)
            {
                case NgParameterType.Object: list = ObjectList; break;
                case NgParameterType.Timer: list = TimerList; break;
                case NgParameterType.Extra: list = ExtraList; break;
            }

            var result = new List<NgTriggerKeyValuePair>();
            foreach (var pair in list)
                result.Add(pair.Value);

            return result.ToArray();
        }

        public override string ToString()
        {
            return Key + ": " + Value;
        }
    }
}
