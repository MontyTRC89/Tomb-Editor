﻿using System;
using System.Collections.Generic;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadSpriteSequenceId : IWadObjectId, IEquatable<WadSpriteSequenceId>, IComparable<WadSpriteSequenceId>
    {
        public uint TypeId;

        public WadSpriteSequenceId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadSpriteSequenceId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadSpriteSequenceId)other);
        public static bool operator <(WadSpriteSequenceId first, WadSpriteSequenceId second) => first.TypeId < second.TypeId;
        public static bool operator <=(WadSpriteSequenceId first, WadSpriteSequenceId second) => first.TypeId <= second.TypeId;
        public static bool operator >(WadSpriteSequenceId first, WadSpriteSequenceId second) => first.TypeId > second.TypeId;
        public static bool operator >=(WadSpriteSequenceId first, WadSpriteSequenceId second) => first.TypeId >= second.TypeId;
        public static bool operator ==(WadSpriteSequenceId first, WadSpriteSequenceId second) => first.TypeId == second.TypeId;
        public static bool operator !=(WadSpriteSequenceId first, WadSpriteSequenceId second) => !(first == second);
        public bool Equals(WadSpriteSequenceId other) => this == other;
        public override bool Equals(object other) => other is WadSpriteSequenceId && this == (WadSpriteSequenceId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(TRVersion.Game gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetSpriteSequenceName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(TRVersion.Game.TR4);
    }

    public class WadSpriteSequence : IWadObject
    {
        public WadSpriteSequenceId Id { get; private set; }
        public DataVersion Version { get; set; } = DataVersion.GetNext();

        public List<WadSprite> Sprites { get; private set; } = new List<WadSprite>();

        public WadSpriteSequence(WadSpriteSequenceId id)
        {
            Id = id;
        }

        public string ToString(TRVersion.Game gameVersion) => Id.ToString(gameVersion.Native());
        public override string ToString() => Id.ToString();
        IWadObjectId IWadObject.Id => Id;
    }
}
