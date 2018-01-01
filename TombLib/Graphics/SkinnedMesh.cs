using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class SkinnedMesh : Mesh<SkinnedVertex>
    {
        public SkinnedMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }
    }
}
