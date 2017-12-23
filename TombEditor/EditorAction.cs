using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;

namespace TombEditor
{
    public interface IEditorAction
    { }

    public interface IEditorActionDisableOnLostFocus : IEditorAction
    { }

    public interface IEditorActionPlace : IEditorAction
    {
        PositionBasedObjectInstance CreateInstance(Level level, Room room);
        bool ShouldBeActive { get; }
    }

    public class EditorActionPlace : IEditorActionPlace
    {
        public bool Repeat { get; }
        private Func<Level, Room, PositionBasedObjectInstance> _createObjectInstance;

        public EditorActionPlace(bool repeat, Func<Level, Room, PositionBasedObjectInstance> createObjectInstance)
        {
            Repeat = repeat;
            _createObjectInstance = createObjectInstance;
        }

        public PositionBasedObjectInstance CreateInstance(Level level, Room room) => _createObjectInstance(level, room);
        public bool ShouldBeActive => Repeat;
    }

    public class EditorActionRelocateCamera : IEditorAction, IEditorActionDisableOnLostFocus
    { }
}
