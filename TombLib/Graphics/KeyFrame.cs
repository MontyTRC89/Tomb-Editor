using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace TombLib.Graphics
{
    public class KeyFrame
    {
        public BoundingBox BoundingBox;
        public List<Matrix> Translations = new List<Matrix>();
        public List<Matrix> Rotations = new List<Matrix>();
    }
}
