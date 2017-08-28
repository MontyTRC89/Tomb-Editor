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
        // TODO: this structure will be refactored in the future (MontyTRC)
        public ushort V1;
        public ushort V2;
        public ushort V3;
        public ushort V4;
        //public ushort Texture;
        public byte Attributes;
        public byte Unknown;
        public int Unused1;
        public int Unused2;
        public int Unused3;
        public int Unused4;

        // Correct fields
        public Shape Shape;
        public List<int> Indices;
        public int Texture;  
        public byte ShineStrength;
        public bool Transparent;
    }
}
