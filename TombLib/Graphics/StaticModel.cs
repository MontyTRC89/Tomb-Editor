using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class StaticModel : Model<StaticMesh, StaticVertex>
    {
        public StaticModel(GraphicsDevice device)
            : base(device, ModelType.Static)
        { }

        public override void BuildBuffers()
        {
            int lastBaseIndex = 0;
            int lastBaseVertex = 0;

            Vertices = new List<StaticVertex>();
            Indices = new List<int>();

            for (int i = 0; i < Meshes.Count; i++)
            {
                Vertices.AddRange(Meshes[i].Vertices);

                Meshes[i].BaseIndex = lastBaseIndex;
                Meshes[i].NumIndices = Meshes[i].Indices.Count;

                for (int j = 0; j < Meshes[i].Indices.Count; j++)
                {
                    Indices.Add((ushort)(lastBaseVertex + Meshes[i].Indices[j]));
                }

                lastBaseIndex += Meshes[i].Indices.Count;
                lastBaseVertex += Meshes[i].Vertices.Count;
            }

            if (Vertices.Count == 0) return;

            VertexBuffer = Buffer.Vertex.New<StaticVertex>(GraphicsDevice, Vertices.ToArray<StaticVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static StaticModel FromWad2(GraphicsDevice device, Wad2 wad, WadStatic staticMesh, List<WadTexture> reallocatedTextures)
        {
            StaticModel model = new StaticModel(device);

            // Initialize the mesh
            WadMesh msh = staticMesh.Mesh;
            StaticMesh mesh = new StaticMesh(device, staticMesh.ToString() + "_mesh");

            mesh.BoundingBox = msh.BoundingBox;
            mesh.BoundingSphere = msh.BoundingSphere;

            for (int j = 0; j < msh.Polys.Count; j++)
            {
                WadPolygon poly = msh.Polys[j];
                Vector2 positionInPackedTexture = ((WadTexture)(poly.Texture.Texture)).PositionInTextureAtlas;

                if (poly.Shape == WadPolygonShape.Triangle)
                {
                    int v1 = poly.Indices[0];
                    int v2 = poly.Indices[1];
                    int v3 = poly.Indices[2];

                    PutStaticVertexAndIndex(msh.VerticesPositions[v1], mesh, poly.Texture.TexCoord0, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v1] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v2], mesh, poly.Texture.TexCoord1, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v2] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v3], mesh, poly.Texture.TexCoord2, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v3] : 0), positionInPackedTexture);
                }
                else
                {
                    int v1 = poly.Indices[0];
                    int v2 = poly.Indices[1];
                    int v3 = poly.Indices[2];
                    int v4 = poly.Indices[3];

                    PutStaticVertexAndIndex(msh.VerticesPositions[v1], mesh, poly.Texture.TexCoord0, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v1] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v2], mesh, poly.Texture.TexCoord1, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v2] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v4], mesh, poly.Texture.TexCoord3, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v4] : 0), positionInPackedTexture);

                    PutStaticVertexAndIndex(msh.VerticesPositions[v4], mesh, poly.Texture.TexCoord3, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v4] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v2], mesh, poly.Texture.TexCoord1, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v2] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v3], mesh, poly.Texture.TexCoord2, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v3] : 0), positionInPackedTexture);

                }
            }

            model.Meshes.Add(mesh);

            // Prepare data by uploading data to the GPU
            model.BuildBuffers();

            return model;
        }

    }
}
