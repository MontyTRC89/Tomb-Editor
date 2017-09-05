using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX;
using System.IO;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadMesh : IEquatable<WadMesh>
    {
        private BoundingSphere _boundingSphere;
        private List<Vector3> _verticesPositions;
        private List<Vector2> _verticesUV;
        private List<Vector3> _verticesNormals;
        private List<short> _verticesShades;
        private List<WadPolygon> _polygons;
        private Hash _hash;
        private BoundingBox _boundingBox;

        public BoundingSphere BoundingSphere { get { return _boundingSphere; } set { _boundingSphere = value; } }
        public List<Vector3> VerticesPositions { get { return _verticesPositions; } }
        public List<Vector2> VerticesUV { get { return _verticesUV; } }
        public List<Vector3> VerticesNormals { get { return _verticesNormals; } }
        public List<short> VerticesShades { get { return _verticesShades; } }
        public List<WadPolygon> Polys { get { return _polygons; } }
        public Hash Hash { get { return _hash; } }
        public BoundingBox BoundingBox { get { return _boundingBox; } set { _boundingBox = value; } }

        public WadMesh()
        {
            _verticesPositions = new List<Vector3>();
            _verticesNormals = new List<Vector3>();
            _verticesShades = new List<short>();
            _verticesUV = new List<Vector2>();
            _polygons = new List<WadPolygon>();
        }

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                writer.Write(_boundingSphere.Center.X);
                writer.Write(_boundingSphere.Center.Y);
                writer.Write(_boundingSphere.Center.Z);
                writer.Write(_boundingSphere.Radius);

                int numVertices = _verticesPositions.Count;
                writer.Write(numVertices);

                for (int i = 0; i < _verticesPositions.Count; i++)
                {
                    writer.Write(_verticesPositions[i].X);
                    writer.Write(_verticesPositions[i].Y);
                    writer.Write(_verticesPositions[i].Z);
                }

                if (_verticesNormals.Count > 0)
                {
                    for (int i = 0; i < _verticesNormals.Count; i++)
                    {
                        writer.Write(_verticesNormals[i].X);
                        writer.Write(_verticesNormals[i].Y);
                        writer.Write(_verticesNormals[i].Z);
                    }
                }

                if (_verticesShades.Count > 0)
                {
                    for (int i = 0; i < _verticesShades.Count; i++)
                    {
                        writer.Write(_verticesShades[i]);
                    }
                }

                int numPolygons = _polygons.Count;

                for (int i = 0; i < _polygons.Count; i++)
                {
                    writer.Write((short)_polygons[i].Shape);
                    writer.Write(_polygons[i].Indices[0]);
                    writer.Write(_polygons[i].Indices[1]);
                    writer.Write(_polygons[i].Indices[2]);
                    if (_polygons[i].Shape == WadPolygonShape.Rectangle)
                        writer.Write(_polygons[i].Indices[3]);
                    writer.Write(_polygons[i].Texture.Hash.Hash1);
                    writer.Write(_polygons[i].Transparent);
                    writer.Write(_polygons[i].ShineStrength);
                }

                return ms.ToArray();
            }
        }

        public Hash UpdateHash()
        {
            _hash = Hash.FromByteArray(this.ToByteArray());
            return _hash;
        }

        public bool Equals(WadMesh other)
        {
            return (Hash == other.Hash);
        }
    }
}

