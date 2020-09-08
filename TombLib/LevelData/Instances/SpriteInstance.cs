using TombLib.Wad.Catalog;

namespace TombLib.LevelData
{
    public class SpriteInstance : PositionBasedObjectInstance, IReplaceable
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

        public string PrimaryAttribDesc => "Sequence and Frame";

        public string SecondaryAttribDesc => string.Empty;

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            if (other is SpriteInstance)
            {
                var otherSprite = other as SpriteInstance;
                return (otherSprite.Frame == Frame && otherSprite.Sequence == Sequence);
            }
            else
                return false;                    
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var thatSprite = (SpriteInstance)other;
            if (!ReplaceableEquals(other))
            {
                Sequence = thatSprite.Sequence;
                Frame = thatSprite.Frame;
                return true;
            }
            else
                return false;
        }

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
            // Cases for out-of room entities, e.g. in find-and-replace dialog
            if (Room == null)
                return "Sequence " + Sequence.ToString();

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
