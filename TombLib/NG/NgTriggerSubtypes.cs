using System.Collections.Generic;

namespace TombLib.NG
{
    public class NgTriggerSubtypes
    {
        public SortedList<ushort, NgTriggerSubtype> MainList { get; private set; } = new SortedList<ushort, NgTriggerSubtype>();
    }
}
