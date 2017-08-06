using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit;
using SharpDX;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class StaticModel : Model<StaticMesh, StaticVertex>
    {
        public StaticModel(GraphicsDevice device)
            : base(device, ModelType.Static)
        { }

        public override void BuildBuffers()
        {
            Vertices = new List<StaticVertex>();
            Vertices.AddRange(Meshes[0].Vertices);

            Indices = new List<int>();
            Indices.AddRange(Meshes[0].Indices);

            if (Vertices.Count == 0)
                return;

            Meshes[0].BaseIndex = 0;
            Meshes[0].NumIndices = Indices.Count;

            VertexBuffer = Buffer.Vertex.New<StaticVertex>(GraphicsDevice, Vertices.ToArray<StaticVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static StaticModel FromWad(GraphicsDevice device, WadStatic static_, 
            Dictionary<uint, WadTexturePage> texturePages, Dictionary<uint, WadTextureSample> textureSamples)
        {
            StaticModel model = new StaticModel(device);

            // Initialize the mesh
            WadMesh msh = static_.Mesh;
            StaticMesh mesh = new StaticMesh(device, static_.ToString() + "_mesh");
            mesh.BoundingBox = msh.BoundingBox;

            for (int j = 0; j < texturePages.Count; j++)
            {
                Submesh submesh = new Submesh();
                submesh.Material = new Material();
                submesh.Material.Type = MaterialType.Flat;
                submesh.Material.Name = "material_" + j.ToString();
                submesh.Material.DiffuseMap = (uint)j;
                mesh.SubMeshes.Add(submesh);
            }

            for (int j = 0; j < msh.Polygons.Length; j++)
            {
                WadPolygon poly = msh.Polygons[j];
                int textureId = poly.Texture & 0xfff;
                if (textureId > 2047)
                    textureId = -(textureId - 4096);
                short submeshIndex = textureSamples[(uint)textureId].Page;

                List<Vector2> uv = CalculateUVCoordinates(poly, textureSamples);

                if (poly.Shape == Shape.Triangle)
                {
                    AddStaticVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V1] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V2] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V3] : 0));
                }
                else
                {
                    AddStaticVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V1] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V2] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V4] : 0));

                    AddStaticVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V4] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V2] : 0));
                    AddStaticVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex, (short)(msh.Shades != null ? msh.Shades[poly.V3] : 0));

                }
            }

            for (int j = 0; j < mesh.SubMeshes.Count; j++)
            {
                Submesh current = mesh.SubMeshes[j];
                current.StartIndex = (ushort)mesh.Indices.Count;
                for (int k = 0; k < current.Indices.Count; k++)
                    mesh.Indices.Add(current.Indices[k]);
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
