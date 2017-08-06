using TombLib.Wad;

namespace TombEditor.Geometry
{
    public class MoveableInstance : ItemInstance
    {
        public short Ocb { get; set; } = 0;

        public MoveableInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.Moveable; }
        }

        public override ItemType ItemType
        {
            get { return new ItemType(false, WadObjectId); }
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
