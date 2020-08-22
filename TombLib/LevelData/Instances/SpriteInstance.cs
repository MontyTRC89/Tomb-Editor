using TombLib.Utils;

namespace TombLib.LevelData
{
    public class SpriteInstance : PositionBasedObjectInstance
    {
        public ushort SpriteID { get; set; }

        public override string ToString()
        {
            return "Sprite ID = " + SpriteID +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y;
        }

        public string ShortName() => "Sprite ID = " + SpriteID;
    }
}
