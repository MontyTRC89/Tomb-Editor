using System.Numerics;
using TombLib;
using TombLib.Wad;

namespace WadTool
{
    public class WadMeshBoneNode
    {
        public WadMesh Mesh { get; set; }
        public WadBone Bone { get; set; }
        public Matrix4x4 GlobalTransform { get; set; }

        public Vector3 Center
        {
            get
            {
                if (Bone == null || Mesh == null || GlobalTransform == null)
                    return Vector3.Zero;
                else
                    return MathC.HomogenousTransform(Mesh.BoundingBox.Center, GlobalTransform);
            }
        }

        public WadMeshBoneNode(WadMesh wadMesh, WadBone bone)
        {
            Mesh = wadMesh;
            Bone = bone;
            GlobalTransform = Matrix4x4.Identity;
        }
    }
}
