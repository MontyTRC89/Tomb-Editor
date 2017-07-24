using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor
{
    public enum EditorActionType
    {
        EditFace
    }

    public struct UndoRedoAction
    {
        public EditorActionType ActionType;
        public List<object> Parameters;
    }

    public class UndoRedoSystem
    { }
}
