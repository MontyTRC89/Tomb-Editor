using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class WadLight
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public float Intensity { get; set; }

        public WadLight()
        {
            Position = Vector3.Zero;
            Radius = 1024.0f;
            Intensity = 0.5f;
        }

        public WadLight(Vector3 position, float radius, float intensity)
        {
            Position = position;
            Radius = radius;
            Intensity = intensity;
        }
    }
}
