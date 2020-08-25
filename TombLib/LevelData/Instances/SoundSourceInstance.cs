using System;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public enum SoundSourcePlayMode
    {
        Always = 0,
        OnlyInBaseRoom = 1,
        OnlyInAlternateRoom = 2,
        Automatic = 3
    }

    public class SoundSourceInstance : PositionAndScriptBasedObjectInstance, IReplaceable
    {
        public int SoundId { get; set; } = -1;
        public SoundSourcePlayMode PlayMode { get; set; } = SoundSourcePlayMode.Automatic;

        // XML_SOUND_SYSTEM: legacy stuff present only for loading. Probably we'll force user to to a one-way migration on 
        // load time so we need just properties
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

        public bool IsEmpty => SoundId == -1;

        public WadSoundInfo GetSoundInfo(Level level)
        {
            if (SoundId != -1)
                return level.Settings.WadTryGetSoundInfo(SoundId);
            else
                return null;
        }

        public string SoundNameToDisplay
        {
            get
            {
                if (IsEmpty || Room == null || Room.Level == null)
                    return "Empty";
                var info = GetSoundInfo(Room.Level);
                if (info == null)
                    return "Empty";
                else
                    return info.Name;
            }
        }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Sound source " + SoundNameToDisplay +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                (ScriptId.HasValue ? ", ScriptId = " + ScriptId.Value : "");
        }

        public string PrimaryAttribDesc => "Sound ID";
        public string SecondaryAttribDesc => string.Empty;

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            return (other is SoundSourceInstance && (other as SoundSourceInstance)?.SoundId == SoundId);
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var thatSound = (SoundSourceInstance)other;
            if (!ReplaceableEquals(other) && thatSound.SoundId != SoundId)
            {
                SoundId = ((SoundSourceInstance)other).SoundId;
                return true;
            }
            else
                return false;
        }

        public string ShortName => "Sound source (ID = " + SoundId + ")" + (ScriptId.HasValue ? " <" + ScriptId.Value + ">" : "");
    }
}
