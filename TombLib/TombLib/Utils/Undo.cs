using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Utils
{
    // Basic class for any undo type.
    // Any new undo type must be based on this base class!

    // According to command-based pattern, each undo instance has undo and complementary 
    // redo instance. In simple cases, redo command simply does back-to-back push-pop 
    // operation on both undo and redo stacks. In this situation UndoAction's UndoInstance
    // must mirror RedoInstance (see MoveRoomsUndoInstance as example).
    // In complicated cases, such as removing previously created object, UndoAction will
    // differ from RedoAction (in case of objects, we need to re-link room to newly created
    // instance), hence RedoInstance delegate has more complicated code.
    // In case redo action is impossible, just don't define RedoInstance.

    public abstract class UndoRedoInstance
    {
        public Action UndoAction { get; set; }
        public Func<UndoRedoInstance> RedoInstance { get; set; }
        public Func<bool> Valid { get; set; }
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
        
        public class UndoManagerMessageEventArgs : EventArgs
        {
            public string Message { get; set; }
        }
        
        public event EventHandler UndoStackChanged;
        public event EventHandler<UndoManagerMessageEventArgs> MessageSent;

        private const int MaxUndoDepth = 1000;

        //public Editor Editor { get; private set; }
        private UndoRedoStack _undoStack;
        private UndoRedoStack _redoStack;

        public UndoManager(int undoDepth)
        {
            undoDepth = MathC.Clamp(undoDepth, 1, MaxUndoDepth);
            
            _undoStack = new UndoRedoStack(undoDepth);
            _redoStack = new UndoRedoStack(undoDepth);
        }

        public void Push(List<UndoRedoInstance> instance)
        {
            if (!StackValid(_undoStack)) return;

            _undoStack.Push(instance);
            _redoStack.Clear();
            UndoStackChanged?.Invoke(this, new EventArgs());
        }
        public void Push(UndoRedoInstance instance) => Push(new List<UndoRedoInstance> { instance });

        public void Resize(int newSize)
        {
            if (newSize == _undoStack.Count) return;
            newSize = MathC.Clamp(newSize, 1, MaxUndoDepth);
            _undoStack.Resize(newSize);
            _redoStack.Resize(newSize);
            UndoStackChanged?.Invoke(this, new EventArgs());
        }

        public void ClearAll()
        {
            Clear(_undoStack, true);
            Clear(_redoStack, true);
            UndoStackChanged?.Invoke(this, new EventArgs());
        }

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
            if (!silent) UndoStackChanged?.Invoke(this, new EventArgs());
        }

        private void Engage(UndoRedoStack stack)
        {
            if (!StackValid(stack)) return;
            var counterStack = stack == _undoStack ? _redoStack : _undoStack;

            var instance = stack.Pop();
            if (instance == null) return;

            if (instance.All(item => item.Valid == null || item.Valid()))
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
                MessageSent?.Invoke(this, new UndoManagerMessageEventArgs() { Message = "Level state changed. " + (counterStack == _redoStack ? "Undo" : "Redo") + " action ignored." });
                
            UndoStackChanged?.Invoke(this, new EventArgs());
        }
    }
}
