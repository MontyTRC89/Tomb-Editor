using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using TombEditor.Controls;

namespace TombEditor.Geometry
{
    public class BlockFace
    {
        public BlockFaceShape Shape { get; set; }

        public Plane Plane { get; set; }

        public bool Defined { get; set; }

        public bool Flipped { get; set; }

        public short Texture { get; set; }

        public short NewTexture { get; set; }

        public byte Rotation { get; set; }

        public bool Transparent { get; set; }

        public bool DoubleSided { get; set; }

        public bool Invisible { get; set; }

        public bool NoCollision { get; set; }

       // public List<EditorVertex> Vertices { get; set; }

        public short StartVertex { get; set; }

        public byte SplitMode { get; set; }

        public Vector2[] RectangleUV { get; set; }

        public Vector2[] TriangleUV { get; set; }

        public Vector2[] TriangleUV2 { get; set; }

        public byte[] EditorUV { get; set; }

        public TextureTileType TextureTriangle { get; set; }

        public List<short> EditorIndices { get; set; }

        public List<short> EditorIndices2 { get; set; }

        public List<short> Indices { get; set; }

        public int BaseIndexInVertices { get; set; }

        public int BaseIndexInOptimizedVertices { get; set; }

        public EditorVertex[] Vertices { get; set; }

        public BlockFace()
        {
            Texture = -1;
            Rotation = 0;
            //Vertices = new List<EditorVertex>();
            Indices = new List<short>();

            RectangleUV = new Vector2[4];
            TriangleUV = new Vector2[3];
            TriangleUV2 = new Vector2[3];
        }

        public bool RayIntersect(ref Ray ray, out Vector3 point)
        {
            if (Shape == BlockFaceShape.Rectangle)
            {
                EditorVertex v1 = Vertices[0];
                Vector3 p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                EditorVertex v2 = Vertices[0];
                Vector3 p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                EditorVertex v3 = Vertices[0];
                Vector3 p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);
                EditorVertex v4 = Vertices[0];
                Vector3 p4 = new Vector3(v4.Position.X, v4.Position.Y, v4.Position.Z);
                EditorVertex v5 = Vertices[0];
                Vector3 p5 = new Vector3(v5.Position.X, v5.Position.Y, v5.Position.Z);
                EditorVertex v6 = Vertices[0];
                Vector3 p6 = new Vector3(v6.Position.X, v6.Position.Y, v6.Position.Z);

                if (!ray.Intersects(ref p1, ref p2, ref p3, out point)) return false;
                if (!ray.Intersects(ref p4, ref p5, ref p6, out point)) return false;

                return true;
            }
            else
            {
                EditorVertex v1 = Vertices[0];
                Vector3 p1 = new Vector3(v1.Position.X, v1.Position.Y, v1.Position.Z);
                EditorVertex v2 = Vertices[0];
                Vector3 p2 = new Vector3(v2.Position.X, v2.Position.Y, v2.Position.Z);
                EditorVertex v3 = Vertices[0];
                Vector3 p3 = new Vector3(v3.Position.X, v3.Position.Y, v3.Position.Z);

                if (!ray.Intersects(ref p1, ref p2, ref p3, out point)) return false;

                return true;
            }
        }        
    }
}
