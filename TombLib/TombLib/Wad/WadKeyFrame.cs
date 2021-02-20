using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Wad
{
    public class WadKeyFrame
    {
        public BoundingBox BoundingBox { get; set; }
        public Vector3 Offset { get; set; }
        public List<WadKeyFrameRotation> Angles { get; private set; } = new List<WadKeyFrameRotation>();

        public WadKeyFrame Clone()
        {
            var result = (WadKeyFrame)MemberwiseClone();
            result.Angles = new List<WadKeyFrameRotation>(Angles);
            return result;
        }
    }
}
