using SharpDX.Toolkit.Graphics;

namespace TombLib.Graphics
{
    public class StaticMesh : Mesh<StaticVertex>
    {
        public StaticMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }
    }
}
