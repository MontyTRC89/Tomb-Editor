using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadPolygon
    {
        public WadPolygonShape Shape { get; }
        public List<int> Indices { get; } = new List<int>();
        public TextureArea Texture { get; set; }
        public byte ShineStrength { get; set; }

        public WadPolygon(WadPolygonShape shape)
        {
            Shape = shape;
        }
    }
}
