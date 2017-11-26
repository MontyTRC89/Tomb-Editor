using System;

namespace TombEditor.Geometry
{
    public class SinkInstance : PositionBasedObjectInstance, IHasScriptID
    {
        public ushort? ScriptId { get; set; }
        public short Strength { get; set; }
        
        public override bool CopyToFlipRooms => false;

        public SinkInstance()
        {
            ScriptId = UInt16.MaxValue;
        }
        
        public override string ToString()
        {
            return "Sink with strength " + Strength +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }
    }
}
