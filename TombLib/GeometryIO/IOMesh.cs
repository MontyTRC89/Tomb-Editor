using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
        
        public IOMesh(string name)
        {
            Name = name;
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
    }
}
