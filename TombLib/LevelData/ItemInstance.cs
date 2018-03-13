using System;
using TombLib.Wad;

namespace TombLib.LevelData
{
    public abstract class ItemInstance : PositionAndScriptBasedObjectInstance, IRotateableY
    {
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

        public abstract ItemType ItemType { get; }

        public override string ToString()
        {
            return ItemType +
                   ", Room = " + (Room?.ToString() ?? "NULL") +
                   ", X = " + SectorPosition.X +
                   ", Y = " + SectorPosition.Y +
                   ", Z = " + SectorPosition.Z;
        }

        public static ItemInstance FromItemType(ItemType item)
        {
            if (item.IsStatic)
                return new StaticInstance() { WadObjectId = item.StaticId };
            else
                return new MoveableInstance() { WadObjectId = item.MoveableId };
        }
    }

    public struct ItemType
    {
        public bool IsStatic { get; }
        public WadMoveableId MoveableId { get; }
        public WadStaticId StaticId { get; }
        private readonly WadGameVersion _wadGameVersion;

        public ItemType(WadStaticId staticId, WadGameVersion wadGameVersion = WadGameVersion.TR4_TRNG)
        {
            IsStatic = true;
            MoveableId = new WadMoveableId();
            _wadGameVersion = wadGameVersion;
            StaticId = staticId;
        }

        public ItemType(WadMoveableId moveableId, WadGameVersion wadGameVersion = WadGameVersion.TR4_TRNG)
        {
            IsStatic = false;
            MoveableId = moveableId;
            _wadGameVersion = wadGameVersion;
            StaticId = new WadStaticId();
        }

        public ItemType(WadStaticId staticId, LevelSettings levelSettings) // wad can be null
            : this(staticId, levelSettings?.WadGameVersion ?? WadGameVersion.TR4_TRNG)
        { }

        public ItemType(WadMoveableId moveableId, LevelSettings levelSettings) // wad can be null
            : this(moveableId, levelSettings?.WadGameVersion ?? WadGameVersion.TR4_TRNG)
        { }

        public static bool operator ==(ItemType first, ItemType second)
        {
            // Don't compare _gameVersion because nobody really cares about that, it's only there to make ToString work.
            return (first.IsStatic == second.IsStatic) && (first.MoveableId == second.MoveableId) && (first.StaticId == second.StaticId);
        }

        public static bool operator !=(ItemType first, ItemType second) => !(first == second);

        public override bool Equals(object other) => (other is ItemType) && this == (ItemType)other;

        public override int GetHashCode()
        {
            int hashCode = IsStatic ? StaticId.GetHashCode() : MoveableId.GetHashCode();
            if (IsStatic)
                hashCode ^= unchecked((int)0xdcbc635b);
            return hashCode;
        }

        public override string ToString()
        {
            if (IsStatic)
                return StaticId.ToString(_wadGameVersion);
            else
                return MoveableId.ToString(_wadGameVersion);
        }
    }

}
