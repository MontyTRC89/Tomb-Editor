using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NGExtraParameter
    {
        public int Id { get; internal set; }
        public string Description { get; internal set; }

        public NGExtraParameter(int id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}
