using TombLib.Wad.Catalog;

namespace TombLib.LevelData
{
    public class SpriteInstance : PositionBasedObjectInstance
    {
        public ushort SpriteID { get; set; }

        private string GetSequenceName()
        {
            uint index = 0;

            foreach (var seq in Room.Level.Settings.WadGetAllSpriteSequences())
                foreach (var spr in seq.Value.Sprites)
                {
                    if (index == SpriteID)
                        return TrCatalog.GetSpriteSequenceName(Room.Level.Settings.GameVersion, seq.Value.Id.TypeId);
                    index++;
                }

            return "Missing sequence";
        }

        public override string ToString()
        {
            return "Sprite ID = " + SpriteID + " (" + GetSequenceName() + ")" +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y;
        }

        public string ShortName() => "Sprite (ID = " + SpriteID + ", " + GetSequenceName() + ")";
    }
}
