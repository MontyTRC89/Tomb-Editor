using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class WadBone
    {
        public string Name { get; set; }
        public WadMesh Mesh { get; set; }
        public Matrix4x4 Transform { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }
        public WadBone Parent { get; set; }
        public int Index { get; set; }
        public List<WadBone> Children { get; set; } = new List<WadBone>();
    }
}
