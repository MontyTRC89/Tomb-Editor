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
        public WadMesh Mesh { get { return _mesh; } set { _mesh = value; } }
        public short Flags { get { return _flags; } set { _flags = value; } }
        public BoundingBox VisibilityBox { get { return _visibilityBox; } set { _visibilityBox = value; } }
        public BoundingBox CollisionBox { get { return _collisionBox; } set { _collisionBox = value; } }
        
        private BoundingBox _visibilityBox;
        private BoundingBox _collisionBox;
        private short _flags;
        private WadMesh _mesh;

        public override string ToString()
        {
            return "(" + ObjectID + ") " + ObjectNames.GetStaticName(ObjectID);
        }
    }
}