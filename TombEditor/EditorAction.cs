using System;
using TombLib.LevelData;

namespace TombEditor
{
    public interface IEditorAction
    { }

    public interface IEditorActionDisableOnLostFocus : IEditorAction
    { }

    public interface IEditorActionPlace : IEditorAction
    {
        ObjectInstance CreateInstance(Level level, Room room);
        bool ShouldBeActive { get; }
    }

    public class EditorActionPlace : IEditorActionPlace
    {
        public bool Repeat { get; }
        private readonly Func<Level, Room, ObjectInstance> _createObjectInstance;

        public EditorActionPlace(bool repeat, Func<Level, Room, ObjectInstance> createObjectInstance)
        {
            Repeat = repeat;
            _createObjectInstance = createObjectInstance;
        }

        public ObjectInstance CreateInstance(Level level, Room room) => _createObjectInstance(level, room);
        public bool ShouldBeActive => Repeat;
    }

    public class EditorActionRelocateCamera : IEditorActionDisableOnLostFocus
    { }
}
