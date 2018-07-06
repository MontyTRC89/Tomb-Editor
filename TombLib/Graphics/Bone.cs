using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Graphics
{
    public class Bone
    {
        public string Name { get; set; }
        public Matrix4x4 Transform { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }
        public Bone Parent { get; set; }
        public List<Bone> Children { get; set; } = new List<Bone>();
        public int Index { get; set; }
    }
}
