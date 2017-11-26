using System;
using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public class CameraInstance : PositionAndScriptBasedObjectInstance
    {
        public bool Fixed { get; set; }

        public override bool CopyToFlipRooms => false;

        public override string ToString()
        {
            return "Camera " + (Fixed ? "Fixed" : "") +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }
    }
}
