using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadVector3
    {
        public float X;
        public float Y;
        public float Z;
    }
}
