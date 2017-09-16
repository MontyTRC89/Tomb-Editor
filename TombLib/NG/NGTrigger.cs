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

    public class NGTrigger
    {
        public int Id { get; internal set; }
        public string Description { get; internal set; }
        public Dictionary<int, NGTimerParameter> TimerParameters { get; private set; }
        public Dictionary<int, NGExtraParameter> ExtraParameters { get; private set; }
        
        public NGTrigger(int id, string description)
        {
            Id = id;
            Description = description;
            TimerParameters = new Dictionary<int, NGTimerParameter>();
            ExtraParameters = new Dictionary<int, NGExtraParameter>();
        }
    }
}
