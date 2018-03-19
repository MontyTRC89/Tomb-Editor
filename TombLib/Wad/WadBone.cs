using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Wad
{
    public class WadBone
    {
        public string Name { get; set; }
        public WadMesh Mesh { get; set; }
        public Matrix4x4 Transform { get; set; }
        public Vector3 Translation { get; set; }
        public WadBone Parent { get; set; }
        public List<WadBone> Children { get; } = new List<WadBone>();

        public IEnumerable<WadBone> LinearizedBones
        {
            get
            {
                yield return this;
                foreach (WadBone child in Children)
                    foreach (WadBone childBones in child.LinearizedBones)
                        yield return childBones;
            }
        }
    }
}
