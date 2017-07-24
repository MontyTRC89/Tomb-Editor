using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WadStaticMesh
    {
        public uint ObjectID;
        public WadMesh Mesh;
        public short Flags;
        public WadVertex VisibilityBox1;
        public WadVertex VisibilityBox2;
        public WadVertex CollisionBox1;
        public WadVertex CollisionBox2;

        public BoundingBox BoundingBox;
    }
}