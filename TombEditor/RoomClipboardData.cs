using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.IO;
using TombLib.Utils;
using TombLib.LevelData.IO;

namespace TombEditor
{
    [Serializable]
    public class RoomClipboardData
    {
        private const int _magicWord = 0x1f6cf4f5;

        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public struct ContourLine
        {
            public Vector2 Start;
            public Vector2 End;
        }

        private byte[] _data;
        private List<ContourLine> _contourLines;
        private Vector2 _dropPosition;
        private string _levelPath;

        public RoomClipboardData(Editor editor, Vector2 dropPosition)
        {
            _dropPosition = dropPosition;

            // Collect contour lines
            _contourLines = new List<ContourLine>();
            foreach (Room room in editor.SelectedRooms)
                for (int z = 1; z < room.NumZSectors; ++z)
                    for (int x = 1; x < room.NumXSectors; ++x)
                    {
                        Block thisBlock = room.Blocks[x, z];
                        Block aboveBlock = room.Blocks[x, z - 1];
                        Block leftBlock = room.Blocks[x - 1, z];
                        if (aboveBlock.IsAnyWall != thisBlock.IsAnyWall)
                            _contourLines.Add(new ContourLine
                            {
                                Start = new Vector2(x + room.Position.X, z + room.Position.Z),
                                End = new Vector2(x + room.Position.X + 1, z + room.Position.Z)
                            });
                        if (leftBlock.IsAnyWall != thisBlock.IsAnyWall)
                            _contourLines.Add(new ContourLine
                            {
                                Start = new Vector2(x + room.Position.X, z + room.Position.Z),
                                End = new Vector2(x + room.Position.X, z + room.Position.Z + 1)
                            });
                    }

            // Write data
            _levelPath = editor.Level.Settings.LevelFilePath ?? "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(stream);
                Prj2Writer.SaveToPrj2(stream, editor.Level, new Prj2Writer.Filter
                {
                    RoomPredicate = (room) => editor.SelectedRoomsContains(room)
                });
                _data = stream.GetBuffer();
            }
        }

        public RoomClipboardData(Editor editor)
            : this(editor, editor.SelectedRooms.Aggregate(
                new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue),
                (area, room) => room.WorldArea.Union(area),
                (area) => area.GetMid()))
        { }

        public IReadOnlyList<ContourLine> ContourLines => _contourLines;

        public Vector2 DropPosition => _dropPosition;

        public Level CreateLevel()
        {
            using (var stream = new MemoryStream(_data, false))
            {
                Level level = Prj2Loader.LoadFromPrj2(_levelPath, stream, new ProgressReporterSimple(),
                    new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
                return level;
            }
        }

        public void MergeInto(Editor editor, Vector2 offset)
        {
            Level level = CreateLevel();
            List<Room> newRooms = level.Rooms.Where(room => room != null).ToList();
            foreach (Room room in newRooms)
                room.Position += new Vector3(offset.X, 0, offset.Y);
            LevelSettings newLevelSettings = editor.Level.Settings.Clone();
            editor.Level.MergeFrom(level, true, newSettings => editor.UpdateLevelSettings(newSettings));
            editor.RoomListChange();
            editor.SelectRoomsAndResetCamera(newRooms);
        }
    }
}