using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    public struct WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }
    }
}
