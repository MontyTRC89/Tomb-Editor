using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SolidVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)]
        public Vector3 Position;
        [VertexElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12)]
        public Vector4 Color;

        Vector3 IVertex.Position => Position;

        public SolidVertex(Vector3 pos, Vector4 color)
        {
            Position = pos;
            Color = color;
        }
    }
}
