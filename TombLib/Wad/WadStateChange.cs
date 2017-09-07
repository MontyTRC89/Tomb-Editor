using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{

    public class WadStateChange
    {
        public ushort StateId;
        public uint NumDispatches;
        public List<WadAnimDispatch> Dispatches;
    }
}
