using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit;
using SharpDX;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class StaticModel : Model<StaticMesh, StaticVertex>
    {
        public StaticModel(GraphicsDevice device, uint objectId)
            : base(device, objectId, ModelType.Static)
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

            _vb = Buffer.Vertex.New<StaticVertex>(GraphicsDevice, Vertices.ToArray<StaticVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            _ib = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }
    }
}
