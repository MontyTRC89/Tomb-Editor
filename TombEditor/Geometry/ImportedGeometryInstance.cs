using SharpDX;
using System;
using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public class ImportedGeometryInstance : PositionBasedObjectInstance
    {
        public ImportedGeometry Model { get; set; }

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
