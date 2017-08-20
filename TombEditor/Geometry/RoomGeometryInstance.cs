using System;
using TombLib.Graphics;

namespace TombEditor.Geometry
{
    public class RoomGeometryInstance : PositionBasedObjectInstance
    {
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(255, 128, 128, 128);

        private RoomGeometryModel _model;
        
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

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
