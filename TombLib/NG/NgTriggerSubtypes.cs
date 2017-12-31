using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public class NgTriggerSubtypes
    {
        public SortedList<ushort, NgTriggerSubtype> MainList { get; private set; } = new SortedList<ushort, NgTriggerSubtype>();
    }
}
