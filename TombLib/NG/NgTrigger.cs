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

    public class NgTrigger
    {
        public int Id { get; internal set; }
        public string Description { get; internal set; }
        public Dictionary<int, NgTimerParameter> TimerParameters { get; private set; }
        public Dictionary<int, NgExtraParameter> ExtraParameters { get; private set; }
        
        public NgTrigger(int id, string description)
        {
            Id = id;
            Description = description;
            TimerParameters = new Dictionary<int, NgTimerParameter>();
            ExtraParameters = new Dictionary<int, NgExtraParameter>();
        }
    }
}
