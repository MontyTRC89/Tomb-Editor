using TombLib.Wad;

namespace TombEditor.Geometry
{
    public class MoveableInstance : ItemInstance
    {
        public short Ocb { get; set; } = 0;
        public bool Invisible { get; set; } = false;
        public bool ClearBody { get; set; } = false;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

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
