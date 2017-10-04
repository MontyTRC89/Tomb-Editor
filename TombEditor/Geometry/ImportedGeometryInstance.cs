using SharpDX;
using System;
using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public class ImportedGeometryInstance : PositionBasedObjectInstance, IScaleable, IRotateableYXRoll
    {
        public ImportedGeometry Model { get; set; }
        public float Scale { get; set; } = 1;

        private float _roll { get; set; } = 0;
        private float _rotationX { get; set; } = 0;
        private float _rotationY { get; set; } = 0;

        /// <summary> Degrees in the range [0, 360) </summary>
        public float Roll
        {
            get { return _roll; }
            set { _roll = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        /// <summary> Degrees in the range [-90, 90] </summary>
        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = Math.Max(-90, Math.Min(90, value)); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
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
