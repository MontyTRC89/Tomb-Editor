using SharpDX;
using System;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum PortalDirection : byte
    {
        Floor, Ceiling, WallPositiveZ, WallNegativeZ, WallPositiveX, WallNegativeX
    }

    public enum PortalOpacity : byte
    {
        None,
        SolidFaces, // Called 'Opacity 1' in the old editor
        TraversableFaces // Called 'Opacity 2' in the old editor
    }

    public class PortalInstance : SectorBasedObjectInstance
    {
        private Room _adjoiningRoom;
        public Room AdjoiningRoom
        {
            get { return _adjoiningRoom; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("'AdjoiningRoom' must not be null");
                _adjoiningRoom = value.AlternateBaseRoom ?? value;
            }
        }
        private PortalDirection _direction;
        public PortalDirection Direction
        {
            get { return _direction; }
            set
            {
                if (Room != null)
                    throw new InvalidOperationException("Portal objects may not change in direction while they are assigned to a room.");
                _direction = value;
            }
        }
        public PortalOpacity Opacity { get; set; } = PortalOpacity.None;
        public bool HasTexturedFaces => Opacity != PortalOpacity.None;
        public bool IsTraversable => Opacity != PortalOpacity.SolidFaces;

        public PortalInstance(Rectangle area, PortalDirection direction, Room adjoiningRoom = null)
            : base(area)
        {
            AdjoiningRoom = adjoiningRoom;
            Direction = direction;
        }

        public override string ToString()
        {
            string text = "Portal ";
            switch (Direction)
            {
                case PortalDirection.Ceiling:
                    text += "(On Ceiling) ";
                    break;
                case PortalDirection.Floor:
                    text += "(On Floor) ";
                    break;
                case PortalDirection.WallNegativeX:
                    text += "(Towards -X) ";
                    break;
                case PortalDirection.WallNegativeZ:
                    text += "(Towards -Z) ";
                    break;
                case PortalDirection.WallPositiveX:
                    text += "(Towards +X) ";
                    break;
                case PortalDirection.WallPositiveZ:
                    text += "(Towards +Z) ";
                    break;
            }
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
                case PortalDirection.WallPositiveX:
                    return PortalDirection.WallNegativeX;
                case PortalDirection.WallNegativeX:
                    return PortalDirection.WallPositiveX;
                case PortalDirection.WallPositiveZ:
                    return PortalDirection.WallNegativeZ;
                case PortalDirection.WallNegativeZ:
                    return PortalDirection.WallPositiveZ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Rectangle GetOppositePortalArea(PortalDirection direction, Rectangle area)
        {
            switch (direction)
            {
                case PortalDirection.WallPositiveX:
                    return area.Offset(new DrawingPoint(-1, 0));
                case PortalDirection.WallPositiveZ:
                    return area.Offset(new DrawingPoint(0, -1));
                case PortalDirection.WallNegativeZ:
                    return area.Offset(new DrawingPoint(0, 1));
                case PortalDirection.WallNegativeX:
                    return area.Offset(new DrawingPoint(1, 0));
                default:
                    return area;
            }
        }

        // Usually this should return a portal, but be prepared for the situation that this returns null because in case of problems this might happen.
        public PortalInstance FindOppositePortal(Room room)
        {
            var adjoiningRoomArea = GetOppositePortalArea(Direction, Area).Offset(room.SectorPos).OffsetNeg(AdjoiningRoom.SectorPos);
            if (!new Rectangle(0, 0, AdjoiningRoom.NumXSectors, AdjoiningRoom.NumZSectors).Contains(adjoiningRoomArea.X, adjoiningRoomArea.Y))
                return null;

            // Check sectors
            var sector = AdjoiningRoom.GetBlockTry(adjoiningRoomArea.X, adjoiningRoomArea.Y);
            switch (Direction)
            {
                case PortalDirection.Floor:
                    return sector?.CeilingPortal; // A floor portal in this room is a ceiling portal in the adjoining room.
                case PortalDirection.Ceiling:
                    return sector?.FloorPortal; // A ceiling portal in this room is a floor portal in the adjoining room.
                default:
                    return sector?.WallPortal;
            }
        }

        public PortalInstance FindAlternatePortal(Room alternateRoom)
        {
            var sector = alternateRoom?.GetBlockTry(Area.X, Area.Y);
            switch (Direction)
            {
                case PortalDirection.Floor:
                    return sector?.FloorPortal; // A floor portal in this room is a ceiling portal in the adjoining room.
                case PortalDirection.Ceiling:
                    return sector?.CeilingPortal; // A ceiling portal in this room is a floor portal in the adjoining room.
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
                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            if (room.Blocks[x, z].FloorPortal != null)
                                throw new ApplicationException("The new floor portal '" + this + "' in room '" + room + "' overlaps with '" + room.Blocks[x, z].FloorPortal + "'!");

                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            room.Blocks[x, z].FloorPortal = this;
                    break;

                case PortalDirection.Ceiling:
                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            if (room.Blocks[x, z].CeilingPortal != null)
                                throw new ApplicationException("The new ceiling portal '" + this + "' in room '" + room + "' overlaps with '" + room.Blocks[x, z].CeilingPortal + "'!");

                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            room.Blocks[x, z].CeilingPortal = this;
                    break;

                default:
                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            if (room.Blocks[x, z].WallPortal != null)
                                throw new ApplicationException("The new wall portal '" + this + "' in room '" + room + "' overlaps with '" + room.Blocks[x, z].WallPortal + "'!");

                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            room.Blocks[x, z].WallPortal = this;
                    break;
            }
        }

        public bool ContainsPoint(DrawingPoint point)
        {
            return ContainsPoint(point.X, point.Y);
        }

        public bool ContainsPoint(int x, int z)
        {
            return (x >= Area.X && z >= Area.Y && x <= Area.X + Area.Width && z <= Area.Y + Area.Height);
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);

            // Remove portal reference
            switch (Direction)
            {
                case PortalDirection.Floor:
                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            room.Blocks[x, z].FloorPortal = null;
                    break;
                case PortalDirection.Ceiling:
                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            room.Blocks[x, z].CeilingPortal = null;
                    break;
                default:
                    for (int z = Area.Y; z <= Area.Bottom; ++z)
                        for (int x = Area.X; x <= Area.Right; ++x)
                            room.Blocks[x, z].WallPortal = null;
                    break;
            }
        }

        public override void Transform(RectTransformation transformation, DrawingPoint oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);

            if (transformation.MirrorX)
                switch (Direction)
                {
                    case PortalDirection.WallNegativeX:
                        Direction = PortalDirection.WallPositiveX;
                        break;
                    case PortalDirection.WallPositiveX:
                        Direction = PortalDirection.WallNegativeX;
                        break;
                }

            for (int i = 0; i < transformation.QuadrantRotation; ++i)
                switch (Direction)
                {
                    case PortalDirection.WallPositiveX:
                        Direction = PortalDirection.WallPositiveZ;
                        break;
                    case PortalDirection.WallPositiveZ:
                        Direction = PortalDirection.WallNegativeX;
                        break;
                    case PortalDirection.WallNegativeX:
                        Direction = PortalDirection.WallNegativeZ;
                        break;
                    case PortalDirection.WallNegativeZ:
                        Direction = PortalDirection.WallPositiveX;
                        break;
                }
        }

        public override void TransformRoomReferences(Func<Room, Room> transformRoom)
        {
            base.TransformRoomReferences(transformRoom);
            AdjoiningRoom = transformRoom(AdjoiningRoom);
        }
    }
}
