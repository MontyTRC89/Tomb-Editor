using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TombLib.Graphics
{
    public class KeyFrame
    {
        public BoundingBox BoundingBox { get; set; }
        public List<Matrix4x4> Translations { get; set; } = new List<Matrix4x4>();
        public List<Matrix4x4> Rotations { get; set; } = new List<Matrix4x4>();
    }
}
