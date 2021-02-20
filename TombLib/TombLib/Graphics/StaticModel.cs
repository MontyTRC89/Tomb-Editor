using SharpDX.Toolkit.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class StaticModel : Model<ObjectMesh, ObjectVertex>
    {
        public StaticModel(GraphicsDevice device)
            : base(device, ModelType.Static)
        {}

        public override void UpdateBuffers()
        {
            foreach (var mesh in Meshes)
            {
                mesh.UpdateBoundingBox();
                mesh.UpdateBuffers();
            }
        }
    }
}
