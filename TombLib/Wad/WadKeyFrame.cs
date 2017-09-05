using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class WadKeyFrame
    {
        public BoundingBox BoundingBox;
        public Vector3 Offset;
        public WadKeyFrameRotation[] Angles;
    }
}
