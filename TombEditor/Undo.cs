using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor
{
    public abstract class EditorUndoRedoInstance : UndoRedoInstance
    {
        public EditorUndoManager Parent;

        public Room Room { get; internal set; }
        protected EditorUndoRedoInstance(EditorUndoManager parent, Room room) { Parent = parent; Room = room; }
    }

    public class AddSectorBasedObjectUndoInstance : EditorUndoRedoInstance
    {
        private SectorBasedObjectInstance SectorObject;

        public AddSectorBasedObjectUndoInstance(EditorUndoManager parent, SectorBasedObjectInstance obj) : base(parent, obj.Room)
        {
            SectorObject = obj;
            Valid = () => SectorObject != null && SectorObject.Room != null && Room != null && SectorObject.Room == Room && Room.ExistsInLevel;
            UndoAction = () => EditorActions.DeleteObjectWithoutUpdate(SectorObject);
        }
    }

    public class AddRoomUndoInstance : EditorUndoRedoInstance
    {
        public AddRoomUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            Valid = () => Room != null && Room.ExistsInLevel && Room.AnyObjects.Count() == 0;
            UndoAction = () => EditorActions.DeleteRooms(new[] { Room });
        }
    }

    public class AddAdjoiningRoomUndoInstance : EditorUndoRedoInstance
    {
        private Room ParentRoom;

        public AddAdjoiningRoomUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            ParentRoom = room.Portals.Count() == 1 ? room.Portals.First().AdjoiningRoom : null;

            Valid = () => Room != null && ParentRoom != null &&
                         Room.ExistsInLevel && ParentRoom.ExistsInLevel &&
                         Room.Portals.Count(p => p.AdjoiningRoom == ParentRoom) == 1 && Room.AnyObjects.Count(obj => (obj is PortalInstance)) == 1 &&
                         Room.AnyObjects.Count(obj => !(obj is PortalInstance)) == 0;

            UndoAction = () =>
            {
                Parent.Editor.SelectedRoom = ParentRoom;
                EditorActions.DeleteRooms(new[] { Room });
            };
        }
    }

    public class MoveRoomsUndoInstance : EditorUndoRedoInstance
    {
        VectorInt3 Delta;
        List<Room> Rooms;
        Dictionary<Room, VectorInt2> Sizes = new Dictionary<Room, VectorInt2>();

        public MoveRoomsUndoInstance(EditorUndoManager parent, List<Room> rooms, VectorInt3 delta) : base(parent, null)
        {
            Delta = -delta;
            Rooms = rooms;
            Rooms.ForEach(room => Sizes.Add(room, room.SectorSize));

            Valid = () => Rooms != null && Rooms.All(room => room != null && room.ExistsInLevel && !room.Locked) &&
                                          Rooms.All(room => !Parent.Editor.Level.GetConnectedRooms(room).Except(Rooms).Any()) &&
                                          Rooms.All(room => Sizes.ContainsKey(room) && room.SectorSize == Sizes[room]);
            UndoAction = () => EditorActions.MoveRooms(Delta, Rooms, true);
            RedoInstance = () => new MoveRoomsUndoInstance(Parent, Rooms, Delta);
        }
    }

    public class AddRemoveObjectUndoInstance : EditorUndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private bool Created;

        public AddRemoveObjectUndoInstance(EditorUndoManager parent, PositionBasedObjectInstance obj, bool created) : base(parent, obj.Room)
        {
            Created = created;
            UndoObject = obj;

            Valid = () => UndoObject != null && ((Created && UndoObject.Room != null) ||
                        (!Created && Room.ExistsInLevel && Room.LocalArea.Width > UndoObject.SectorPosition.X && Room.LocalArea.Height > UndoObject.SectorPosition.Y));

            UndoAction = () =>
            {
                if (Created)
                    EditorActions.DeleteObjectWithoutUpdate(UndoObject);
                else
                {
                    var backupPos = obj.Position; // Preserve original position and reassign it after placement
                    EditorActions.PlaceObjectWithoutUpdate(Room, obj.SectorPosition, UndoObject);
                    EditorActions.MoveObject(UndoObject, backupPos);
                }
            };

            RedoInstance = () =>
            {
                var result = new AddRemoveObjectUndoInstance(Parent, UndoObject, !Created);
                result.Room = Room; // Relink parent room
                return result;
            };
        }
    }

    public class TransformObjectUndoInstance : EditorUndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private Vector3 Position;

        // Optional fields for various interface types

        private float? Scale = null;
        private float? RotationY = null;
        private float? RotationX = null;
        private float? Roll = null;

        public TransformObjectUndoInstance(EditorUndoManager parent, PositionBasedObjectInstance obj) : base(parent, obj.Room)
        {
            UndoObject = obj;
            Position = obj.Position;

            if (obj is IScaleable) Scale = ((IScaleable)obj).Scale;
            if (obj is IRotateableY) RotationY = ((IRotateableY)obj).RotationY;
            if (obj is IRotateableYX) RotationX = ((IRotateableYX)obj).RotationX;
            if (obj is IRotateableYXRoll) Roll = ((IRotateableYXRoll)obj).Roll;

            Valid = () => UndoObject != null && UndoObject.Room != null && Room.ExistsInLevel;

            UndoAction = () =>
            {
                bool roomChanged = false;

                if (UndoObject.Room != Room)
                {
                    var oldRoom = UndoObject.Room;
                    oldRoom.RemoveObject(Parent.Editor.Level, UndoObject);
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Remove, oldRoom);

                    Room.AddObject(Parent.Editor.Level, UndoObject);
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Add);

                    roomChanged = true;
                }

                UndoObject.Position = Position;
                if (UndoObject is IScaleable && Scale.HasValue) ((IScaleable)obj).Scale = Scale.Value;
                if (UndoObject is IRotateableY && RotationY.HasValue) ((IRotateableY)obj).RotationY = RotationY.Value;
                if (UndoObject is IRotateableYX && RotationX.HasValue) ((IRotateableYX)obj).RotationX = RotationX.Value;
                if (UndoObject is IRotateableYXRoll && Roll.HasValue) ((IRotateableYXRoll)obj).Roll = Roll.Value;

                if (UndoObject is LightInstance)
                    Room.BuildGeometry(); // Rebuild lighting!

                if (!roomChanged)
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Change);
            };
            RedoInstance = () => new TransformObjectUndoInstance(Parent, UndoObject);
        }
    }

    public class GeometryUndoInstance : EditorUndoRedoInstance
    {
        private RectangleInt2 Area;
        private Block[,] Blocks;

        public GeometryUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            Area.Start = VectorInt2.Zero;
            Area.End = new VectorInt2(room.NumXSectors, room.NumZSectors);

            Blocks = new Block[Area.Size.X + 1, Area.Size.Y + 1];

            for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                    Blocks[i, j] = Room.Blocks[x, z].Clone();

            Valid = () => (Room != null && Room.ExistsInLevel && Room.SectorSize == Area.Size);

            UndoAction = () =>
            {
                for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                    for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                        Room.Blocks[x, z].ReplaceGeometry(Parent.Editor.Level, Blocks[i, j]);

                Room.BuildGeometry();
                Parent.Editor.RoomGeometryChange(Room);
                Parent.Editor.RoomSectorPropertiesChange(Room);
                var relevantRooms = room.Portals.Select(p => p.AdjoiningRoom);
                Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());

                foreach (Room relevantRoom in relevantRooms)
                    Parent.Editor.RoomGeometryChange(relevantRoom);
            };
            RedoInstance = () => new GeometryUndoInstance(Parent, Room);
        }
    }

    public class EditorUndoManager : UndoManager
    {
        public Editor Editor;

        public EditorUndoManager(Editor editor, int undoDepth) : base(undoDepth)
        {
            Editor = editor;

            UndoStackChanged += (s, e) => Editor.UndoStackChanged();
            MessageSent += (s, e) => Editor.SendMessage(e.Message, TombLib.Forms.PopupType.Warning);
        }

        public void PushRoomCreated(Room room) => Push(new AddRoomUndoInstance(this, room));
        public void PushAdjoiningRoomCreated(Room room) => Push(new AddAdjoiningRoomUndoInstance(this, room));
        public void PushRoomsMoved(List<Room> rooms, VectorInt3 delta) { if (delta != VectorInt3.Zero) Push(new MoveRoomsUndoInstance(this, rooms, delta)); }
        public void PushSectorObjectCreated(SectorBasedObjectInstance obj) => Push(new AddSectorBasedObjectUndoInstance(this, obj));
        public void PushGeometryChanged(Room room) => Push(new GeometryUndoInstance(this, room));
        public void PushGeometryChanged(List<Room> rooms) => Push(rooms.Select(room => (new GeometryUndoInstance(this, room)) as UndoRedoInstance).ToList());
        public void PushObjectCreated(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, true));
        public void PushObjectDeleted(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, false));
        public void PushObjectTransformed(PositionBasedObjectInstance obj) => Push(new TransformObjectUndoInstance(this, obj));
    }
}
