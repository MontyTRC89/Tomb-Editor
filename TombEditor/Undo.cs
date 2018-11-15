using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;

namespace TombEditor
{
    public abstract class UndoInstance
    {
        public UndoManager Parent { get; set; }
        public Action UndoAction { get; set; }
        public Action RedoAction { get; set; }
    }

    public class RoomUndoInstance : UndoInstance
    {
        public int RoomIndex;
        public Room UndoRoom;

        public RoomUndoInstance(UndoManager parent, Room room)
        {
            Parent = parent;
            RoomIndex = Array.FindIndex(Parent.Editor.Level.Rooms, r => r == room);
            UndoRoom = room.Clone(Parent.Editor.Level, null, true);

            UndoAction = delegate ()
            {
                if (UndoRoom == null)
                    return;

                var roomIsCurrent = Parent.Editor.Level.Rooms[RoomIndex] == Parent.Editor.SelectedRoom;

                Parent.Editor.Level.Rooms[RoomIndex] = UndoRoom;

                if (roomIsCurrent)
                    Parent.Editor.SelectedRoom = Parent.Editor.Level.Rooms[RoomIndex];
                Parent.Editor.Level.Rooms[RoomIndex].BuildGeometry();
                Parent.Editor.RoomGeometryChange(Parent.Editor.Level.Rooms[RoomIndex]);
            };
        }
    }

    public class UndoManager
    {
        private class UndoRedoStack
        {
            private UndoInstance[] items;
            private int top = -1;

            public bool Empty => top == -1;

            public UndoRedoStack(int capacity)
            {
                items = new UndoInstance[capacity];
            }

            public void Push(UndoInstance item)
            {
                if (top == -1)
                    top = 0;

                items[top] = item;
                top = (top + 1) % items.Length;
            }
            public UndoInstance Pop()
            {
                if (top == -1)
                    return null;

                top = (items.Length + top - 1) % items.Length;
                if (top == 0)
                {
                    top = -1;
                    return items[0];
                }
                else
                    return items[top];
            }

            public void Clear()
            {
                top = -1;
                for (int i = 0; i < items.Length; i++)
                    items[i] = null;
            }
        }

        public Editor Editor;
        private UndoRedoStack _undoStack = new UndoRedoStack(10);
        private UndoRedoStack _redoStack = new UndoRedoStack(10);

        public UndoManager(Editor editor)
        {
            Editor = editor;
        }

        public void Push(UndoInstance instance)
        {
            _undoStack.Push(instance);
            _redoStack.Clear();
            Editor.UndoStackChanged();
        }

        public void Push(Room room) => Push(new RoomUndoInstance(this, room));
        public void Redo() => Engage(_redoStack);
        public void Undo() => Engage(_undoStack);
        public bool UndoPossible => !_undoStack.Empty;
        public bool RedoPossible => !_redoStack.Empty;

        public void ClearUndo()
        {
            _undoStack.Clear();
            Editor.UndoStackChanged();
        }

        public void ClearRedo()
        {
            _redoStack.Clear();
            Editor.UndoStackChanged();
        }

        public void ClearAll()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            Editor.UndoStackChanged();
        }

        private void Engage(UndoRedoStack stack)
        {
            var instance = stack.Pop();
            if (instance == null) return;

            if (instance is RoomUndoInstance)
            {
                UndoInstance back = new RoomUndoInstance(this, Editor.Level.Rooms[((RoomUndoInstance)instance).RoomIndex]);
                (stack == _undoStack ? _redoStack : _undoStack).Push(back);
            }

            instance.UndoAction.Invoke();
            Editor.UndoStackChanged();
        }
    }
}
