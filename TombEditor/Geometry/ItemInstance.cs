using System;
using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public abstract class ItemInstance : ObjectInstance
    {
        public uint ObjectId { get; set; }

        protected ItemInstance(int id, Room room)
            : base(id, room)
        {}

        public abstract ItemType ItemType { get; }
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
            get { return IsStatic ? ObjectInstanceType.StaticMesh : ObjectInstanceType.Moveable; }
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

    };

}
