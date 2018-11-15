using System;
using TombLib;
using TombLib.LevelData;

namespace TombEditor
{
    // Basic class for any undo type.
    // Any new undo type must be based on this base class!
    
    // According to command-based pattern, each undo instance has undo and redo command.
    // In simple cases, redo command simply does back-to-back push-pop operation on both
    // undo and redo stacks. In this situation UndoAction must be equal to RedoAction
    // (see RoomUndoInstance as example).
    // In complicated cases, such as removing previously created object, UndoAction will
    // differ from RedoAction, hence 2 separate actions will be needed.

    public abstract class UndoRedoInstance
    {
        public UndoManager Parent { get; set; }
        public Action UndoAction { get; set; }
        public Func<UndoRedoInstance> RedoInstance { get; set; }
        protected UndoRedoInstance(UndoManager parent) { Parent = parent; }
    }

    public class ObjectUndoInstance : UndoRedoInstance
    {
        public PositionBasedObjectInstance UndoObject;
        public Room Room;
        public bool Created;

        public ObjectUndoInstance(UndoManager parent, PositionBasedObjectInstance obj, bool created) : base(parent)
        {
            Created = created;
            UndoObject = obj;
            Room = obj.Room;

            UndoAction = delegate ()
            {
                if (Created)
                    EditorActions.DeleteObjectWithoutUpdate(UndoObject);
                else
                {
                    var backupPos = obj.Position;
                    VectorInt2 pos = new VectorInt2((int)((obj.Position.X - 512.0f) / 1024.0f), (int)((obj.Position.Z - 512.0f) / 1024.0f));
                    EditorActions.PlaceObjectWithoutUpdate(Room, pos, UndoObject);
                    EditorActions.MoveObject(UndoObject, backupPos);
                }
            };

            RedoInstance = delegate ()
            {
                var result = new ObjectUndoInstance(Parent, UndoObject, !Created);
                if (result.Room == null) result.Room = Room;
                return result;
            };
        }
    }

    public class RoomUndoInstance : UndoRedoInstance
    {
        public int RoomIndex; // Needed to put resulting room in appropriate slot.
        public Room UndoRoom;

        public RoomUndoInstance(UndoManager parent, Room room) : base(parent)
        {
            RoomIndex = Array.FindIndex(Parent.Editor.Level.Rooms, r => r == room);
            UndoRoom = room.Clone(Parent.Editor.Level, null, true);

            UndoAction = delegate ()
            {
                if (UndoRoom == null) return;
                var roomIsCurrent = Parent.Editor.Level.Rooms[RoomIndex] == Parent.Editor.SelectedRoom;

                Room.Replace(ref Parent.Editor.Level.Rooms[RoomIndex], ref UndoRoom);

                if (roomIsCurrent)
                    Parent.Editor.SelectedRoom = Parent.Editor.Level.Rooms[RoomIndex];

                Parent.Editor.Level.Rooms[RoomIndex].BuildGeometry();
                Parent.Editor.RoomGeometryChange(Parent.Editor.Level.Rooms[RoomIndex]);
            };

            RedoInstance = delegate ()
            {
                return new RoomUndoInstance(Parent, Parent.Editor.Level.Rooms[RoomIndex]);
            };
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

        public void Push(UndoRedoInstance instance)
        {
            if (!StackValid(_undoStack)) return;

            _undoStack.Push(instance);
            _redoStack.Clear();
            Editor.UndoStackChanged();
        }

        public void ClearAll()
        {
            Clear(_undoStack, true);
            Clear(_redoStack, true);
            Editor.UndoStackChanged();
        }

        public void Push(Room room) => Push(new RoomUndoInstance(this, room));
        public void Push(PositionBasedObjectInstance obj, bool created = true) => Push(new ObjectUndoInstance(this, obj, created));
        public void Redo() => Engage(_redoStack);
        public void Undo() => Engage(_undoStack);
        public void UndoClear() => Clear(_undoStack);
        public void RedoClear() => Clear(_redoStack);
        public bool UndoPossible => StackValid(_undoStack) && !_undoStack.Empty;
        public bool RedoPossible => StackValid(_redoStack) && !_redoStack.Empty;

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
