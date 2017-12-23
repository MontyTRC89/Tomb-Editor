using SharpDX.Toolkit.Graphics;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Graphics;

namespace TombLib.LevelData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EditorVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)]
        public Vector3 Position;
        private float _unusedPadding;
        [VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16)]
        public Vector2 UV;
        [VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 24)]
        public Vector3 Normal;
        [VertexElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 36)]
        public Vector4 Color;
        [VertexElement("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 52)]
        public Vector2 EditorUV;

        // EditorUV Map:
        //                      | +Y
        //   ################## | #################  
        //   ###   Triangle   # | #               #
        //   #  ###    Split  # | #               #
        //   #     ###        # | #      Quad     #
        //   #        ###     # | #      Face     #
        //   #           ###  # | #               #
        //   #              ### | #               #
        //   ################## | #################    +x
        // ---------------------0---------------------------
        //   ################## | ##################  
        //   ###   Triangle   # | ###   Triangle   #
        //   #  ###    Split  # | #  ###    Split  #
        //   #     ###        # | #     ###        #
        //   #        ###     # | #        ###     #
        //   #           ###  # | #           ###  #
        //   #              ### | #              ###
        //   ################## | ##################
        //                      |


        Vector3 IVertex.Position => Position;
    }
}
