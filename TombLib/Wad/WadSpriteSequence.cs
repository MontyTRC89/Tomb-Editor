using System;
using System.Collections.Generic;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadSpriteSequenceId : IWadObjectId, IEquatable<WadSpriteSequenceId>, IComparable<WadSpriteSequenceId>, IComparable
    {
        public uint TypeId;

        public WadSpriteSequenceId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadSpriteSequenceId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadSpriteSequenceId)other);
        public static bool operator <(WadSpriteSequenceId first, WadSpriteSequenceId second) => (first.TypeId < second.TypeId);
        public static bool operator <=(WadSpriteSequenceId first, WadSpriteSequenceId second) => (first.TypeId <= second.TypeId);
        public static bool operator >(WadSpriteSequenceId first, WadSpriteSequenceId second) => (first.TypeId > second.TypeId);
        public static bool operator >=(WadSpriteSequenceId first, WadSpriteSequenceId second) => (first.TypeId >= second.TypeId);
        public static bool operator ==(WadSpriteSequenceId first, WadSpriteSequenceId second) => (first.TypeId == second.TypeId);
        public static bool operator !=(WadSpriteSequenceId first, WadSpriteSequenceId second) => !(first == second);
        public bool Equals(WadSpriteSequenceId other) => this == other;
        public override bool Equals(object other) => (other is WadSpriteSequenceId) && this == (WadSpriteSequenceId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(WadGameVersion gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetSpriteSequenceName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(WadGameVersion.TR4_TRNG);
    }

    public class WadSpriteSequence : IWadObject
    {
        public WadSpriteSequenceId Id { get; private set; }

        public List<WadSprite> Sprites { get; private set; } = new List<WadSprite>();

        public WadSpriteSequence(WadSpriteSequenceId id)
        {
            Id = id;
        }

        public string ToString(WadGameVersion gameVersion) => Id.ToString(gameVersion);
        public override string ToString() => "Uncertain game version - " + ToString(WadGameVersion.TR4_TRNG);
        IWadObjectId IWadObject.Id => Id;
    }
}
