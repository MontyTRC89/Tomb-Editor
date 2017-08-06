using TombLib.Wad;

namespace TombEditor.Geometry
{
    public class MoveableInstance : ItemInstance
    {
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
            return new MoveableInstance(0, Room)
            {
                X = X,
                Y = Y,
                Z = Z,
                Ocb = Ocb,
                Rotation = Rotation,
                Invisible = Invisible,
                ClearBody = ClearBody,
                CodeBits = CodeBits,
                WadObjectId = WadObjectId
            };
        }
    }
}
