using System;
using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public class CameraInstance : PositionBasedObjectInstance, IHasScriptID
    {
        public ushort? ScriptId { get; set; }
        public bool Fixed { get; set; }

        public override bool CopyToFlipRooms => false;

        public CameraInstance()
        {
            ScriptId = UInt16.MaxValue;
        }
        
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
