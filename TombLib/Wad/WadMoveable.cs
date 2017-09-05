using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace TombLib.Wad
{
    public class WadMoveable : WadObject
    {
        public List<WadMesh> Meshes {  get { return _meshes; } }
        public List<WadLink> Links {  get { return _links; } }
        public Vector3 Offset { get { return _offset; } set { _offset = value; } }
        public BoundingBox BoundingBox { get { return _boundingBox; } set { _boundingBox = value; } }
        public List<uint> MeshPointers { get { return _meshPointers; } }
        public List<WadLink> MeshTrees { get { return _meshTrees; } }
        public List<WadAnimation> Animations { get { return _animations; } }

        private List<WadMesh> _meshes;
        private List<WadLink> _links;
        private Vector3 _offset;
        private BoundingBox _boundingBox;
        private List<uint> _meshPointers;
        private List<WadLink> _meshTrees;
        private List<WadAnimation> _animations;

        public WadMoveable()
        {
            _meshes = new List<WadMesh>();
            _links = new List<WadLink>();
            _meshPointers = new List<uint>();
            _meshTrees = new List<WadLink>();
            _animations = new List<WadAnimation>();
        }

        public override string ToString()
        {
            return "(" + ObjectID + ") " + ObjectNames.GetMoveableName(ObjectID);
        }
    }
}
