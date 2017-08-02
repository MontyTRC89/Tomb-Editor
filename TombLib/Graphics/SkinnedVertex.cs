using SharpDX;
using SharpDX.Toolkit.Graphics;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SkinnedVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0)]
        public Vector4 Position;
        [VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16)]
        public Vector2 UV;
        [VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 24)]
        public Vector3 Normal;
        [VertexElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 36)]
        public Vector3 Tangent;
        [VertexElement("BINORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 48)]
        public Vector3 Binormal;
        [VertexElement("BONEWEIGTH", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 60)]
        public Vector4 BoneWeigths;
        [VertexElement("BLENDINDICES", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 76)]
        public Vector4 BoneIndices;

        Vector4 IVertex.Position
        {
            get { return Position; }
        }
    }
}
