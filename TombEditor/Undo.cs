using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib;
using TombLib.LevelData;

namespace TombEditor
{
    // Basic class for any undo type.
    // Any new undo type must be based on this base class!

    // According to command-based pattern, each undo instance has undo and complementary 
    // redo instance. In simple cases, redo command simply does back-to-back push-pop 
    // operation on both undo and redo stacks. In this situation UndoAction's UndoInstance
    // must mirror RedoInstance (see RoomUndoInstance as example).
    // In complicated cases, such as removing previously created object, UndoAction will
    // differ from RedoAction (in case of objects, we need to re-link room to newly created
    // instance), hence RedoInstance delegate has more complicated code.
    // In case redo action is impossible, just don't define RedoInstance.

    public abstract class UndoRedoInstance
    {
        public Room Room { get; internal set; }
        public UndoManager Parent { get; set; }
        public Action UndoAction { get; set; }
        public Func<UndoRedoInstance> RedoInstance { get; set; }
        public Func<bool> Valid { get; set; }
        protected UndoRedoInstance(UndoManager parent, Room room = null) { Parent = parent; Room = room; }
    }

    public class AddSectorBasedObjectUndoInstance : UndoRedoInstance
    {
        private SectorBasedObjectInstance SectorObject;

        public AddSectorBasedObjectUndoInstance(UndoManager parent, SectorBasedObjectInstance obj) : base(parent, obj.Room)
        {
            SectorObject = obj;
            Valid =()=> SectorObject != null && SectorObject.Room != null && Room != null && SectorObject.Room == Room && Room.ExistsInLevel;
            UndoAction =()=> EditorActions.DeleteObjectWithoutUpdate(SectorObject);
        }
    }

    public class AddRoomUndoInstance : UndoRedoInstance
    {
        public AddRoomUndoInstance(UndoManager parent, Room room) : base(parent, room)
        {
            Valid =()=> Room != null && Room.ExistsInLevel && Room.AnyObjects.Count() == 0;
            UndoAction =()=> EditorActions.DeleteRooms(new[] { Room });
        }
    }

    public class AddAdjoiningRoomUndoInstance : UndoRedoInstance
    {
        private Room ParentRoom;

        public AddAdjoiningRoomUndoInstance(UndoManager parent, Room room) : base(parent, room)
        {
            ParentRoom = room.Portals.Count() == 1 ? room.Portals.First().AdjoiningRoom : null;

            Valid =()=> Room != null && ParentRoom != null &&
                        Room.ExistsInLevel && ParentRoom.ExistsInLevel &&
                        Room.Portals.Count(p => p.AdjoiningRoom == ParentRoom) == 1 && Room.AnyObjects.Count(obj => (obj is PortalInstance)) == 1 &&
                        Room.AnyObjects.Count(obj => !(obj is PortalInstance)) == 0;

            UndoAction =()=>
            {
                Parent.Editor.SelectedRoom = ParentRoom;
                EditorActions.DeleteRooms(new[] { Room });
            };
        }
    }

    public class MoveRoomsUndoInstance : UndoRedoInstance
    {
        VectorInt3 Delta;
        List<Room> Rooms;
        Dictionary<Room, VectorInt2> Sizes = new Dictionary<Room, VectorInt2>();

        public MoveRoomsUndoInstance(UndoManager parent, List<Room> rooms, VectorInt3 delta) : base(parent)
        {
            Delta = -delta;
            Rooms = rooms;
            Rooms.ForEach(room => Sizes.Add(room, room.SectorSize));

            Valid =()=> Rooms != null && Rooms.All(room => room != null && room.ExistsInLevel && !room.Locked) && 
                                         Rooms.All(room => !Parent.Editor.Level.GetConnectedRooms(room).Except(Rooms).Any()) &&
                                         Rooms.All(room => Sizes.ContainsKey(room) && room.SectorSize == Sizes[room]);
            UndoAction =()=> EditorActions.MoveRooms(Delta, Rooms, true);
            RedoInstance =()=> new MoveRoomsUndoInstance(Parent, Rooms, Delta);
        }
    }

    public class AddRemoveObjectUndoInstance : UndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private bool Created;

