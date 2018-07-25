using System;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public class SoundSourceInstance : PositionAndScriptBasedObjectInstance
    {
        private string _wadReferencedSoundName = null;
        public string WadReferencedSoundName
        {
            get { return _wadReferencedSoundName; }
            set
            {
                _wadReferencedSoundName = string.IsNullOrEmpty(value) ? null : value;
                _embeddedSoundInfo = null;
            }
        }

        private WadSoundInfo _embeddedSoundInfo = null;
        public WadSoundInfo EmbeddedSoundInfo
        {
            get { return _embeddedSoundInfo; }
            set
            {
                _embeddedSoundInfo = value;
                _wadReferencedSoundName = null;
            }
        }

        public bool IsEmpty => EmbeddedSoundInfo == null && string.IsNullOrEmpty(WadReferencedSoundName);

        public WadSoundInfo GetSoundInfo(Level level)
        {
            if (EmbeddedSoundInfo != null)
                return EmbeddedSoundInfo;
            return level.Settings.WadTryGetSoundInfo(WadReferencedSoundName);
        }

        public string SoundNameToDisplay
        {
            get
            {
                if (IsEmpty)
                    return "Empty";
                else if (EmbeddedSoundInfo != null)
                    return "Embedded sound";
                else
                    return "Wad sound: '" + WadReferencedSoundName + "'";
            }
        }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Sound " + SoundNameToDisplay +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }
    }
}
