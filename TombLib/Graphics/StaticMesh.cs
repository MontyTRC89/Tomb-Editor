using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class StaticMesh : Mesh<StaticVertex>
    {
        public StaticMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }
    }
}
