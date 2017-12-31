using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.NG;

namespace NgXmlBuilder
{
    public class NgTriggerNode
    {
        public ushort Id { get; internal set; }
        public string Value { get; internal set; }

        public Dictionary<ushort, NgTriggerNode> TargetList { get; set; }
        public Dictionary<ushort, NgTriggerNode> TimerList { get; set; }
        public Dictionary<ushort, NgTriggerNode> ExtraList { get; set; }

        public NgParameterKind TargetListKind { get; set; }
        public NgParameterKind TimerListKind { get; set; }
        public NgParameterKind ExtraListKind { get; set; }

        public NgTriggerNode(ushort id, string value)
        {
            Id = id;
            Value = value;
            TargetList = new Dictionary<ushort, NgTriggerNode>();
            TimerList = new Dictionary<ushort, NgTriggerNode>();
            ExtraList = new Dictionary<ushort, NgTriggerNode>();
        }
    }
}
