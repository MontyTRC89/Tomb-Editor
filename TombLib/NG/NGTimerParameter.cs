using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NGTimerParameter
    {
        public int Id { get; internal set; }
        public string Description { get; internal set; }
        public Dictionary<int, NGExtraParameter> ExtraParameters { get; private set; }

        public NGTimerParameter(int id, string description)
        {
            Id = id;
            Description = description;
            ExtraParameters = new Dictionary<int, NGExtraParameter>();
        }
    }
}
