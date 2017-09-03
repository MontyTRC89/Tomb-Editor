using SharpDX;
using TombLib.Graphics;

namespace TombEditor.Geometry
{
    public class RoomGeometryInstance : PositionBasedObjectInstance
    {
        public Vector4 Color { get; set; } = new Vector4(1.0f);

        public RoomGeometryModel Model { get; set; }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }

        public override string ToString()
        {
            return Model?.Name ?? "Imported geometry";
        }
    }
}
