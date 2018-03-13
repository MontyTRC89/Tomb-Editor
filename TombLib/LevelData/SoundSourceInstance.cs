namespace TombLib.LevelData
{
    public class SoundSourceInstance : PositionAndScriptBasedObjectInstance
    {
        public string SoundName { get; set; } = "";
        public short Flags { get; set; } = 0;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

        public override bool CopyToFlipRooms => false;

        public override string ToString()
        {
            return "Sound " + SoundName +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }
    }
}
