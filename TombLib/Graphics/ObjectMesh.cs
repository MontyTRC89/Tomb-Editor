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
    public class ObjectMesh : Mesh<ObjectVertex>, IDisposable
    {
        public Buffer<ObjectVertex> VertexBuffer { get; private set; }
        public Buffer IndexBuffer { get; private set; }
        public List<Material> Materials { get; private set; }

        public ObjectMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }

        public void UpdateBuffers()
        {
            int lastBaseIndex = 0;

            foreach (var submesh in Submeshes)
            {
                submesh.Value.BaseIndex = lastBaseIndex;
                if (submesh.Value.NumIndices != 0)
                    foreach (var index in submesh.Value.Indices)
                        Indices.Add((ushort)(index));
                lastBaseIndex += submesh.Value.NumIndices;
            }

            UpdateBoundingBox();

            if (Vertices.Count == 0)
                return;

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<ObjectVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        ~ObjectMesh()
        {
            Dispose(true);
        }

        private static void PutObjectVertexAndIndex(Vector3 v, Vector3 n,
                                                    ObjectMesh mesh, Submesh submesh, Vector2 uv, int submeshIndex,
                                                    short color, Vector2 positionInAtlas)
        {
            var newVertex = new ObjectVertex();

            newVertex.Position = new Vector3(v.X, v.Y, v.Z);
            newVertex.UV = new Vector2((positionInAtlas.X + uv.X) / WadRenderer.TextureAtlasSize,
                                       (positionInAtlas.Y + uv.Y) / WadRenderer.TextureAtlasSize);
            newVertex.Normal = n / n.Length();

            var shade = 1.0f - color / 8191.0f;
            newVertex.Shade = new Vector2(shade, 0.0f);

            mesh.Vertices.Add(newVertex);
            submesh.Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }

        public static ObjectMesh FromWad2(GraphicsDevice device, WadMesh msh, Func<WadTexture, VectorInt2> allocateTexture)
        {
            // Initialize the mesh
            var mesh = new ObjectMesh(device, msh.Name /*"ObjectMesh_" + msh.Hash.ToString()*/);

            // Prepare materials
            var materialOpaque = new Material(Material.Material_Opaque + "_0_0_0_0", null, false, false, 0);
            var materialOpaqueDoubleSided = new Material(Material.Material_OpaqueDoubleSided + "_0_0_1_0", null, false, true, 0);
            var materialAdditiveBlending = new Material(Material.Material_AdditiveBlending + "_0_1_0_0", null, true, false, 0);
            var materialAdditiveBlendingDoubleSided = new Material(Material.Material_AdditiveBlendingDoubleSided + "_0_1_1_0", null, true, true, 0);

            mesh.Materials = new List<Material>();
            mesh.Materials.Add(materialOpaque);
            mesh.Materials.Add(materialOpaqueDoubleSided);
            mesh.Materials.Add(materialAdditiveBlending);
            mesh.Materials.Add(materialAdditiveBlendingDoubleSided);

            mesh.Submeshes.Add(materialOpaque, new Submesh(materialOpaque));
            mesh.Submeshes.Add(materialOpaqueDoubleSided, new Submesh(materialOpaqueDoubleSided));
            mesh.Submeshes.Add(materialAdditiveBlending, new Submesh(materialAdditiveBlending));
            mesh.Submeshes.Add(materialAdditiveBlendingDoubleSided, new Submesh(materialAdditiveBlendingDoubleSided));

            mesh.BoundingBox = msh.BoundingBox;
            mesh.BoundingSphere = msh.BoundingSphere;

            var hasShades = msh.VerticesShades.Count != 0;

            for (int j = 0; j < msh.Polys.Count; j++)
            {
                WadPolygon poly = msh.Polys[j];
                Vector2 positionInPackedTexture = allocateTexture((WadTexture)poly.Texture.Texture);

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

                // Do half-pixel correction on texture to prevent bleeding
                var coords = poly.Texture.TexCoords;
                var shape = (int)TextureExtensions.GetTextureShapeType(poly.Texture.TexCoords, poly.Shape == WadPolygonShape.Triangle);

                for(int i = 0; i < (poly.Texture.TextureIsTriangle ? 3 : 4); i++)
                    if (poly.Texture.TextureIsTriangle)
                        coords[i] += TextureExtensions.CompensationTris[shape, i];
                    else
                        coords[i] += TextureExtensions.CompensationQuads[shape, i];

                if (poly.Shape == WadPolygonShape.Triangle)
                {
                    int v1 = poly.Index0;
                    int v2 = poly.Index1;
                    int v3 = poly.Index2;

                    PutObjectVertexAndIndex(msh.VerticesPositions[v1], msh.VerticesNormals[v1], mesh, submesh,
                                            coords[0], 0, (short)(hasShades ? msh.VerticesShades[v1] : 0),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VerticesPositions[v2], msh.VerticesNormals[v2], mesh, submesh,
                                            coords[1], 0, (short)(hasShades ? msh.VerticesShades[v2] : 0),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VerticesPositions[v3], msh.VerticesNormals[v3], mesh, submesh,
                                            coords[2], 0, (short)(hasShades ? msh.VerticesShades[v3] : 0),
                                            positionInPackedTexture);
                }
                else
                {
                    int v1 = poly.Index0;
                    int v2 = poly.Index1;
                    int v3 = poly.Index2;
                    int v4 = poly.Index3;

                    PutObjectVertexAndIndex(msh.VerticesPositions[v1], msh.VerticesNormals[v1], mesh, submesh,
                                            coords[0], 0, (short)(hasShades ? msh.VerticesShades[v1] : 0),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VerticesPositions[v2], msh.VerticesNormals[v2], mesh, submesh,
                                            coords[1], 0, (short)(hasShades ? msh.VerticesShades[v2] : 0),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VerticesPositions[v4], msh.VerticesNormals[v4], mesh, submesh,
                                            coords[3], 0, (short)(hasShades ? msh.VerticesShades[v4] : 0),
                                            positionInPackedTexture);

                    PutObjectVertexAndIndex(msh.VerticesPositions[v4], msh.VerticesNormals[v4], mesh, submesh,
                                            coords[3], 0, (short)(hasShades ? msh.VerticesShades[v4] : 0),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VerticesPositions[v2], msh.VerticesNormals[v2], mesh, submesh,
                                            coords[1], 0, (short)(hasShades ? msh.VerticesShades[v2] : 0),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VerticesPositions[v3], msh.VerticesNormals[v3], mesh, submesh,
                                            coords[2], 0, (short)(hasShades ? msh.VerticesShades[v3] : 0),
                                            positionInPackedTexture);
                }
            }

            mesh.UpdateBuffers();

            return mesh;
        }
    }
}
