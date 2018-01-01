using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

            Vertices = new List<StaticVertex>();
            Indices = new List<int>();

            foreach (var mesh in Meshes)
            {
                Vertices.AddRange(mesh.Vertices);

                foreach (var submesh in mesh.Submeshes)
                {
                    submesh.Value.BaseIndex = lastBaseIndex;
                    foreach (var index in submesh.Value.Indices)
                        Indices.Add((ushort)(lastBaseIndex + index));
                    lastBaseIndex += submesh.Value.NumIndices;
                }

                mesh.UpdateBoundingBox();
            }

            if (Vertices.Count == 0) return;

            VertexBuffer = Buffer.Vertex.New<StaticVertex>(GraphicsDevice, Vertices.ToArray<StaticVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static StaticModel FromWad2(GraphicsDevice device, Wad2 wad, WadStatic staticMesh, List<WadTexture> reallocatedTextures)
        {
            StaticModel model = new StaticModel(device);

            // TODO: with new renderer add support for other materials and maybe multiple textures
            var material = new Material("Wad2Mat");

            // Initialize the mesh
            var msh = staticMesh.Mesh;
            var mesh = new StaticMesh(device, staticMesh.ToString() + "_mesh");
            var submesh = new Submesh(material);
            mesh.Submeshes.Add(material, submesh);

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

                    PutStaticVertexAndIndex(msh.VerticesPositions[v1], mesh, submesh, poly.Texture.TexCoord0, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v1] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v2], mesh, submesh, poly.Texture.TexCoord1, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v2] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v3], mesh, submesh, poly.Texture.TexCoord2, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v3] : 0), positionInPackedTexture);
                }
                else
                {
                    int v1 = poly.Indices[0];
                    int v2 = poly.Indices[1];
                    int v3 = poly.Indices[2];
                    int v4 = poly.Indices[3];

                    PutStaticVertexAndIndex(msh.VerticesPositions[v1], mesh, submesh, poly.Texture.TexCoord0, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v1] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v2], mesh, submesh, poly.Texture.TexCoord1, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v2] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v4], mesh, submesh, poly.Texture.TexCoord3, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v4] : 0), positionInPackedTexture);

                    PutStaticVertexAndIndex(msh.VerticesPositions[v4], mesh, submesh, poly.Texture.TexCoord3, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v4] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v2], mesh, submesh, poly.Texture.TexCoord1, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v2] : 0), positionInPackedTexture);
                    PutStaticVertexAndIndex(msh.VerticesPositions[v3], mesh, submesh, poly.Texture.TexCoord2, 0, (short)(msh.VerticesShades.Count != 0 ? msh.VerticesShades[v3] : 0), positionInPackedTexture);
                }
            }

            model.Meshes.Add(mesh);

            // Prepare data by uploading data to the GPU
            model.BuildBuffers();

            return model;
        }

    }
}
