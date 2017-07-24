using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Toolkit.Graphics;
using SharpDX.DXGI;

namespace TombLib.Graphics
{
    public class InputLayouts
    {
        private static VertexInputLayout _skinned;

        private static VertexInputLayout _static;

        static InputLayouts()
        {
            _skinned = VertexInputLayout.New(0, new VertexElement("POSITION0", 0, Format.R32G32B32A32_Float),
                                                new VertexElement("TEXCOORD0", 0, Format.R32G32_Float),
                                                new VertexElement("NORMAL0", 0, Format.R32G32B32_Float),
                                                new VertexElement("TANGENT0", 0, Format.R32G32B32_Float),
                                                new VertexElement("BINORMAL0", 0, Format.R32G32B32_Float),
                                                new VertexElement("BLENDWEIGTH0", 0, Format.R32G32B32A32_Float),
                                                new VertexElement("BLENDINDICES0", 0, Format.R32G32B32A32_Float));

            _static = VertexInputLayout.New(0, new VertexElement("POSITION0", 0, Format.R32G32B32A32_Float),
                                               new VertexElement("TEXCOORD0", 0, Format.R32G32_Float),
                                               new VertexElement("NORMAL0", 0, Format.R32G32B32_Float),
                                               new VertexElement("TANGENT0", 0, Format.R32G32B32_Float),
                                               new VertexElement("BINORMAL0", 0, Format.R32G32B32_Float));                             
        }

        public static VertexInputLayout SkinnedVertexLayout
        {
            get
            {
                return _skinned;
            }
        }

        public static VertexInputLayout StaticVertexLayout
        {
            get
            {
                return _static;
            }
        }
    }
}
