using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace TombLib.Graphics
{
    public struct StaticVertex
    {
        [VertexElementAttribute("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0)]
        public Vector4 Position;
        [VertexElementAttribute("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16)]
        public Vector2 UV;
        [VertexElementAttribute("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 24)]
        public Vector3 Normal;
        [VertexElementAttribute("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 36)]
        public Vector2 Shade;
    }
}
