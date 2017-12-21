using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.IO;
using System.IO;
using SharpDX;
using System.Threading.Tasks;
using System.Diagnostics;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerTr4 : LevelCompiler
    {
        public class CompilerStatistics
        {
            public int BoxCount { get; set; }
            public int OverlapCount { get; set; }
            public int ObjectTextureCount { get; set; }
            public override string ToString()
            {
                return "Boxes: " + BoxCount + " | Overlaps: " + OverlapCount + " | Applied 'object' textures: " + ObjectTextureCount;
            }
        };

        private readonly Dictionary<Room, tr_room> _tempRooms = new Dictionary<Room, tr_room>();

        private class ComparerFlyBy : IComparer<tr4_flyby_camera>
        {
            public int Compare(tr4_flyby_camera x, tr4_flyby_camera y)
            {
                if (x.Sequence != y.Sequence)
                    return x.Sequence > y.Sequence ? 1 : -1;

                if (x.Index == y.Index)
                    return 0;

                return x.Index > y.Index ? 1 : -1;
            }
        }

        private byte[] _texture32Data;
        private List<ushort> _floorData = new List<ushort>();
        private List<tr_mesh> _meshes = new List<tr_mesh>();
        private List<uint> _meshPointers = new List<uint>();
        private List<tr_animation> _animations = new List<tr_animation>();
        private List<tr_state_change> _stateChanges = new List<tr_state_change>();
        private List<tr_anim_dispatch> _animDispatches = new List<tr_anim_dispatch>();
        private List<ushort> _animCommands = new List<ushort>();
        private List<int> _meshTrees = new List<int>();
        private List<short> _frames = new List<short>();
        private List<tr_moveable> _moveables = new List<tr_moveable>();
        private List<tr_staticmesh> _staticMeshes = new List<tr_staticmesh>();

        private List<tr_sprite_texture> _spriteTextures = new List<tr_sprite_texture>();
        private List<tr_sprite_sequence> _spriteSequences = new List<tr_sprite_sequence>();
        private List<tr_camera> _cameras = new List<tr_camera>();
        private List<tr4_flyby_camera> _flyByCameras = new List<tr4_flyby_camera>();
        private List<tr_sound_source> _soundSources = new List<tr_sound_source>();
        private tr_box[] _boxes = new tr_box[0];
        private ushort[] _overlaps = new ushort[0];
        private tr_zone[] _zones = new tr_zone[0];

        private List<tr_item> _items = new List<tr_item>();
        private List<tr_ai_item> _aiItems = new List<tr_ai_item>();

        // texture data
        private Util.ObjectTextureManagerWithAnimations _objectTextureManager;

        // Temporary dictionaries for mapping editor IDs to level IDs
        private Dictionary<MoveableInstance, int> _moveablesTable;
        private Dictionary<CameraInstance, int> _cameraTable;
        private Dictionary<SinkInstance, int> _sinkTable;
        private Dictionary<MoveableInstance, int> _aiObjectsTable;
        private Dictionary<SoundSourceInstance, int> _soundSourcesTable;
        private Dictionary<FlybyCameraInstance, int> _flybyTable;
        private Dictionary<StaticInstance, int> _staticsTable;

        private byte[] _bufferSamples;

        public LevelCompilerTr4(Level level, string dest, IProgressReporter progressReporter)
            : base(level, dest, progressReporter)
        {
            _objectTextureManager = new Util.ObjectTextureManagerWithAnimations(level.Settings.AnimatedTextureSets);
        }

        private void PrepareLevelData()
        {
            ConvertWadMeshes(_level.Wad);
            ConvertWad2DataToTr4(_level.Wad);
            BuildRooms();
            PrepareItems();
            PrepareSoundSources();
            BuildCamerasAndSinks();
            GetAllReachableRooms();
            BuildPathFindingData();
            BuildFloorData();

            // Combine all texture data in the final texture map
            PrepareTextures();
        }

        private void PrepareSoundsData()
        {
            uint numSamples = 0;
            for (int i = 0; i < _level.Wad.SoundInfo.Count; i++)
            {
                var soundInfo = _level.Wad.SoundInfo.ElementAt(i).Value;
                numSamples += (uint)soundInfo.Samples.Count;
            }

            var stream = new MemoryStream();
            using (var writer = new BinaryWriterEx(stream))
            {
                writer.Write(numSamples);

                for (int i = 0; i < _level.Wad.SoundMapSize; i++)
                {
                    if (!_level.Wad.SoundInfo.ContainsKey((ushort)i)) continue;

                    var soundInfo = _level.Wad.SoundInfo[(ushort)i];

                    foreach (var sound in soundInfo.Samples)
                    {
                        writer.Write(sound.WaveData.GetLength(0));
                        writer.Write(sound.WaveData.GetLength(0));
                        writer.Write(sound.WaveData);
                    }
                }
            }

            _bufferSamples = stream.ToArray();
        }

        public CompilerStatistics CompileLevel()
        {
            ReportProgress(0, "Tomb Raider IV Level Compiler by MontyTRC");

            if (_level.Wad == null)
                throw new NotSupportedException("A wad must be loaded to compile to *.tr4.");

            // Prepare level data in parallel to the sounds
            using (Task task1 = Task.Factory.StartNew(PrepareLevelData))
                using (Task task2 = Task.Factory.StartNew(PrepareSoundsData))
                    Task.WaitAll(task1, task2);

            //Write the final level
            WriteLevelTr4();

            // Return statics
            return new CompilerStatistics
            {
                BoxCount = _boxes.Length,
                OverlapCount = _overlaps.Length,
                ObjectTextureCount = _objectTextureManager.ObjectTextureCount,
            };
        }

        private void PrepareSoundSources()
        {
            ReportProgress(40, "Building sound sources");

            _soundSourcesTable = new Dictionary<SoundSourceInstance, int>();

            foreach (var room in _level.Rooms.Where(room => room != null))
                foreach (var obj in room.Objects.OfType<SoundSourceInstance>())
                    _soundSourcesTable.Add(obj, _soundSourcesTable.Count);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _soundSourcesTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                _soundSources.Add(new tr_sound_source
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    SoundID = instance.SoundId,
                    Flags = 0x80
                });
            }

            ReportProgress(41, "    Number of sound sources: " + _soundSources.Count);
        }

        private void BuildCamerasAndSinks()
        {
            ReportProgress(42, "Building cameras and sinks");

            {
                int cameraSinkID = 0;
                int flybyID = 0;
                _cameraTable = new Dictionary<CameraInstance, int>();
                _sinkTable = new Dictionary<SinkInstance, int>();
                _flybyTable = new Dictionary<FlybyCameraInstance, int>();
                foreach (var room in _level.Rooms.Where(room => room != null))
                {
                    foreach (var obj in room.Objects.OfType<CameraInstance>())
                        _cameraTable.Add(obj, cameraSinkID++);
                    foreach (var obj in room.Objects.OfType<FlybyCameraInstance>())
                        _flybyTable.Add(obj, flybyID++);
                }
                foreach (var room in _level.Rooms.Where(room => room != null))
                {
                    foreach (var obj in room.Objects.OfType<SinkInstance>())
                        _sinkTable.Add(obj, cameraSinkID++);
                }
            }

            foreach (var instance in _cameraTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                _cameras.Add(new tr_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = (short)_roomsRemappingDictionary[instance.Room],
                    Flags = instance.Fixed ? (ushort)1 : (ushort)0
                });
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _sinkTable.Keys)
            {
                int xSector = (int)Math.Floor(instance.Position.X / 1024);
                int zSector = (int)Math.Floor(instance.Position.Z / 1024);

                var tempRoom = _tempRooms[instance.Room];
                Vector3 position = instance.Room.WorldPos + instance.Position;
                _cameras.Add(new tr_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = instance.Strength,
                    Flags = (ushort)((tempRoom.Sectors[tempRoom.NumZSectors * xSector + zSector].BoxIndex &
                                       0x7f00) >> 4)
                });
            }

            foreach (var instance in _flybyTable.Keys)
            {
                Vector3 direction = instance.GetDirection();
                Vector3 position = instance.Room.WorldPos + instance.Position;
                ushort rollTo65536 = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Roll * (65536.0 / 360.0))));
                _flyByCameras.Add(new tr4_flyby_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)Math.Round(-position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = _roomsRemappingDictionary[instance.Room],
                    FOV = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Fov * (65536.0 / 360.0)))),
                    Roll = unchecked((short)rollTo65536),
                    Timer = (ushort)instance.Timer,
                    Speed = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Speed * 655.0f))),
                    Sequence = (byte)instance.Sequence,
                    Index = (byte)instance.Number,
                    Flags = instance.Flags,
                    DirectionX = (int)Math.Round(position.X + 1024 * direction.X),
                    DirectionY = (int)-Math.Round(position.Y + 1024 * direction.Y),
                    DirectionZ = (int)Math.Round(position.Z + 1024 * direction.Z),
                });
            }
            _flyByCameras.Sort(new ComparerFlyBy());

            ReportProgress(45, "    Number of cameras: " + _cameraTable.Count);
            ReportProgress(45, "    Number of flyby cameras: " + _flyByCameras.Count);
            ReportProgress(45, "    Number of sinks: " + _sinkTable.Count);
        }

        private static tr_room_sector GetSector(tr_room room, int x, int z)
        {
            return room.Sectors[room.NumZSectors * x + z];
        }

        private static void SaveSector(tr_room room, int x, int z, tr_room_sector sector)
        {
            room.Sectors[room.NumZSectors * x + z] = sector;
        }

        private void GetAllReachableRooms()
        {
            foreach (var room in _level.Rooms.Where(r => r != null))
                _tempRooms[room].ReachableRooms = new List<Room>();

            foreach (var room in _level.Rooms.Where(r => r != null))
            {
                GetAllReachableRoomsUp(room, room);
                GetAllReachableRoomsDown(room, room);
            }
        }

        private void GetAllReachableRoomsUp(Room baseRoom, Room currentRoom)
        {
            // Wall portals
            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction == PortalDirection.Floor || p.Direction == PortalDirection.Ceiling)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (!tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
            }

            // Ceiling portals
            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction != PortalDirection.Ceiling)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    continue;

                tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
                GetAllReachableRoomsUp(baseRoom, p.AdjoiningRoom);
            }
        }

        private void GetAllReachableRoomsDown(Room baseRoom, Room currentRoom)
        {
            // portali laterali
            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction == PortalDirection.Floor || p.Direction == PortalDirection.Ceiling)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (!tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
            }

            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction != PortalDirection.Floor)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    continue;

                tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
                GetAllReachableRoomsDown(baseRoom, p.AdjoiningRoom);
            }
        }

        private void FindBottomFloor(ref Room room, ref int x, ref int z)
        {
            while (room.GetFloorRoomConnectionInfo(new DrawingPoint(x, z)).TraversableType == Room.RoomConnectionType.FullPortal)
            {
                var sector = room.Blocks[x, z];
                x += (int)(room.Position.X - sector.FloorPortal.AdjoiningRoom.Position.X);
                z += (int)(room.Position.Z - sector.FloorPortal.AdjoiningRoom.Position.Z);
                room = sector.FloorPortal.AdjoiningRoom;
            }
        }

        private short GetMostDownFloor(Room room, int x, int z)
        {
            FindBottomFloor(ref room, ref x, ref z);
            return (short)(_tempRooms[room].AuxSectors[x, z].LowestFloor * 256);
        }

        private bool FindMonkeyFloor(Room room, int x, int z)
        {
            FindBottomFloor(ref room, ref x, ref z);
            return room.Blocks[x, z].HasFlag(BlockFlags.Monkey);
        }

        private void PrepareItems()
        {
            ReportProgress(18, "Building items table");

            _moveablesTable = new Dictionary<MoveableInstance, int>();
            _aiObjectsTable = new Dictionary<MoveableInstance, int>();

            foreach (Room room in _level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects.OfType<MoveableInstance>())
                {
                    Vector3 position = instance.Room.WorldPos + instance.Position;
                    double angle = Math.Round(instance.RotationY * (65536.0 / 360.0));
                    ushort angleInt = unchecked((ushort)(Math.Max(0, Math.Min(ushort.MaxValue, angle))));

                    if (instance.WadObjectId >= 398 && instance.WadObjectId <= 406)
                    {
                        _aiItems.Add(new tr_ai_item
                        {
                            X = (int)Math.Round(position.X),
                            Y = (int)-Math.Round(position.Y),
                            Z = (int)Math.Round(position.Z),
                            ObjectID = (ushort)instance.WadObjectId,
                            Room = (ushort)_roomsRemappingDictionary[instance.Room],
                            Angle = angleInt,
                            OCB = instance.Ocb,
                            Flags = (ushort)(instance.CodeBits << 1)
                        });
                        _aiObjectsTable.Add(instance, _aiObjectsTable.Count);
                    }
                    else
                    {
                        int flags = (instance.CodeBits << 9) | (instance.ClearBody ? 0x80 : 0) | (instance.Invisible ? 0x100 : 0);

                        _items.Add(new tr_item
                        {
                            X = (int)Math.Round(position.X),
                            Y = (int)-Math.Round(position.Y),
                            Z = (int)Math.Round(position.Z),
                            ObjectID = (short)instance.WadObjectId,
                            Room = (short)_roomsRemappingDictionary[instance.Room],
                            Angle = angleInt,
                            Intensity1 = -1,
                            Ocb = instance.Ocb,
                            Flags = unchecked((ushort)flags)
                        });
                        _moveablesTable.Add(instance, _moveablesTable.Count);
                    }
                }
            ReportProgress(30, "    Number of items: " + _items.Count);
            ReportProgress(30, "    Number of AI objects: " + _aiItems.Count);
        }
    }
}
