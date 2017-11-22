using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NgTriggersDefinitions
    {
        public static NgTriggerNode ActionTrigger { get; set; }
        public static NgTriggerNode ConditionTrigger { get; set; }
        public static NgTriggerNode FlipEffectTrigger { get; set; }
        public static NgTriggerNode TimerFieldTrigger { get; set; }
    }
}
