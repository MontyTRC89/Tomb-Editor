using TombLib.Wad.Catalog;

namespace TombLib.LevelData
{
    public class SpriteInstance : PositionBasedObjectInstance
    {
        public int Sequence { get; set; }
        public int Frame { get; set; }

        public ushort SpriteID
        {
            get
            {
                ushort id = 0;
                foreach (var seq in Room.Level.Settings.WadGetAllSpriteSequences())
                {
                    if (seq.Key.TypeId == Sequence)
                        return (ushort)(id + Frame);
                    else
                        id += (ushort)seq.Value.Sprites.Count;
                }

                return ushort.MaxValue; // No sprite found, return -1
            }
        }

        public bool SpriteIsValid => SpriteID != ushort.MaxValue;

        public bool SetSequenceAndFrame(int absIndex)
        {
            int currIndex = 0;
            foreach (var seq in Room.Level.Settings.WadGetAllSpriteSequences())
            {
                ushort frameIndex = 0;
                foreach (var frame in seq.Value.Sprites)
                {
                    if (absIndex == currIndex)
                    {
                        Sequence = (ushort)seq.Key.TypeId;
                        Frame = frameIndex;
                        return true;
                    }
                    else
                    {
                        frameIndex++;
                        currIndex++;
                    }
                }
            }

            return false;
        }

        private string GetSequenceName()
        {
            foreach (var seq in Room.Level.Settings.WadGetAllSpriteSequences())
                if (Sequence == seq.Key.TypeId)
                    return TrCatalog.GetSpriteSequenceName(Room.Level.Settings.GameVersion, seq.Value.Id.TypeId);

            return "Missing sequence";
        }

        public override string ToString()
        {
            return "Sprite '" + GetSequenceName() + "', Frame " + Frame +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y;
        }

        public string ShortName() => "Sprite '" + GetSequenceName() + "', Frame " + Frame;
    }
}
