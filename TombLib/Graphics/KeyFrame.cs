using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Graphics
{
    public class KeyFrame
    {
        public BoundingBox BoundingBox { get; set; }
        public List<Matrix4x4> Translations { get; set; } = new List<Matrix4x4>();
        public List<Matrix4x4> Rotations { get; set; } = new List<Matrix4x4>();
    }
}
