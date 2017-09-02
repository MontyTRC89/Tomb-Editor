using System.Collections.Generic;
using System.Linq;
using SharpDX.Toolkit.Graphics;
using SharpDX;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class StaticModel : Model<StaticMesh, StaticVertex>
    {
        public StaticModel(GraphicsDevice device)
            : base(device, ModelType.Static)
        {
        }

        public override void BuildBuffers()
        {
            int lastBaseIndex = 0;
            int lastBaseVertex = 0;

            Vertices = new List<StaticVertex>();
            Indices = new List<int>();

            foreach (var mesh in Meshes)
            {
                Vertices.AddRange(mesh.Vertices);

                mesh.BaseIndex = lastBaseIndex;
                mesh.NumIndices = mesh.Indices.Count;

                foreach (int index in mesh.Indices)
                {
                    Indices.Add((ushort)(lastBaseVertex + index));
                }

                lastBaseIndex += mesh.Indices.Count;
                lastBaseVertex += mesh.Vertices.Count;
            }

            if (Vertices.Count == 0) return;

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<StaticVertex>(),
                SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static StaticModel FromWad(GraphicsDevice device, WadStatic @static,
            Dictionary<uint, WadTexture> texturePages, Dictionary<uint, WadTextureSample> textureSamples)
        {
            var model = new StaticModel(device);

            // Initialize the mesh
            var msh = @static.Mesh;
            var mesh = new StaticMesh(device, $"{@static}_mesh") {BoundingBox = msh.BoundingBox};

            for (int j = 0; j < texturePages.Count; j++)
            {
                var submesh = new Submesh
                {
                    Material = new Material
                    {
                        Type = MaterialType.Flat,
                        Name = $"material_{j}",
                        DiffuseMap = (uint)j
                    }
                };
                mesh.SubMeshes.Add(submesh);
            }

            foreach (var poly in msh.Polygons)
            {
                int textureId = poly.Texture & 0xfff;
                if (textureId > 2047)
                    textureId = -(textureId - 4096);
                short submeshIndex = textureSamples[(uint)textureId].Page;

                var uv = CalculateUvCoordinates(poly, textureSamples);

                if (poly.Shape == Shape.Triangle)
                {
                    AddStaticVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V1] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V2] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V3] : 0));
                }
                else
                {
                    AddStaticVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V1] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V2] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V4] : 0));

                    AddStaticVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V4] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V2] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex,
                        (short)(msh.Shades != null ? msh.Shades[poly.V3] : 0));
                }
            }

            foreach (var current in mesh.SubMeshes)
            {
                current.StartIndex = (ushort)mesh.Indices.Count;
                foreach (ushort index in current.Indices)
                    mesh.Indices.Add(index);
                current.NumIndices = (ushort)current.Indices.Count;
            }

            mesh.BoundingSphere = new BoundingSphere(new Vector3(msh.SphereX, msh.SphereY, msh.SphereZ), msh.Radius);

            model.Meshes.Add(mesh);

            // Prepare data by uploading data to the GPU
            model.BuildBuffers();

            return model;
        }
    }
}
