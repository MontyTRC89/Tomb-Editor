using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace TombLib.Wad
{
    public class WadMoveable
    {
        public uint ObjectID;
        public List<WadMesh> Meshes = new List<WadMesh>();
        public List<WadLink> Links = new List<WadLink>();
        public List<WadAnimation> Animations = new List<WadAnimation>();
        public Vector3 Offset;
        public BoundingBox BoundingBox;
    }
}
