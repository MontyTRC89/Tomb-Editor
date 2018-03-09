using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadFixedSoundInfoId : IWadObjectId, IEquatable<WadFixedSoundInfoId>, IComparable<WadFixedSoundInfoId>, IComparable
    {
        public uint TypeId;

        public WadFixedSoundInfoId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadFixedSoundInfoId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadFixedSoundInfoId)other);
        public static bool operator <(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => (first.TypeId < second.TypeId);
        public static bool operator <=(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => (first.TypeId <= second.TypeId);
        public static bool operator >(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => (first.TypeId > second.TypeId);
        public static bool operator >=(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => (first.TypeId >= second.TypeId);
        public static bool operator ==(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => (first.TypeId == second.TypeId);
        public static bool operator !=(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => !(first == second);
        public bool Equals(WadFixedSoundInfoId other) => this == other;
        public override bool Equals(object other) => (other is WadFixedSoundInfoId) && this == (WadFixedSoundInfoId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(WadGameVersion gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetOriginalSoundName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(WadGameVersion.TR4_TRNG);
    }

    public class WadFixedSoundInfo : IWadObject
    {
        public WadFixedSoundInfoId Id { get; private set; }
        public WadSoundInfo SoundInfo { get; set; } = WadSoundInfo.Empty;

        public WadFixedSoundInfo(WadFixedSoundInfoId id)
        {
            Id = id;
        }

        public string ToString(WadGameVersion gameVersion) => Id.ToString(gameVersion);
        public override string ToString() => "Uncertain game version - " + ToString(WadGameVersion.TR4_TRNG);
        IWadObjectId IWadObject.Id => Id;
    }
}
