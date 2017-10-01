using SharpDX;
using System;
using System.Collections.Generic;
using TombLib.Graphics;

namespace TombEditor.Geometry
{
    public class ImportedGeometryInstance : PositionBasedObjectInstance, IScaleable, IRotateableYXRoll
    {
        public ImportedGeometry Model { get; set; }
        public float Scale { get; set; }
        public float RotationY { get; set; }
        public float Roll { get; set; }
        public float RotationX { get; set; }

        public ImportedGeometryInstance()
        {
            Scale = 1.0f;
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }

        public override string ToString()
        {
            string result = "Imported Geometry: ";
            if (Model == null)
                result += "None";
            else
            {
                result += Model.Info.Name;
                if (Model.DirectXModel == null)
                    result += "(Unloaded: " + (Model.LoadException?.Message ?? "") + ")";
            }
            return result;
        }
    }
}
