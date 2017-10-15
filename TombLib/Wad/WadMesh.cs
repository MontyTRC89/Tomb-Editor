using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX;
using System.IO;
using TombLib.Utils;
using TombLib.IO;

namespace TombLib.Wad
{
    public class WadMesh : IEquatable<WadMesh>
    {
        private BoundingSphere _boundingSphere;
        private List<Vector3> _verticesPositions;
        private List<Vector3> _verticesNormals;
        private List<short> _verticesShades;
        private List<WadPolygon> _polygons;
        private Hash _hash;
        private BoundingBox _boundingBox;

        public BoundingSphere BoundingSphere { get { return _boundingSphere; } set { _boundingSphere = value; } }
        public List<Vector3> VerticesPositions { get { return _verticesPositions; } }
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
            _polygons = new List<WadPolygon>();
        }

        public WadMesh Clone()
        {
            var mesh = new WadMesh();

            mesh.BoundingBox = new BoundingBox(new Vector3(BoundingBox.Minimum.X,
                                                           BoundingBox.Minimum.Y,
                                                           BoundingBox.Minimum.Z),
                                               new Vector3(BoundingBox.Maximum.X,
                                                           BoundingBox.Maximum.Y,
                                                           BoundingBox.Maximum.Z));

            mesh.BoundingSphere = new BoundingSphere(new Vector3(BoundingSphere.Center.X,
                                                                 BoundingSphere.Center.Y,
                                                                 BoundingSphere.Center.Z),
                                                     BoundingSphere.Radius);

            foreach (var position in VerticesPositions)
                mesh.VerticesPositions.Add(new Vector3(position.X, position.Y, position.Z));

            foreach (var normal in VerticesNormals)
                mesh.VerticesNormals.Add(new Vector3(normal.X, normal.Y, normal.Z));

            foreach (var shade in VerticesShades)
                mesh.VerticesShades.Add(shade);

            foreach (var poly in Polys)
            {
                var newPoly = new WadPolygon(poly.Shape);

                newPoly.Indices.AddRange(poly.Indices.ToArray());
                newPoly.Texture = poly.Texture;
                newPoly.ShineStrength = poly.ShineStrength;

                mesh.Polys.Add(newPoly);
            }

            mesh.UpdateHash();

            return mesh;
        }

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriterEx(ms);
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
                    if (_polygons[i].Shape == WadPolygonShape.Quad)
                        writer.Write(_polygons[i].Indices[3]);
                    if (_polygons[i].Shape == WadPolygonShape.Quad)
                        writer.Write(_polygons[i].Indices[3]);
                    writer.Write(((WadTexture)(_polygons[i].Texture.Texture)).Hash.Hash1);
                    writer.Write(_polygons[i].Texture.DoubleSided);
                    writer.Write((short)_polygons[i].Texture.BlendMode);
                    writer.Write(_polygons[i].Texture.TexCoord0);
                    writer.Write(_polygons[i].Texture.TexCoord1);
                    writer.Write(_polygons[i].Texture.TexCoord2);
                    if (_polygons[i].Shape == WadPolygonShape.Quad)
                        writer.Write(_polygons[i].Texture.TexCoord3);
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

