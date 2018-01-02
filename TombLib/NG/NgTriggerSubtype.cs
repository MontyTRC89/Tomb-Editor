using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;

namespace TombLib.NG
{
    public class NgTriggerSubtype : TriggerParameterUshort
    {
        public NgTriggerSubtype(ushort key, string value)
            : base(key, value)
        {}

        public NgParameterRange Target { get; set; }
        public NgParameterRange Timer { get; set; }
        public NgParameterRange Extra { get; set; }
    }
}
