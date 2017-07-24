using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadPolygon
    {
        public Shape Shape;
        public ushort V1;
        public ushort V2;
        public ushort V3;
        public ushort V4;
        public ushort Texture;
        public byte Attributes;
        public byte Unknown;
        public int Unused1;
        public int Unused2;
        public int Unused3;
        public int Unused4;
    }
}
