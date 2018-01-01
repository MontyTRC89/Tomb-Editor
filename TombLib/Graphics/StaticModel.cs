using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
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

            // Prepare materials
            var materialOpaque = new Material("TeMat_0_0_0_0", null, false, false, 0);
            var materialOpaqueDoubleSided = new Material("TeMat_0_0_1_0", null, false, true, 0);
            var materialAdditiveBlending = new Material("TeMat_0_1_0_0", null, true, false, 0);
            var materialAdditiveBlendingDoubleSided = new Material("TeMat_0_1_1_0", null, true, true, 0);

            model.Materials.Add(materialOpaque);
            model.Materials.Add(materialOpaqueDoubleSided);
            model.Materials.Add(materialAdditiveBlending);
            model.Materials.Add(materialAdditiveBlendingDoubleSided);

            // Initialize the mesh
            var msh = staticMesh.Mesh;
            var mesh = new StaticMesh(device, staticMesh.ToString() + "_mesh");

            mesh.Submeshes.Add(materialOpaque, new Submesh(materialOpaque));
            mesh.Submeshes.Add(materialOpaqueDoubleSided, new Submesh(materialOpaqueDoubleSided));
            mesh.Submeshes.Add(materialAdditiveBlending, new Submesh(materialAdditiveBlending));
            mesh.Submeshes.Add(materialAdditiveBlendingDoubleSided, new Submesh(materialAdditiveBlendingDoubleSided));

            mesh.BoundingBox = msh.BoundingBox;
            mesh.BoundingSphere = msh.BoundingSphere;

            for (int j = 0; j < msh.Polys.Count; j++)
            {
                WadPolygon poly = msh.Polys[j];
                Vector2 positionInPackedTexture = ((WadTexture)(poly.Texture.Texture)).PositionInTextureAtlas;

                // Get the right submesh
                var submesh = mesh.Submeshes[materialOpaque];
                if (poly.Texture.BlendMode == BlendMode.Additive)
                {
                    if (poly.Texture.DoubleSided)
                        submesh = mesh.Submeshes[materialAdditiveBlendingDoubleSided];
                    else
                        submesh = mesh.Submeshes[materialAdditiveBlending];
                }
                else
                {
                    if (poly.Texture.DoubleSided)
                        submesh = mesh.Submeshes[materialOpaqueDoubleSided];
                }

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
