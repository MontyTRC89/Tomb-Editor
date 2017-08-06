using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            return "(" + ObjectID + ") " + ObjectNames.GetStaticName(ObjectID);
        }
    }
}