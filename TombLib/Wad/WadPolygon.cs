using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.Utils;

namespace TombLib.Wad
{
    public struct WadPolygon
    {
        public WadPolygonShape Shape;
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;
        public TextureArea Texture;
        public byte ShineStrength;
    }
}
