using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class Portal : IObjectInstance
    {
        public PortalDirection Direction { get; set; }
        public byte NumXBlocks { get; set; }
        public byte NumZBlocks { get; set; }
        public int OtherID { get; set; }
        public int OtherIDFlipped { get; set; }
        public Room AdjoiningRoom { get; set; }
        public short PrjThingIndex { get; set; }
        public short PrjOtherThingIndex { get; set; }
        public bool PrjAdjusted { get; set; }
        public short PrjRealRoom { get; set; }
        public bool MemberOfFlippedRoom { get; set; }
        public bool Flipped { get; set; }
        public List<int> Vertices { get; set; }
        public bool LightAveraged { get; set; }

        public Portal(int id, Room room) : base(ObjectInstanceType.Portal, id, room)
        {
            OtherIDFlipped = -1;
            Vertices = new List<int>();
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
                OtherID = OtherID,
                AdjoiningRoom = AdjoiningRoom
            };
        }

        public override IObjectInstance Clone()
        {
            throw new NotImplementedException();
        }
    }
}
