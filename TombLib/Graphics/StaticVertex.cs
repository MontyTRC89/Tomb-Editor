using SharpDX;
using SharpDX.Toolkit.Graphics;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)]
        public Vector3 Position;
        private float _unusedPadding;
        [VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16)]
        public Vector2 UV;
        [VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 24)]
        public Vector3 Normal;
        [VertexElement("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 36)]
        public Vector2 Shade;

        Vector3 IVertex.Position => Position;
    }
}
