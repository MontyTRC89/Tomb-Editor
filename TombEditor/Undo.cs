using System;
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

    public abstract class UndoRedoInstance
    {
        public UndoManager Parent { get; set; }
        public Action UndoAction { get; set; }
        public Func<UndoRedoInstance> RedoInstance { get; set; }
        protected UndoRedoInstance(UndoManager parent) { Parent = parent; }

        public void BrokenWarning() => Parent.Editor.SendMessage("Level state changed. Undo action ignored.", TombLib.Forms.PopupType.Warning);
    }

    public class AddRemoveObjectUndoInstance : UndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private Room Room;
        private bool Created;

        public AddRemoveObjectUndoInstance(UndoManager parent, PositionBasedObjectInstance obj, bool created) : base(parent)
        {
            Created = created;
            UndoObject = obj;
            Room = obj.Room;

            UndoAction =()=>
            {
                if (Created)
                {
                    if (UndoObject.Room != null)
                        EditorActions.DeleteObjectWithoutUpdate(UndoObject);
                    else
                        BrokenWarning();
                }
                else
                {
                    if (Parent.Editor.Level.RoomExists(Room))
                    {
                        var backupPos = obj.Position; // Preserve original position and reassign it after placement
                        VectorInt2 pos = new VectorInt2((int)((obj.Position.X - 512.0f) / 1024.0f), (int)((obj.Position.Z - 512.0f) / 1024.0f));
                        EditorActions.PlaceObjectWithoutUpdate(Room, pos, UndoObject);
                        EditorActions.MoveObject(UndoObject, backupPos);
                    }
                    else
                        BrokenWarning();

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

        public TransformObjectUndoInstance(UndoManager parent, PositionBasedObjectInstance obj) : base(parent)
        {
            UndoObject = obj;
            Position = obj.Position;

            if (obj is IScaleable) Scale = ((IScaleable)obj).Scale;
            if (obj is IRotateableY) RotationY = ((IRotateableY)obj).RotationY;
            if (obj is IRotateableYX) RotationX = ((IRotateableYX)obj).RotationX;
            if (obj is IRotateableYXRoll) Roll = ((IRotateableYXRoll)obj).Roll;

            UndoAction =()=>
            {
                if (UndoObject.Room != null)
                {
                    UndoObject.Position = Position;
                    if (UndoObject is IScaleable && Scale.HasValue) ((IScaleable)obj).Scale = Scale.Value;
                    if (UndoObject is IRotateableY && RotationY.HasValue) ((IRotateableY)obj).RotationY = RotationY.Value;
                    if (UndoObject is IRotateableYX && RotationX.HasValue) ((IRotateableYX)obj).RotationX = RotationX.Value;
                    if (UndoObject is IRotateableYXRoll && Roll.HasValue) ((IRotateableYXRoll)obj).Roll = Roll.Value;

                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Change);
                }
                else
                    BrokenWarning();
            };

            RedoInstance =()=> new TransformObjectUndoInstance(Parent, UndoObject);
        }
    }

    public class GeometryUndoInstance : UndoRedoInstance
    {
        private Room UndoRoom;
        private RectangleInt2 Area;
        private Block[,] Blocks;

        public GeometryUndoInstance(UndoManager parent, Room room) : base(parent)
        {
            UndoRoom = room;
            Area.Start = VectorInt2.Zero;
            Area.End = new VectorInt2(room.NumXSectors, room.NumZSectors);

            Blocks = new Block[Area.Size.X + 1, Area.Size.Y + 1];

            for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                    Blocks[i, j] = UndoRoom.Blocks[x, z].Clone();

            UndoAction =()=>
            {
                if (UndoRoom != null && Parent.Editor.Level.RoomExists(UndoRoom) &&
                    UndoRoom.NumXSectors == Area.Size.X && UndoRoom.NumZSectors == Area.Size.Y)
                {
                    for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                        for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                        {
                            var origin = UndoRoom.Blocks[x, z];
                            var replacement = Blocks[i, j];

                            if (origin.Type != BlockType.BorderWall) origin.Type = replacement.Type;

                            origin.Flags = replacement.Flags;
                            origin.ForceFloorSolid = replacement.ForceFloorSolid;

                            for (BlockFace face = 0; face < BlockFace.Count; face++)
                            {
                                var texture = replacement.GetFaceTexture(face);
                                if (Parent.Editor.Level.Settings.Textures.Contains(texture.Texture))
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

                    Parent.Editor.RoomGeometryChange(UndoRoom);
                    Parent.Editor.RoomSectorPropertiesChange(UndoRoom);
                    var relevantRooms = room.Portals.Select(p => p.AdjoiningRoom);
                    Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());

                    UndoRoom.BuildGeometry();
                    foreach (Room relevantRoom in relevantRooms)
                    {
                        Parent.Editor.RoomGeometryChange(relevantRoom);
                        Parent.Editor.RoomSectorPropertiesChange(relevantRoom);
                    }
                }
                else
                    BrokenWarning();
            };

            RedoInstance =()=> new GeometryUndoInstance(Parent, UndoRoom);
        }
    }

    public class UndoManager
    {
        private class UndoRedoStack
        {
            private UndoRedoInstance[] items;
            private int top = -1;

            public bool Empty => top == -1;
            public int Count => items.Length;

            public UndoRedoStack(int capacity)
            {
                items = new UndoRedoInstance[capacity];
            }

            public void Push(UndoRedoInstance item)
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

            public UndoRedoInstance Pop()
            {
                if (top == -1) return null;
                return items[top--];
            }

            public void Clear()
            {
                top = -1;
                for (int i = 0; i < items.Length; i++)
                    items[i] = null;
            }
        }

        public Editor Editor;
        private UndoRedoStack _undoStack;
        private UndoRedoStack _redoStack;

        public UndoManager(Editor editor, int undoDepth)
        {
            Editor = editor;
            _undoStack = new UndoRedoStack(Math.Abs(undoDepth));
            _redoStack = new UndoRedoStack(Math.Abs(undoDepth));
        }

        public void ClearAll()
        {
            Clear(_undoStack, true);
            Clear(_redoStack, true);
            Editor.UndoStackChanged();
        }

        public void PushGeometryChanged(Room room, SectorSelection? selection = null) => Push(new GeometryUndoInstance(this, room));
        public void PushObjectCreated(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, true));
        public void PushObjectDeleted(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, false));
        public void PushObjectTransformed(PositionBasedObjectInstance obj) => Push(new TransformObjectUndoInstance(this, obj));

        public void Redo() => Engage(_redoStack);
        public void Undo() => Engage(_undoStack);
        public void UndoClear() => Clear(_undoStack);
        public void RedoClear() => Clear(_redoStack);
        public bool UndoPossible => StackValid(_undoStack) && !_undoStack.Empty;
        public bool RedoPossible => StackValid(_redoStack) && !_redoStack.Empty;

        private bool StackValid(UndoRedoStack stack) => (stack != null && stack.Count > 0);
        
        private void Push(UndoRedoInstance instance)
        {
            if (!StackValid(_undoStack)) return;

            _undoStack.Push(instance);
            _redoStack.Clear();
            Editor.UndoStackChanged();
        }

        private void Clear(UndoRedoStack stack, bool silent = false)
        {
            if (!StackValid(stack)) return;
            stack.Clear();
            if (!silent) Editor.UndoStackChanged();
        }

        private void Engage(UndoRedoStack stack)
        {
            if (!StackValid(stack)) return;

            var instance = stack.Pop();
            if (instance == null) return;

            // Generate and push redo instance
            var redo = instance.RedoInstance();
            if (redo != null) (stack == _undoStack ? _redoStack : _undoStack).Push(redo);

            // Invoke original undo instance
            instance.UndoAction?.Invoke();
            Editor.UndoStackChanged();
        }
    }
}
