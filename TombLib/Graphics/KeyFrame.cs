using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace TombLib.Graphics
{
    public class KeyFrame
    {
        public BoundingBox BoundingBox { get; set; }
        public List<Matrix> Translations { get; set; } = new List<Matrix>();
        public List<Matrix> Rotations { get; set; } = new List<Matrix>();
    }
}
