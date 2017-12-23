using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public WadKeyFrame Clone()
        {
            var keyframe = new WadKeyFrame();

            keyframe.BoundingBox = new BoundingBox(new Vector3(BoundingBox.Minimum.X, BoundingBox.Minimum.Y, BoundingBox.Minimum.Z),
                                                   new Vector3(BoundingBox.Maximum.X, BoundingBox.Maximum.Y, BoundingBox.Maximum.Z));
            keyframe.Offset = new Vector3(Offset.X, Offset.Y, Offset.Z);

            foreach (var angle in Angles)
                keyframe.Angles.Add(angle.Clone());

            return keyframe;
        }
    }
}
