using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Graphics
{
    public class Bone
    {
        public string Name { get; set; }
        public Matrix4x4 Transform { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }
        public Bone Parent { get; set; }
        public List<Bone> Children { get; set; } = new List<Bone>();
        public short Index { get; set; }
    }
}
