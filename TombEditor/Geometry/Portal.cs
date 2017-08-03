using SharpDX;
using System;

namespace TombEditor.Geometry
{
    public class Portal : SectorBasedObjectInstance
    {
        [Obsolete]
        public new int Id { get { return base.Id; } }
        public PortalDirection Direction { get; set; }
        public Portal Other { get; set; }
        public Room AdjoiningRoom { get; set; }
        public bool MemberOfFlippedRoom { get; set; }
        public bool Flipped { get; set; }

        private static int _nextPortalId = 0;
        
        public Portal(Room room)
            : base(_nextPortalId++, room)
        {}

        public override ObjectInstanceType Type => ObjectInstanceType.Portal;

        public override ObjectInstance Clone()
        {
            return new Portal(Room)
            {
                Direction = Direction,
                X = X,
                Z = Z,
                NumXBlocks = NumXBlocks,
                NumZBlocks = NumZBlocks,
                Other = Other,
                AdjoiningRoom = AdjoiningRoom
            };
        }

        public override string ToString()
        {
            string text = "Portal ";
            if (Direction == PortalDirection.Floor)
                text += " (On Floor) ";
            if (Direction == PortalDirection.Ceiling)
                text += " (On Ceiling) ";
            text += "to Room " + Room.ToString();
            return text;
        }
    }
}
