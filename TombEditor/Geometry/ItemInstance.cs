using System;
using System.Collections.Generic;
using TombLib.Wad;

namespace TombEditor.Geometry
{
    public abstract class ItemInstance : ObjectInstance
    {
        // Don't use a reference here because the loaded Wad might not
        // contain each required object. Additionally the loaded Wads
        // can change. It would be unnecesary difficult to update all this references.
        public uint WadObjectId { get; set; }

        private float _rotation;
        /// <summary> Rotation in radians in the interval [0, 360). The value is range reduced. </summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        /// <summary> Rotation in radians in the interval [0, 2*Pi). The value is range reduced. </summary>
        public float RotationRadians
        {
            get { return Rotation * (float)(Math.PI / 180); }
            set { Rotation = value * (float)(180 / Math.PI); }
        }

        protected ItemInstance(int id, Room room)
            : base(id, room)
        {}

        public abstract ItemType ItemType { get; }
        
        public override string ToString()
        {
            return ItemType.ToString() +
                ", ID = " + Id +
                ", Room = " + Room.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }

        public static ItemInstance FromItemType(int id, Room room, ItemType item)
        {
            if (item.IsStatic)
                return new StaticInstance(id, room) { WadObjectId = item.Id };
            else
                return new MoveableInstance(id, room) { WadObjectId = item.Id };
        }
    }

    public struct ItemType
    {
        public bool IsStatic { get; set; }
        public uint Id { get; set; }

        public ItemType(bool isStatic, uint id)
        {
            IsStatic = isStatic;
            Id = id;
        }

        public ObjectInstanceType ObjectInstanceType
        {
            get { return IsStatic ? ObjectInstanceType.Static : ObjectInstanceType.Moveable; }
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
                return "Static (" + Id + ") " + ObjectNames.GetStaticName(Id);
            else
                return "Moveable (" + Id + ") " + ObjectNames.GetMoveableName(Id);
        }
    };

}
