﻿using System;
using TombLib.LevelData;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadFixedSoundInfoId : IWadObjectId, IEquatable<WadFixedSoundInfoId>, IComparable<WadFixedSoundInfoId>
    {
        public uint TypeId;

        public WadFixedSoundInfoId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadFixedSoundInfoId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadFixedSoundInfoId)other);
        public static bool operator <(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => first.TypeId < second.TypeId;
        public static bool operator <=(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => first.TypeId <= second.TypeId;
        public static bool operator >(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => first.TypeId > second.TypeId;
        public static bool operator >=(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => first.TypeId >= second.TypeId;
        public static bool operator ==(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => first.TypeId == second.TypeId;
        public static bool operator !=(WadFixedSoundInfoId first, WadFixedSoundInfoId second) => !(first == second);
        public bool Equals(WadFixedSoundInfoId other) => this == other;
        public override bool Equals(object other) => other is WadFixedSoundInfoId && this == (WadFixedSoundInfoId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(TRVersion.Game gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetOriginalSoundName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(TRVersion.Game.TR4);
    }

    public class WadFixedSoundInfo : IWadObject
    {
        public WadFixedSoundInfoId Id { get; private set; }
        public WadSoundInfo SoundInfo { get; set; } = new WadSoundInfo(-1);

        public WadFixedSoundInfo(WadFixedSoundInfoId id)
        {
            Id = id;
        }

        public string ToString(TRVersion.Game gameVersion) => Id.ToString(gameVersion.Native());
        public override string ToString() => Id.ToString();
        IWadObjectId IWadObject.Id => Id;
    }
}
