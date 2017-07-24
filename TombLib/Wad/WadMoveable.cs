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
        public List<WadMesh> Meshes;
        public List<WadLink> Links;
        public List<WadAnimation> Animations;
        public Vector3 Offset;
        public BoundingBox BoundingBox;

        public WadMoveable()
        {
            Meshes = new List<WadMesh>();
            Links = new List<WadLink>();
            Animations = new List<WadAnimation>();
        }

    }
}
