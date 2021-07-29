using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOMesh
    {
        public string Name { get; private set; }
        public List<Vector3> Positions { get; private set; } = new List<Vector3>();
        public List<Vector3> Normals { get; private set; } = new List<Vector3>();
        public List<Vector2> UV { get; private set; } = new List<Vector2>();
        public List<Vector4> Colors { get; private set; } = new List<Vector4>();
        public Dictionary<IOMaterial, IOSubmesh> Submeshes { get; private set; } = new Dictionary<IOMaterial, IOSubmesh>();
        public Vector3 Position { get; set; }
        public Vector3 Origin { get; set; }

        public IOMesh(string name)
        {
            Name = name;
            Position = Vector3.Zero;
        }

        public int NumQuads
        {
            get
            {
                var numQuads = 0;
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                        if (poly.Shape == IOPolygonShape.Quad)
                        numQuads++;
                return numQuads;
            }
        }

        public int NumTriangles
        {
            get
            {
                var numTriangles = 0;
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                        if (poly.Shape == IOPolygonShape.Triangle)
                            numTriangles++;
                return numTriangles;
            }
        }

        public int NumIndices
        {
            get
            {
                var numIndieces = 0;
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                    {
                        if (poly.Shape == IOPolygonShape.Triangle)
                            numIndieces += 3;
                        if (poly.Shape == IOPolygonShape.Quad)
                            numIndieces += 4;
                    }
                return numIndieces;
            }
        }

        public List<uint> Indices
        {
            get
            {
                List<uint> indices = new List<uint>();
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                        if(poly.Shape == IOPolygonShape.Quad)
                        {
                            indices.Add((uint)poly.Indices[0]);
                            indices.Add((uint)poly.Indices[1]);
                            indices.Add((uint)poly.Indices[3]);
                            indices.Add((uint)poly.Indices[1]);
                            indices.Add((uint)poly.Indices[2]);
                            indices.Add((uint)poly.Indices[3]);
                            
                        }
                        else if(poly.Shape == IOPolygonShape.Triangle)
                        {
                            indices.Add((uint)poly.Indices[0]);
                            indices.Add((uint)poly.Indices[1]);
                            indices.Add((uint)poly.Indices[2]);
                        }
                return indices;
            }
        }

        public int NumPolygons
        {
            get
            {
                return NumQuads + NumTriangles;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                Vector3 minVertex = new Vector3(float.PositiveInfinity);
                Vector3 maxVertex = new Vector3(float.NegativeInfinity);
                foreach (Vector3 position in Positions)
                {
                    minVertex = Vector3.Min(minVertex, position);
                    maxVertex = Vector3.Max(maxVertex, position);
                }
                return new BoundingBox(minVertex, maxVertex);
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                BoundingBox boundingBox = BoundingBox;
                Vector3 center = (boundingBox.Minimum + boundingBox.Maximum) * 0.5f;
                float radius = (boundingBox.Maximum - center).Length();
                return new BoundingSphere(center, radius);
            }
        }

        public void CalculateNormals(bool weighted = false)
        {
            Normals.Clear();

            var tempNormals = new List<VertexNormalAverageHelper>();
            for (int i = 0; i < Positions.Count; i++)
                tempNormals.Add(new VertexNormalAverageHelper());

            foreach (var submesh in Submeshes)
            {
                foreach (var poly in submesh.Value.Polygons)
                {
                    var p0 = Positions[poly.Indices[0]];
                    var p1 = Positions[poly.Indices[1]];
                    var p2 = Positions[poly.Indices[2]];

                    var v1 = p1 - p0;
                    var v2 = p2 - p0;
                    var n  = Vector3.Cross(v1, v2);

                    if (weighted)
                        n = Vector3.Normalize(n);

                    // Get the angle between the two other points for each point;
                    // The starting point will be the 'base' and the two adjacent points will be normalized against it

                    var a1 = (p1 - p0).Angle(p2 - p0);
                    var a2 = (p2 - p1).Angle(p0 - p1);
                    var a3 = (p0 - p2).Angle(p1 - p2);

                    tempNormals[poly.Indices[0]].Normal += weighted ? Vector3.Multiply(a1, n) : n;
                    tempNormals[poly.Indices[0]].NumVertices++;

                    tempNormals[poly.Indices[1]].Normal += weighted ? Vector3.Multiply(a2, n) : n;
                    tempNormals[poly.Indices[1]].NumVertices++;

                    tempNormals[poly.Indices[2]].Normal += weighted ? Vector3.Multiply(a3, n) : n;
                    tempNormals[poly.Indices[2]].NumVertices++;

                    if (poly.Shape == IOPolygonShape.Quad)
                    {
                        tempNormals[poly.Indices[3]].Normal += weighted ? Vector3.Multiply(a3, n) : n;
                        tempNormals[poly.Indices[3]].NumVertices++;
                    }
                }
            }

            for (int i = 0; i < tempNormals.Count; i++)
            {
                var normal = tempNormals[i].Normal / Math.Max(1, tempNormals[i].NumVertices);
                normal = Vector3.Normalize(normal);
                Normals.Add(normal);
            }
        }
    }
}
