using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX;
using TombLib.Wad.Catalog;

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

        public WadStatic(Wad2 wad)
            : base(wad)
        {

        }

        public override string ToString()
        {
            return "(" + ObjectID + ") " + TrCatalog.GetStaticName(Wad.Version, ObjectID);
        }

        public WadStatic Clone()
        {
            var staticMesh = new WadStatic(Wad);
            staticMesh.VisibilityBox = new BoundingBox(VisibilityBox.Minimum, VisibilityBox.Maximum);
            staticMesh.CollisionBox = new BoundingBox(CollisionBox.Minimum, CollisionBox.Maximum);
            staticMesh.Flags = Flags;
            staticMesh.Mesh = Mesh;
            staticMesh.ObjectID = ObjectID;
            return staticMesh;
        }
    }
}