using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class WadKeyFrame
    {
        public WadVector BoundingBox1;
        public WadVector BoundingBox2;
        public WadVector Offset;
        public WadKeyFrameRotation[] Angles;
    }
}
