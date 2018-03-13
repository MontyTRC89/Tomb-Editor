using SharpDX.Toolkit.Graphics;

namespace TombLib.Graphics
{
    public class SkinnedMesh : Mesh<SkinnedVertex>
    {
        public SkinnedMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }
    }
}
