using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadTextureSample
    {
        public short X;
        public short Y;
        public short Page;
        public sbyte FlipX;
        public byte Width;
        public sbyte FlipY;
        public byte Height;
    }
}
