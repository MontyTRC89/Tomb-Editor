using SharpDX;
using System;
using System.Collections.Generic;
using TombLib.Graphics;

namespace TombEditor.Geometry
{
    public class RoomGeometryInstance : PositionBasedObjectInstance
    {
        public Vector4 Color { get; set; } = new Vector4(1.0f);

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

        public override string ToString()
        {
            return (Model != null ? Model.Name : "Imported geometry");
        }
    }
}
