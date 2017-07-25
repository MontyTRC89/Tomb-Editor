using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WadStateChange
    {
        public ushort StateId;
        public ushort NumDispatches;
        public WadAnimDispatch[] Dispatches;
    }
}
