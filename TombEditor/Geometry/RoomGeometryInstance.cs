using System;
using TombLib.Graphics;

namespace TombEditor.Geometry
{
    public class RoomGeometryInstance : PositionBasedObjectInstance
    {
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(255, 128, 128, 128);

        private RoomGeometryModel _model;

        public RoomGeometryInstance(int id, Room room)
            : base(id, room)
        { }

        public RoomGeometryModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
            }
        }

        public override ObjectInstanceType Type => ObjectInstanceType.RoomGeometry;

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
