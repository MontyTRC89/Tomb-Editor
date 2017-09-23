using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{

    public class WadStateChange
    {
        public ushort StateId { get; set; }
        public uint NumDispatches { get; set; }
        public List<WadAnimDispatch> Dispatches { get; private set; }

        public WadStateChange()
        {
            Dispatches = new List<WadAnimDispatch>();
        }

        public WadStateChange Clone()
        {
            var stateChange = new WadStateChange();

            stateChange.StateId = StateId;
            stateChange.NumDispatches = NumDispatches;
            stateChange.Dispatches = new List<WadAnimDispatch>();

            foreach (var dispatch in Dispatches)
                stateChange.Dispatches.Add(dispatch.Clone());

            return stateChange;
        }
    }
}
