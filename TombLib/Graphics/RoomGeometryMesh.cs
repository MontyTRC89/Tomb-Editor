using System.Linq;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class RoomGeometryMesh : Mesh<RoomGeometryVertex>
    {
        public Texture2D Texture { get; set; }
        public PrimitiveType PrimitiveType { get; set; }
        public int PrimitiveCount { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public string TextureFileName { get; set; }

        public RoomGeometryMesh(GraphicsDevice device, string name)
            : base(device, name)
        {
        }

        public override void BuildBuffers()
        {
            BaseIndex = 0;
            NumIndices = Indices.Count;

            if (Vertices.Count == 0)
                return;

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<RoomGeometryVertex>(),
                SharpDX.Direct3D11.ResourceUsage.Default);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Default);
        }
    }
}
