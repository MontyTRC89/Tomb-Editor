using System;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadAdditionalSoundInfoId : IWadObjectId, IEquatable<WadAdditionalSoundInfoId>, IComparable<WadAdditionalSoundInfoId>
    {
        public string Name;

        public WadAdditionalSoundInfoId(string name)
        {
            Name = name;
        }

        public int CompareTo(WadAdditionalSoundInfoId other) => Name.CompareTo(other.Name);
        public int CompareTo(object other) => CompareTo((WadAdditionalSoundInfoId)other);
        public static bool operator <(WadAdditionalSoundInfoId first, WadAdditionalSoundInfoId second) => first.Name.CompareTo(second.Name) < 0;
        public static bool operator <=(WadAdditionalSoundInfoId first, WadAdditionalSoundInfoId second) => first.Name.CompareTo(second.Name) <= 0;
        public static bool operator >(WadAdditionalSoundInfoId first, WadAdditionalSoundInfoId second) => first.Name.CompareTo(second.Name) > 0;
        public static bool operator >=(WadAdditionalSoundInfoId first, WadAdditionalSoundInfoId second) => first.Name.CompareTo(second.Name) >= 0;
        public static bool operator ==(WadAdditionalSoundInfoId first, WadAdditionalSoundInfoId second) => first.Name == second.Name;
        public static bool operator !=(WadAdditionalSoundInfoId first, WadAdditionalSoundInfoId second) => !(first == second);
        public bool Equals(WadAdditionalSoundInfoId other) => this == other;
        public override bool Equals(object other) => other is WadAdditionalSoundInfoId && this == (WadAdditionalSoundInfoId)other;
        public override int GetHashCode() => Name.GetHashCode();

        public string ToString(WadGameVersion gameVersion) => Name;
        public override string ToString() => Name;
    }

    public class WadAdditionalSoundInfo : IWadObject
    {
        private WadAdditionalSoundInfoId _id;
        private WadSoundInfo _soundInfo = WadSoundInfo.Empty;

        public WadAdditionalSoundInfoId Id
        {
            get { return _id; }
            private set
            {
                _soundInfo = _soundInfo.ChangeName(value.Name);
                _id = value;
            }
        }

        public WadSoundInfo SoundInfo
        {
            get { return _soundInfo; }
            set { _soundInfo = value.ChangeName(_id.Name); }
        }

        public WadAdditionalSoundInfo(WadAdditionalSoundInfoId id)
        {
            Id = id;
        }

        public string ToString(WadGameVersion gameVersion) => Id.ToString(gameVersion);
        public override string ToString() => Id.ToString();
        IWadObjectId IWadObject.Id => Id;
    }
}
