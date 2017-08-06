using SharpDX;
using System;

namespace TombEditor.Geometry
{
    public class Portal : SectorBasedObjectInstance
    {
        public PortalDirection Direction { get; set; }
        public int OtherId { get; set; }
        public int OtherIdFlipped { get; set; } = -1;
        public Room AdjoiningRoom { get; set; }
        public bool MemberOfFlippedRoom { get; set; }
        public bool Flipped { get; set; }

        public Portal(int id, Room room)
            : base(id, room)
        {}

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.Portal; }
        }

        public Portal ClonePortal()
        {
            return (Portal)MemberwiseClone();
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
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
