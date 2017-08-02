using TombLib.Graphics;

namespace TombEditor.Geometry
{
    public class MoveableInstance : ItemInstance
    {
        public SkinnedModel Model { get; set; }

        public MoveableInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.Moveable; }
        }

        public override ItemType ItemType
        {
            get { return new ItemType(false, ObjectId); }
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
                Bits =
                {
                    [0] = Bits[0],
                    [1] = Bits[1],
                    [2] = Bits[2],
                    [3] = Bits[3],
                    [4] = Bits[4]
                },
                Model = Model,
                ObjectId = ObjectId
            };
        }

        public override string ToString()
        {
            return "Movable " + ObjectNames.GetMovableName((int)Model.ObjectID) +
                ", ID = " + Id + 
                ", Room = " + Room.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }
    }
}