        public AddRemoveObjectUndoInstance(UndoManager parent, PositionBasedObjectInstance obj, bool created) : base(parent, obj.Room)
        {
            Created = created;
            UndoObject = obj;

            Valid =()=> UndoObject != null && ((Created && UndoObject.Room != null) || 
                       (!Created && Room.ExistsInLevel && Room.LocalArea.Width > UndoObject.SectorPosition.X && Room.LocalArea.Height > UndoObject.SectorPosition.Y));

            UndoAction =()=>
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

            RedoInstance =()=>
            {
                var result = new AddRemoveObjectUndoInstance(Parent, UndoObject, !Created);
                result.Room = Room; // Relink parent room
                return result;
            };
        }
    }

    public class TransformObjectUndoInstance : UndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private Vector3 Position;

        // Optional fields for various interface types

        private float? Scale     = null;
        private float? RotationY = null;
        private float? RotationX = null;
        private float? Roll      = null;

        public TransformObjectUndoInstance(UndoManager parent, PositionBasedObjectInstance obj) : base(parent, obj.Room)
        {
            UndoObject = obj;
            Position = obj.Position;

            if (obj is IScaleable) Scale = ((IScaleable)obj).Scale;
            if (obj is IRotateableY) RotationY = ((IRotateableY)obj).RotationY;
            if (obj is IRotateableYX) RotationX = ((IRotateableYX)obj).RotationX;
            if (obj is IRotateableYXRoll) Roll = ((IRotateableYXRoll)obj).Roll;

            Valid =()=> UndoObject != null && UndoObject.Room != null && Room.ExistsInLevel;

            UndoAction =()=>
            {
                bool roomChanged = false;

                if(UndoObject.Room != Room)
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

                if(!roomChanged)
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Change);
            };
            RedoInstance =()=> new TransformObjectUndoInstance(Parent, UndoObject);
        }
    }

    public class GeometryUndoInstance : UndoRedoInstance
    {
        private RectangleInt2 Area;
        private Block[,] Blocks;

        public GeometryUndoInstance(UndoManager parent, Room room) : base(parent, room)
        {
            Area.Start = VectorInt2.Zero;
            Area.End = new VectorInt2(room.NumXSectors, room.NumZSectors);

            Blocks = new Block[Area.Size.X + 1, Area.Size.Y + 1];

            for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                    Blocks[i, j] = Room.Blocks[x, z].Clone();

            Valid =()=> (Room != null && Room.ExistsInLevel && Room.SectorSize == Area.Size);

            UndoAction =()=>
            {
                for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                    for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                    {
                        var origin = Room.Blocks[x, z];
                        var replacement = Blocks[i, j];

                        if (origin.Type != BlockType.BorderWall) origin.Type = replacement.Type;

                        origin.Flags = replacement.Flags;
                        origin.ForceFloorSolid = replacement.ForceFloorSolid;

                        for (BlockFace face = 0; face < BlockFace.Count; face++)
                        {
                            var texture = replacement.GetFaceTexture(face);
                            if (texture.TextureIsInvisible || Parent.Editor.Level.Settings.Textures.Contains(texture.Texture))
                                origin.SetFaceTexture(face, texture);
                        }

                        for (BlockEdge edge = 0; edge < BlockEdge.Count; edge++)
                        {
                            origin.SetHeight(BlockVertical.Ed, edge, replacement.GetHeight(BlockVertical.Ed, edge));
                            origin.SetHeight(BlockVertical.Rf, edge, replacement.GetHeight(BlockVertical.Rf, edge));
                        }

                        origin.Floor = replacement.Floor;
                        origin.Ceiling = replacement.Ceiling;
                    }

                Parent.Editor.RoomGeometryChange(Room);
                Parent.Editor.RoomSectorPropertiesChange(Room);
                var relevantRooms = room.Portals.Select(p => p.AdjoiningRoom);
                Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());

                Room.BuildGeometry();
                foreach (Room relevantRoom in relevantRooms)
                {
                    Parent.Editor.RoomGeometryChange(relevantRoom);
                    Parent.Editor.RoomSectorPropertiesChange(relevantRoom);
                }
            };
            RedoInstance =()=> new GeometryUndoInstance(Parent, Room);
        }
    }

    public class UndoManager
    {
        private class UndoRedoStack
        {
            private List<UndoRedoInstance>[] items;
            private int top = -1;

            public bool Reversible => !Empty && items[top].Any(item => item.RedoInstance != null);
            public bool Empty => top == -1;
            public int Count => items.Length;

            public UndoRedoStack(int capacity) { items = new List<UndoRedoInstance>[capacity]; }

            public void Resize(int newSize)
            {
                if (newSize == items.Length) return;
                newSize = newSize < 1 ? 1 : newSize;

                // In case new size is smaller, cut stack from bottom
                if (top != -1 && top > newSize - 1 && items.Length > newSize)
                {
                    var newItems = new List<UndoRedoInstance>[newSize];
                    Array.Copy(items, top - newSize + 1, newItems, 0, newSize);
                    top = newSize - 1;
                    items = newItems;
                }
                else
                    Array.Resize(ref items, newSize);
            }

            public void Push(List<UndoRedoInstance> item)
            {
                // Rotate stack if full
                var c = items.Length - 1;
                if (top == c)
                    for (int i = 1; i <= c; i++)
                        items[i - 1] = items[i];
                else
                    top++;

                items[top] = item;
            }

            public List<UndoRedoInstance> Pop()
            {
                if (top == -1) return null;
                var result = items[top];
                items[top--] = null;
                return result;
            }

            public void Clear()
            {
                top = -1;
                for (int i = 0; i < items.Length; i++)
                    items[i] = null;
            }
        }

        private const int MaxUndoDepth = 1000;

        public Editor Editor { get; private set; }
        private UndoRedoStack _undoStack;
        private UndoRedoStack _redoStack;

        public UndoManager(Editor editor, int undoDepth)
        {
            undoDepth = MathC.Clamp(undoDepth, 1, MaxUndoDepth);

            Editor = editor;
            _undoStack = new UndoRedoStack(undoDepth);
            _redoStack = new UndoRedoStack(undoDepth);
        }

        public void Push(List<UndoRedoInstance> instance)
        {
            if (!StackValid(_undoStack)) return;

            _undoStack.Push(instance);
            _redoStack.Clear();
            Editor.UndoStackChanged();
        }
        public void Push(UndoRedoInstance instance) => Push(new List<UndoRedoInstance> { instance });

        public void Resize(int newSize)
        {
            if (newSize == _undoStack.Count) return;
            newSize = MathC.Clamp(newSize, 1, MaxUndoDepth);
            _undoStack.Resize(newSize);
            _redoStack.Resize(newSize);
            Editor.UndoStackChanged();
        }

        public void ClearAll()
        {
            Clear(_undoStack, true);
            Clear(_redoStack, true);
            Editor.UndoStackChanged();
        }

        public void PushRoomCreated(Room room) => Push(new AddRoomUndoInstance(this, room));
        public void PushAdjoiningRoomCreated(Room room) => Push(new AddAdjoiningRoomUndoInstance(this, room));
        public void PushRoomsMoved(List<Room> rooms, VectorInt3 delta) { if(delta != VectorInt3.Zero) Push(new MoveRoomsUndoInstance(this, rooms, delta)); }
        public void PushSectorObjectCreated(SectorBasedObjectInstance obj) => Push(new AddSectorBasedObjectUndoInstance(this, obj));
        public void PushGeometryChanged(Room room) => Push(new GeometryUndoInstance(this, room));
        public void PushGeometryChanged(List<Room> rooms) => Push(rooms.Select(room => (new GeometryUndoInstance(this, room)) as UndoRedoInstance).ToList());
        public void PushObjectCreated(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, true));
        public void PushObjectDeleted(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, false));
        public void PushObjectTransformed(PositionBasedObjectInstance obj) => Push(new TransformObjectUndoInstance(this, obj));

        public void Redo() => Engage(_redoStack);
        public void Undo() => Engage(_undoStack);
        public void UndoClear() => Clear(_undoStack);
        public void RedoClear() => Clear(_redoStack);
        public bool UndoPossible => StackValid(_undoStack) && !_undoStack.Empty;
        public bool RedoPossible => StackValid(_redoStack) && !_redoStack.Empty;
        public bool UndoReversible => _undoStack.Reversible;
        public bool RedoReversible => _redoStack.Reversible;

        private bool StackValid(UndoRedoStack stack) => (stack != null && stack.Count > 0);

        private void Clear(UndoRedoStack stack, bool silent = false)
        {
            if (!StackValid(stack)) return;
            stack.Clear();
            if (!silent) Editor.UndoStackChanged();
        }

        private void Engage(UndoRedoStack stack)
        {
            if (!StackValid(stack)) return;
            var counterStack = stack == _undoStack ? _redoStack : _undoStack;

            var instance = stack.Pop();
            if (instance == null) return;

            if(instance.All(item => item.Valid == null || item.Valid()))
            {
                // Generate and push redo instance, if exists. If not, reset redo stack, since action is irreversible.
                var redoList = instance.Where(item => item.RedoInstance != null).ToList().ConvertAll(item => item.RedoInstance());
                if (redoList.Count > 0)
                    counterStack.Push(redoList);
                else
                    counterStack.Clear();

                // Invoke original undo instance
                instance.ForEach(item => item.UndoAction?.Invoke());
            }
            else
                Editor.SendMessage("Level state changed. " + (counterStack == _redoStack ? "Undo" : "Redo") + " action ignored.", TombLib.Forms.PopupType.Warning);

            Editor.UndoStackChanged();
        }
    }
}
