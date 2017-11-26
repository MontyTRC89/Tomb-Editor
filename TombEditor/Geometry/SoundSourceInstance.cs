using System;

namespace TombEditor.Geometry
{
    public class SoundSourceInstance : PositionAndScriptBasedObjectInstance
    {
        public ushort SoundId { get; set; } = 0;
        public short Flags { get; set; } = 0;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

        public override bool CopyToFlipRooms => false;

        public override string ToString()
        {
            return "Sound " + SoundId +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }
    }
}
