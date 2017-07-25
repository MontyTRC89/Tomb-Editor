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
        public string Name;
        public Matrix Transform;
        public Matrix GlobalTransform;
        public Bone Parent;
        public List<Bone> Children = new List<Bone>();
        public short Index;
    }
}
