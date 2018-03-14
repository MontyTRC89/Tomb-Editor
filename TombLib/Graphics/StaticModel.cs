using SharpDX.Toolkit.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class StaticModel : Model<ObjectMesh, ObjectVertex>
    {
        public StaticModel(GraphicsDevice device)
            : base(device, ModelType.Static)
        { }

        public override void UpdateBuffers()
        {
            int lastBaseIndex = 0;

            Vertices = new List<ObjectVertex>();
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

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<ObjectVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static StaticModel FromWad2(GraphicsDevice device, Wad2 wad, WadStatic staticMesh, Dictionary<WadTexture, VectorInt2> reallocatedTextures)
        {
            StaticModel model = new StaticModel(device);

            // Create the mesh
            var msh = staticMesh.Mesh;
            var mesh = ObjectMesh.FromWad2(device, wad, msh, reallocatedTextures);
            if (!wad.DirectXMeshes.ContainsKey(msh))
                wad.DirectXMeshes.TryAdd(msh, mesh);
            model.Meshes.Add(mesh);

            // Prepare data by uploading data to the GPU
            model.UpdateBuffers();

            return model;
        }

    }
}
