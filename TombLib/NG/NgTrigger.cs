using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public enum NGParameterType
    {
        None,
        Timer,
        Object,
        Extra,
        Button
    }

    public class NgTriggerNode
    {
        public int Id { get; internal set; }
        public string Value { get; internal set; }

        public Dictionary<int, NgTriggerNode> ObjectList { get; set; }
        public Dictionary<int, NgTriggerNode> TimerList { get; set; }
        public Dictionary<int, NgTriggerNode> ExtraList { get; set; }
        public Dictionary<int, NgTriggerNode> ButtonList { get; set; }

        public NgListKind ObjectListKind { get; set; }
        public NgListKind TimerListKind { get; set; }
        public NgListKind ExtraListKind { get; set; }
        public NgListKind ButtonListKind { get; set; }

        public NgTriggerNode(int id, string value)
        {
            Id = id;
            Value = value;
            ObjectList = new Dictionary<int, NgTriggerNode>();
            TimerList = new Dictionary<int, NgTriggerNode>();
            ExtraList = new Dictionary<int, NgTriggerNode>();
            ButtonList = new Dictionary<int, NgTriggerNode>();
        }
    }
}
