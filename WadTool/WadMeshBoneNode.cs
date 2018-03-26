using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public class WadMeshBoneNode
    {
        public WadMesh WadMesh { get; set; }
        public WadBone Bone { get; set; }
        public WadMeshBoneNode Parent { get; set; }
        public ObjectMesh DirectXMesh { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }
        public int LinearizedIndex { get; set; }
        public List<WadMeshBoneNode> Children { get; private set; } = new List<WadMeshBoneNode>();

        public Vector3 Centre
        {
            get
            {
                if (Bone == null || WadMesh == null || GlobalTransform == null)
                    return Vector3.Zero;
                else
                    return MathC.HomogenousTransform(WadMesh.BoundingBox.Center, GlobalTransform);
            }
        }

        public WadMeshBoneNode(WadMeshBoneNode parent, WadMesh wadMesh, WadBone bone, ObjectMesh dxMesh)
        {
            WadMesh = wadMesh;
            Bone = bone;
            DirectXMesh = dxMesh;
            GlobalTransform = Matrix4x4.Identity;
            Parent = parent;
        }

        public IEnumerable<WadMeshBoneNode> LinearizedBones
        {
            get
            {
                yield return this;
                foreach (WadMeshBoneNode child in Children)
                    foreach (WadMeshBoneNode childBones in child.LinearizedBones)
                        yield return childBones;
            }
        }
    }
}
