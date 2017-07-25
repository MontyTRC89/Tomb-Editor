using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadTextureSamplePoint
    {
        public byte X;
        public byte Y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadTextureSample
    {
        public WadTextureSampleType Type;
        public byte X;
        public byte Y;
        public short Page;
        public sbyte FlipX;
        public byte Width;
        public sbyte FlipY;
        public byte Height;
        public WadTextureSamplePoint[] UV;
    }

    public enum WadTextureSampleType : byte
    {
        CoreWad = 0,
        UV = 1
    }
}
