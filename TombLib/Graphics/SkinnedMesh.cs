using System.Linq;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class SkinnedMesh : Mesh<SkinnedVertex>
    {
        public SkinnedMesh(GraphicsDevice device, string name)
            : base(device, name)
        {
        }

        public override void BuildBuffers()
        {
            if (Vertices.Count == 0)
                return;

            Vb = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<SkinnedVertex>(),
                SharpDX.Direct3D11.ResourceUsage.Dynamic);
            Ib = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }
    }
}
