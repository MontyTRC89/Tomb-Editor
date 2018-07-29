using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombEditor
{
    public class EditorLevelSnapshot
    {
        private Snapshot _levelSnapshot;
        private ushort[] _selectedRooms;
        private SectorSelection _selectedSectors;
        private ObjectIndexed _selectedObjectRoomIndex;
        private ItemType? _choosenItem;
        private bool _hasUnsavedChanges;
        private WindowInfo _windowInfo; // Allows each window to save additional information (mainly camera position)

        public EditorLevelSnapshot(Editor editor)
        {
            _levelSnapshot = editor.SnapshotEngine.MakeSnapshot(editor.Level);
            _selectedRooms = new ushort[editor.SelectedRooms.Count];
            for (int i = 0; i < editor.SelectedRooms.Count; ++i)
                _selectedRooms[i] = (ushort)editor.Level.RoomIndexOf(editor.SelectedRooms[i]);
            _selectedSectors = editor.SelectedSectors;
            _selectedObjectRoomIndex = new ObjectIndexed(editor.Level, editor.SelectedObject);
            _choosenItem = editor.ChosenItem;
            _hasUnsavedChanges = editor.HasUnsavedChanges;
            WindowInfoGetEvent getWindowInfo = new WindowInfoGetEvent();
            editor.RaiseEvent(getWindowInfo);
            _windowInfo = getWindowInfo.Info;

        }

        public void Apply(Editor editor)
        {
            editor.Level = _levelSnapshot.MaterializeLevel();
            editor.SelectedRooms = _selectedRooms.Select(i => editor.Level.Rooms[i]).ToArray();
            editor.SelectedSectors = _selectedSectors;
            editor.SelectedObject = _selectedObjectRoomIndex.FindAgain(editor.Level);
            if (_choosenItem.HasValue) // Check that the chosen item is still available since it's an external resource.
                if (_choosenItem.Value.IsStatic)
                    editor.ChosenItem = editor.Level.Settings.WadTryGetStatic(_choosenItem.Value.StaticId) == null ? null : _choosenItem;
                else
                    editor.ChosenItem = editor.Level.Settings.WadTryGetMoveable(_choosenItem.Value.MoveableId) == null ? null : _choosenItem;
            editor.HasUnsavedChanges = _hasUnsavedChanges;
            editor.RaiseEvent(new WindowInfoSetEvent { Info = _windowInfo });
        }

        // Helper class to remember objects in levels not in memory
        private struct ObjectIndexed
        {
            private int roomIndexPlus1; // Initialited to 0.
            private int objectIndexPlus1;
            public ObjectIndexed(Level level, ObjectInstance objectToIndex)
            {
                roomIndexPlus1 = 0;
                objectIndexPlus1 = 0;

                if (objectToIndex?.Room == null) // Shouldn't happen but to be save.
                    return;

                roomIndexPlus1 = level.RoomIndexOf(objectToIndex.Room) + 1;
                foreach (ObjectInstance @object in objectToIndex.Room.AnyObjects)
                {
                    ++objectIndexPlus1;
                    if (objectToIndex == @object)
                        return;
                }
                objectIndexPlus1 = 0;
            }

            public bool HasValue => objectIndexPlus1 != 0;
            public ObjectInstance FindAgain(Level level)
            {
                if (!HasValue)
                    return null;
                if ((roomIndexPlus1 - 1) >= level.Rooms.Length)
                    return null;
                Room room = level.Rooms[roomIndexPlus1 - 1];
                if (room == null)
                    return null;
                return room.AnyObjects.Skip(objectIndexPlus1 - 1).FirstOrDefault();
            }
        }
    }
}
