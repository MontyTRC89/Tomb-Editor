using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace TombLib.Graphics
{
    public class Bone
    {
        public string Name { get; set; }
        public Matrix Transform { get; set; }
        public Matrix GlobalTransform { get; set; }
        public Bone Parent { get; set; }
        public List<Bone> Children { get; set; } = new List<Bone>();
        public short Index { get; set; }
    }
}
