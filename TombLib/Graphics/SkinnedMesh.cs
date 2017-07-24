using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (Vertices.Count == 0) return;

            _vb = Buffer.Vertex.New<SkinnedVertex>(GraphicsDevice, Vertices.ToArray<SkinnedVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            _ib = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }
    }
}
