using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Wad
{
    public class WadBone
    {
        public string Name { get; set; }
        public WadMesh Mesh { get; set; }
        public Matrix4x4 Transform { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }
        public Vector3 Translation { get; set; }
        public WadBone Parent { get; set; }
        public int Index { get; set; }
        public List<WadBone> Children { get; set; } = new List<WadBone>();
    }
}
