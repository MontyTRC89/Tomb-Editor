using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOMesh
    {
        public List<Vector3> Positions { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<Vector2> UV { get; private set; }
        public List<Vector4> Colors { get; private set; }
        public List<IOPolygon> Polygons { get; private set; }
        public BoundingBox BoundingBox { get; internal set; }
        public BoundingSphere BoundingSphere { get; internal set; }
        public IOTexture Texture { get; set; }

        public IOMesh()
        {
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            UV = new List<Vector2>();
            Colors = new List<Vector4>();
            Polygons = new List<IOPolygon>();
        }

        public int NumQuads
        {
            get
            {
                var numQuads = 0;
                foreach (var poly in Polygons)
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
                foreach (var poly in Polygons)
                    if (poly.Shape == IOPolygonShape.Triangle)
                        numTriangles++;
                return numTriangles;
            }
        }
    }
}
