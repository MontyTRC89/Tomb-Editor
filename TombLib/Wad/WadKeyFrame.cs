using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class WadKeyFrame
    {
        public BoundingBox BoundingBox { get; set; }
        public Vector3 Offset { get; set; }
        public List<WadKeyFrameRotation> Angles { get; private set; }

        public WadKeyFrame()
        {
            Angles = new List<WadKeyFrameRotation>();
        }
    }
}
