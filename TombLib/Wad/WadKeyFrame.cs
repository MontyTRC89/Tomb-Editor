using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class WadKeyFrame
    {
        public WadVertex BoundingBox1;
        public WadVertex BoundingBox2;
        public WadVertex Offset;
        public WadKeyFrameRotation[] Angles;
    }
}
