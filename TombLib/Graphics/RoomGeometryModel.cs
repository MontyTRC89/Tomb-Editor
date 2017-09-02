using SharpDX.Toolkit.Graphics;

namespace TombLib.Graphics
{
    public class RoomGeometryModel : Model<RoomGeometryMesh, RoomGeometryVertex>
    {
        public RoomGeometryModel(GraphicsDevice device)
            : base(device, ModelType.RoomGeometry)
        {
        }

        public override void BuildBuffers()
        {
        }
    }
}
