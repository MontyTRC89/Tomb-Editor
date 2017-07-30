using SharpDX;
using System;

namespace TombEditor.Geometry
{
    public class Portal : ObjectInstance
    {
        public PortalDirection Direction { get; set; }
        public byte NumXBlocks { get; set; }
        public byte NumZBlocks { get; set; }
        public int OtherId { get; set; }
        public int OtherIdFlipped { get; set; } = -1;
        public Room AdjoiningRoom { get; set; }
        public short PrjThingIndex { get; set; }
        public short PrjOtherThingIndex { get; set; }
        public bool PrjAdjusted { get; set; }
        public bool MemberOfFlippedRoom { get; set; }
        public bool Flipped { get; set; }
        public bool LightAveraged { get; set; }

        public Portal(int id, Room room)
            : base(ObjectInstanceType.Portal, id, room)
        {
        }

        public Portal ClonePortal()
        {
            return new Portal(0, Room)
            {
                Direction = Direction,
                X = X,
                Z = Z,
                NumXBlocks = NumXBlocks,
                NumZBlocks = NumZBlocks,
                OtherId = OtherId,
                AdjoiningRoom = AdjoiningRoom
            };
        }

        public Rectangle Area
        {
            get { return new Rectangle(X, Z, X + NumXBlocks - 1, Z + NumZBlocks - 1); }
        }

        public override ObjectInstance Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string text = "Portal ";
            if (Direction == PortalDirection.Floor)
                text += " (On Floor) ";
            if (Direction == PortalDirection.Ceiling)
                text += " (On Ceiling) ";
            text += "to Room #" + AdjoiningRoom.ToString();
            return text;
        }
    }
}
