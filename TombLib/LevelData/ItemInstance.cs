using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public abstract class ItemInstance : PositionAndScriptBasedObjectInstance, IRotateableY
    {
        // Don't use a reference here because the loaded Wad might not
        // contain each required object. Additionally the loaded Wads
        // can change. It would be unnecesary difficult to update all this references.
        public uint WadObjectId { get; set; }

        private float _rotation;
        /// <summary> Rotation in radians in the interval [0, 360). The value is range reduced. </summary>
        public float RotationY
        {
            get { return _rotation; }
            set { _rotation = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        /// <summary> Rotation in radians in the interval [0, 2*Pi). The value is range reduced. </summary>
        public float RotationYRadians
        {
            get { return RotationY * (float)(Math.PI / 180); }
            set { RotationY = value * (float)(180 / Math.PI); }
        }

        public Vector4 Color { get; set; } = new Vector4(1.0f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)

        public abstract ItemType ItemType { get; }

        public override string ToString()
        {
            return ItemType.ToString() +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }

        public static ItemInstance FromItemType(ItemType item)
        {
            if (item.IsStatic)
                return new StaticInstance() { WadObjectId = item.Id };
            else
                return new MoveableInstance() { WadObjectId = item.Id };
        }
    }

    public struct ItemType
    {
        public bool IsStatic { get; set; }
        public uint Id { get; set; }
        public WadTombRaiderVersion GameVersion { get; set; }

        public ItemType(bool isStatic, uint id, WadTombRaiderVersion gameVersion)
        {
            IsStatic = isStatic;
            Id = id;
            GameVersion = gameVersion;
        }

        public static bool operator ==(ItemType first, ItemType second)
        {
            return (first.IsStatic == second.IsStatic) && (first.Id == second.Id);
        }

        public static bool operator !=(ItemType first, ItemType second)
        {
            return (first.IsStatic != second.IsStatic) || (first.Id != second.Id);
        }

        public override bool Equals(object obj)
        {
            return this == (ItemType)obj;
        }

        public override int GetHashCode()
        {
            return unchecked((int)Id) ^ (IsStatic ? unchecked((int)0xdcbc635b) : 0);
        }

        public override string ToString()
        {
            if (IsStatic)
                return TombLib.Wad.Catalog.TrCatalog.GetStaticName(GameVersion, Id);
            else
                return TombLib.Wad.Catalog.TrCatalog.GetMoveableName(GameVersion, Id);
        }
    };

}
