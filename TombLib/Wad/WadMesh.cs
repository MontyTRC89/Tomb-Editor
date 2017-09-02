using System.Runtime.InteropServices;
using SharpDX;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadMesh
    {
        public short SphereX;
        public short SphereY;
        public short SphereZ;
        public ushort Radius;
        public ushort Unknown;
        public ushort NumVertices;
        public WadVector[] Vertices;
        public short NumNormals;
        public WadVector[] Normals;
        public short[] Shades;
        public ushort NumPolygons;
        public WadPolygon[] Polygons;
        public BoundingBox BoundingBox;
    }
}
