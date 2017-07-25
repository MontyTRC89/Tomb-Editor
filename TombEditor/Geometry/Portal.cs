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
        public short AdjoiningRoom { get; set; }
        public short PrjThingIndex { get; set; }
        public short PrjOtherThingIndex { get; set; }
        public bool PrjAdjusted { get; set; }
        public short PrjRealRoom { get; set; }
        public bool MemberOfFlippedRoom { get; set; }
        public bool Flipped { get; set; }
        public List<int> Vertices { get; set; }
        public bool LightAveraged { get; set; }

        public Portal(int id, short room) : base(ObjectInstanceType.Portal, id, room)
        {
            OtherIDFlipped = -1;
            Vertices = new List<int>();
        }

        public Portal ClonePortal()
        {
            Portal p = new Geometry.Portal(0, 0);

            p.Direction = Direction;
            p.X = X;
            p.Z = Z;
            p.NumXBlocks = NumXBlocks;
            p.NumZBlocks = NumZBlocks;
            p.OtherID = OtherID;
            p.AdjoiningRoom = AdjoiningRoom;
            p.Room = Room;
            
            return p;
        }

        public override IObjectInstance Clone()
        {
            throw new NotImplementedException();
        }
    }
}
