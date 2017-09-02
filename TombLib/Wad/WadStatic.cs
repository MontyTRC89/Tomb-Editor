using SharpDX;

namespace TombLib.Wad
{
    public class WadStatic : WadObject
    {
        public WadMesh Mesh;
        public short Flags;
        public WadVector VisibilityBox1;
        public WadVector VisibilityBox2;
        public WadVector CollisionBox1;
        public WadVector CollisionBox2;

        public BoundingBox BoundingBox;

        public override string ToString()
        {
            return "(" + ObjectId + ") " + ObjectNames.GetStaticName(ObjectId);
        }
    }
}
