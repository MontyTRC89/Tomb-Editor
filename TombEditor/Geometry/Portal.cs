using SharpDX;
using System;

namespace TombEditor.Geometry
{
    public enum PortalDirection : byte
    {
        Floor,
        Ceiling,
        North,
        South,
        East,
        West
    }

    public enum PortalOpacity : byte
    {
        None,
        Opacity1,
        Opacity2
    }

    public class Portal : SectorBasedObjectInstance
    {
        public PortalDirection Direction { get; }
        public Room AdjoiningRoom { get; }

        public Portal(Rectangle area, PortalDirection direction, Room adjoiningRoom)
            : base(area)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (adjoiningRoom == null)
                throw new NullReferenceException("'adjoiningRoom' must not be null");
            Direction = direction;
            AdjoiningRoom = adjoiningRoom;
        }

        public override bool CopyToFlipRooms => false;

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }

        public override SectorBasedObjectInstance Clone(Rectangle newArea)
        {
            return new Portal(newArea, Direction, AdjoiningRoom);
        }

        public override string ToString()
        {
            string text = "Portal ";
            if (Direction == PortalDirection.Floor)
                text += "(On Floor) ";
            if (Direction == PortalDirection.Ceiling)
                text += "(On Ceiling) ";
            text += "in room '" + (Room?.ToString() ?? "NULL") + "' ";
            text += "on sectors [" + Area.X + ", " + Area.Y + " to " + Area.Right + ", " + Area.Bottom + "] ";
            text += "to Room " + AdjoiningRoom;
            return text;
        }

        public static PortalDirection GetOppositeDirection(PortalDirection direction)
        {
            switch (direction)
            {
                case PortalDirection.Ceiling:
                    return PortalDirection.Floor;
                case PortalDirection.Floor:
                    return PortalDirection.Ceiling;
                case PortalDirection.East:
                    return PortalDirection.West;
                case PortalDirection.West:
                    return PortalDirection.East;
                case PortalDirection.North:
                    return PortalDirection.South;
                case PortalDirection.South:
                    return PortalDirection.North;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Rectangle GetOppositePortalArea(PortalDirection direction, Rectangle area)
        {
            switch (direction)
            {
                case PortalDirection.East:
                    return area.Offset(new DrawingPoint(-1, 0));
                case PortalDirection.North:
                    return area.Offset(new DrawingPoint(0, -1));
                case PortalDirection.South:
                    return area.Offset(new DrawingPoint(0, 1));
                case PortalDirection.West:
                    return area.Offset(new DrawingPoint(1, 0));
                default:
                    return area;
            }
        }

        // Usually this should return a portal, but be prepared for the situation that this returns null because in case of problems this might happen.
        public Portal FindOppositePortal(Room room)
        {
            var adjoiningRoomArea = GetOppositePortalArea(Direction, Area).Offset(room.SectorPos)
                .OffsetNeg(AdjoiningRoom.SectorPos);
            if (!new Rectangle(0, 0, AdjoiningRoom.NumXSectors, AdjoiningRoom.NumZSectors).Contains(adjoiningRoomArea.X,
                adjoiningRoomArea.Y))
                return null;

            // Check sectors
            var sector = AdjoiningRoom.GetBlockTry(adjoiningRoomArea.X, adjoiningRoomArea.Y);
            switch (Direction)
            {
                case PortalDirection.Floor:
                    return
                        sector?.CeilingPortal; // A floor portal in this room is a ceiling portal in the adjoining room.
                case PortalDirection.Ceiling:
                    return
                        sector?.FloorPortal; // A ceiling portal in this room is a floor portal in the adjoining room.
                default:
                    return sector?.WallPortal;
            }
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);

            // Set sector information to this portal ...
            switch (Direction)
            {
                case PortalDirection.Floor:
                    for (int x = Area.X; x <= Area.Right; ++x)
                        for (int z = Area.Y; z <= Area.Bottom; ++z)
                            if (room.Blocks[x, z].FloorPortal != null)
                                throw new ApplicationException(
                                    "The new floor portal '" + this + "' in room '" + room + "' overlaps with '" +
                                    room.Blocks[x, z].FloorPortal + "'!");
                    for (int x = Area.X; x <= Area.Right; ++x)
                        for (int z = Area.Y; z <= Area.Bottom; ++z)
                        {
                            room.Blocks[x, z].FloorPortal = this;
                            room.Blocks[x, z].FloorOpacity = PortalOpacity.None;
                        }
                    break;

                case PortalDirection.Ceiling:
                    for (int x = Area.X; x <= Area.Right; ++x)
                        for (int z = Area.Y; z <= Area.Bottom; ++z)
                            if (room.Blocks[x, z].CeilingPortal != null)
                                throw new ApplicationException(
                                    "The new floor portal '" + this + "' in room '" + room + "' overlaps with '" +
                                    room.Blocks[x, z].FloorPortal + "'!");
                    for (int x = Area.X; x <= Area.Right; ++x)
                        for (int z = Area.Y; z <= Area.Bottom; ++z)
                        {
                            room.Blocks[x, z].CeilingPortal = this;
                            room.Blocks[x, z].CeilingOpacity = PortalOpacity.None;
                        }
                    break;

                default:
                    for (int x = Area.X; x <= Area.Right; ++x)
                        for (int z = Area.Y; z <= Area.Bottom; ++z)
                            if (room.Blocks[x, z].WallPortal != null)
                                throw new ApplicationException(
                                    "The new floor portal '" + this + "' in room '" + room + "' overlaps with '" +
                                    room.Blocks[x, z].FloorPortal + "'!");
                    for (int x = Area.X; x <= Area.Right; ++x)
                        for (int z = Area.Y; z <= Area.Bottom; ++z)
                        {
                            room.Blocks[x, z].WallPortal = this;
                            room.Blocks[x, z].WallOpacity = PortalOpacity.None;
                        }
                    break;
            }
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);

            if ((room.Flipped) || (AdjoiningRoom?.Flipped ?? false))
                throw new NotImplementedException(
                    "Removing portals from rooms that are flipped is not supported just yet. :(");

            for (int x = Area.X; x <= Area.Right; x++)
            {
                for (int z = Area.Y; z <= Area.Bottom; z++)
                {
                    switch (Direction)
                    {
                        case PortalDirection.Floor:
                            room.Blocks[x, z].FloorPortal = null;
                            room.Blocks[x, z].FloorOpacity = PortalOpacity.None;
                            break;
                        case PortalDirection.Ceiling:
                            room.Blocks[x, z].CeilingPortal = null;
                            room.Blocks[x, z].CeilingOpacity = PortalOpacity.None;
                            break;
                        case PortalDirection.North:
                        case PortalDirection.South:
                        case PortalDirection.West:
                        case PortalDirection.East:
                            room.Blocks[x, z].WallPortal = null;
                            room.Blocks[x, z].WallOpacity = PortalOpacity.None;
                            break;
                    }
                }
            }
            room.UpdateCompletely();

            // Delete the corresponding opposite portal.
            // (Will invoke this routine again from the other perspective).
            var oppositePortal = FindOppositePortal(room);
            if (oppositePortal != null)
                AdjoiningRoom.RemoveObject(level, oppositePortal);
        }
    }
}
