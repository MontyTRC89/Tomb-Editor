using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX.Toolkit.Graphics;
using SharpDX;

namespace TombEditor.Geometry
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EditorVertex
    {
        [VertexElementAttribute("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0)]
        public Vector4 Position;
        [VertexElementAttribute("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16)]
        public Vector2 UV;
        [VertexElementAttribute("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 24)]
        public Vector3 Normal;
        [VertexElementAttribute("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 36)]
        public Vector4 FaceColor;
        [VertexElementAttribute("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 52)]
        public Vector2 EditorUV;

        public bool EqualsTo(EditorVertex b)
        {
            if (Position != b.Position)
                return false;
            if (UV != b.UV)
                return false;
            if (Normal != b.Normal)
                return false;
            if (FaceColor != b.FaceColor)
                return false;

            return true;
        }
    }
}
