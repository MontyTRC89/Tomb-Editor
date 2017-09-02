using System.Linq;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class StaticMesh : Mesh<StaticVertex>
    {
        public StaticMesh(GraphicsDevice device, string name)
            : base(device, name)
        {
        }

        public override void BuildBuffers()
        {
            if (Vertices.Count == 0)
                return;

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<StaticVertex>(),
                SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }
    }
}
