using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO
{
    public class IOGeometrySettings
    {
        public bool SwapXY { get; set; }
        public bool SwapXZ { get; set; }
        public bool SwapYZ { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public bool FlipZ { get; set; }
        public bool FlipV { get; set; }
        public float Scale { get; set; }
        public bool ClampUV { get; set; }
        public bool PremultiplyUV { get; set; }

        public IOGeometrySettings()
        {
            Scale = 1.0f;
            ClampUV = true;
            FlipV = true;
            PremultiplyUV = true;
        }
    }
}
