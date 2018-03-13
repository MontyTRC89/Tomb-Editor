using System.Collections.Generic;

namespace TombLib.Wad
{
    public class WadStateChange
    {
        public ushort StateId { get; set; }
        public uint NumDispatches { get; set; }
        public List<WadAnimDispatch> Dispatches { get; private set; } = new List<WadAnimDispatch>();

        public WadStateChange Clone()
        {
            var stateChange = (WadStateChange)MemberwiseClone();
            stateChange.Dispatches = new List<WadAnimDispatch>(Dispatches);
            return stateChange;
        }
    }
}
