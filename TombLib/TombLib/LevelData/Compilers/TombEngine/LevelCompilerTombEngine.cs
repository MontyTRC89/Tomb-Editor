﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine : LevelCompiler
    {
        private readonly Dictionary<Room, TombEngineRoom> _tempRooms = new Dictionary<Room, TombEngineRoom>(new ReferenceEqualityComparer<Room>());

        private readonly ScriptIdTable<IHasScriptID> _scriptingIdsTable;
        private readonly List<ushort> _floorData = new List<ushort>();
        private readonly List<TombEngineMesh> _meshes = new List<TombEngineMesh>();
        private readonly List<TombEngineAnimation> _animations = new List<TombEngineAnimation>();
        private readonly List<TombEngineStateChange> _stateChanges = new List<TombEngineStateChange>();
        private readonly List<TombEngineAnimDispatch> _animDispatches = new List<TombEngineAnimDispatch>();
        private readonly List<short> _animCommands = new List<short>();
        private readonly List<int> _meshTrees = new List<int>();
        private readonly List<TombEngineKeyFrame> _frames = new List<TombEngineKeyFrame>();
        private List<TombEngineMoveable> _moveables = new List<TombEngineMoveable>();
        private readonly List<TombEngineStaticMesh> _staticMeshes = new List<TombEngineStaticMesh>();

        private List<TombEngineSpriteTexture> _spriteTextures = new List<TombEngineSpriteTexture>();
        private List<tr_sprite_sequence> _spriteSequences = new List<tr_sprite_sequence>();
        private readonly List<TombEngineCamera> _cameras = new List<TombEngineCamera>();
        private readonly List<TombEngineSink> _sinks = new List<TombEngineSink>();
        private readonly List<tr4_flyby_camera> _flyByCameras = new List<tr4_flyby_camera>();
        private readonly List<TombEngineSoundSource> _soundSources = new List<TombEngineSoundSource>();
        private List<TombEngineBox> _boxes = new List<TombEngineBox>();
        private List<TombEngineOverlap> _overlaps = new List<TombEngineOverlap>();
        private List<TombEngineZoneGroup> _zones = new List<TombEngineZoneGroup>();

        private readonly List<TombEngineItem> _items = new List<TombEngineItem>();
        private List<TombEngineAiItem> _aiItems = new List<TombEngineAiItem>();

        private TombEngineTexInfoManager _textureInfoManager;

        // Temporary dictionaries for mapping editor IDs to level IDs
        private Dictionary<MoveableInstance, int> _moveablesTable;
        private Dictionary<CameraInstance, int> _cameraTable;
        private Dictionary<SinkInstance, int> _sinkTable;
        private Dictionary<MoveableInstance, int> _aiObjectsTable;
        private Dictionary<SoundSourceInstance, int> _soundSourcesTable;
        private Dictionary<FlybyCameraInstance, int> _flybyTable;
        private Dictionary<StaticInstance, int> _staticsTable;

        // Collected game limits
        private Dictionary<Limit, int> _limits;

        public LevelCompilerTombEngine(Level level, string dest, IProgressReporter progressReporter)
            : base(level, dest, progressReporter)
        {
            _scriptingIdsTable = level.GlobalScriptingIdsTable.Clone();

            _limits = new Dictionary<Limit, int>();
            foreach (Limit limit in Enum.GetValues(typeof(Limit)))
                _limits.Add(limit, TrCatalog.GetLimit(level.Settings.GameVersion, limit));
        }

        public override CompilerStatistics CompileLevel()
        {
            ReportProgress(0, "Tomb Engine Level Compiler");

            if (_level.Settings.Wads.All(wad => wad.Wad == null))
                throw new NotSupportedException("A wad must be loaded to compile the final level.");

            DetectTombEngineVersion(false);
            DetectTombEngineVersion(true);

            _textureInfoManager = new TombEngineTexInfoManager(_level, _progressReporter, _limits[Limit.TexPageSize]);

            // Prepare level data in parallel to the sounds
            ConvertWad2DataToTombEngine();
            BuildRooms();

            // Compile textures
            ReportProgress(30, "Packing textures");
            _textureInfoManager.LayOutAllData();

            ReportProgress(35, "   Number of TexInfos: " + _textureInfoManager.TexInfoCount);
            ReportProgress(35, "   Number of anim texture sequences: " + _textureInfoManager.AnimatedTextures.Count);
            
            GetAllReachableRooms();
            BuildPathFindingData();
            PrepareSoundSources();
            PrepareItems();
            BuildCamerasAndSinks();
            BuildFloorData();
            BuildSprites();

            PrepareRoomsBuckets();
            PrepareMeshBuckets();

            _progressReporter.ReportInfo("\nWriting level file...\n");

            WriteLevelTombEngine();
            CopyNodeScripts();
            
            // Needed to make decision about backup (delete or restore)
            _compiledSuccessfully = true;

            _progressReporter.ReportInfo("\nOutput file: " + _finalDest);

            // Return statistics
            return new CompilerStatistics
            {
                BoxCount = _boxes.Count,
                OverlapCount = _overlaps.Count,
                ObjectTextureCount = _textureInfoManager.TexInfoCount,
            };
        }

        private void PrepareSoundSources()
        {
            ReportProgress(40, "Building sound sources");

            _soundSourcesTable = new Dictionary<SoundSourceInstance, int>(new ReferenceEqualityComparer<SoundSourceInstance>());

            foreach (var room in _level.ExistingRooms)
                foreach (var obj in room.Objects.OfType<SoundSourceInstance>())
                    _soundSourcesTable.Add(obj, _soundSourcesTable.Count);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _soundSourcesTable.Keys)
            {
                if (instance.IsEmpty)
                    continue;

                WadSoundInfo soundInfo = _level.Settings.WadTryGetSoundInfo(instance.SoundId);
                if (soundInfo == null)
                {
                    _progressReporter.ReportWarn("Sound (" + instance.SoundNameToDisplay + ") for sound source in room '" + instance.Room + "' at '" + instance.Position + "' is missing.");
                    continue;
                }

                int flags = 0;
                if (instance.Room.AlternateGroup >= 0)
                    flags = instance.Room.AlternateGroup;

                switch (instance.PlayMode)
                {
                    case SoundSourcePlayMode.Automatic:
                        flags |= instance.Room.Alternated ? (instance.Room.IsAlternate ? (ushort)0x2000 : (ushort)0x4000) : (ushort)0x8000;
                        break;
                    case SoundSourcePlayMode.Always:
                        flags |= 0x8000;
                        break;
                    case SoundSourcePlayMode.OnlyInBaseRoom:
                        flags |= 0x4000;
                        break;
                    case SoundSourcePlayMode.OnlyInAlternateRoom:
                        flags |= 0x2000;
                        break;
                }

                Vector3 position = instance.Room.WorldPos + instance.Position;
                _soundSources.Add(new TombEngineSoundSource
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    SoundID = (ushort)instance.SoundId,
                    Flags = flags,
                    LuaName = instance.LuaName ?? string.Empty
                });
            }

            ReportProgress(41, "    Number of sound sources: " + _soundSources.Count);
        }

        private void BuildCamerasAndSinks()
        {
            ReportProgress(46, "Building cameras and sinks");

            {
                int sinkID = 0;
                int camID = 0;
                int flybyID = 0;

                _cameraTable = new Dictionary<CameraInstance, int>(new ReferenceEqualityComparer<CameraInstance>());
                _sinkTable = new Dictionary<SinkInstance, int>(new ReferenceEqualityComparer<SinkInstance>());
                _flybyTable = new Dictionary<FlybyCameraInstance, int>(new ReferenceEqualityComparer<FlybyCameraInstance>());

                foreach (var room in _level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<CameraInstance>())
                        _cameraTable.Add(obj, camID++);
                    foreach (var obj in room.Objects.OfType<FlybyCameraInstance>())
                        _flybyTable.Add(obj, flybyID++);
                    foreach (var obj in room.Objects.OfType<SinkInstance>())
                        _sinkTable.Add(obj, sinkID++);
                }
            }

            foreach (var instance in _cameraTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                _cameras.Add(new TombEngineCamera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = (short)_roomRemapping[instance.Room],
                    Flags = instance.CameraMode == CameraInstanceMode.Locked ? 1 : 0,
                    Speed = instance.MoveTimer,
                    LuaName = instance.LuaName ?? string.Empty
                });
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _sinkTable.Keys)
            {
                int xSector = (int)Math.Floor(instance.Position.X / Level.BlockSizeUnit);
                int zSector = (int)Math.Floor(instance.Position.Z / Level.BlockSizeUnit);

                var tempRoom = _tempRooms[instance.Room];
                Vector3 position = instance.Room.WorldPos + instance.Position;

                int boxIndex = tempRoom.Sectors[tempRoom.NumZSectors * xSector + zSector].BoxIndex;
                
                _sinks.Add(new TombEngineSink
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Strength = instance.Strength,
                    BoxIndex = boxIndex,
                    LuaName = instance.LuaName ?? string.Empty
                });
            }

            foreach (var instance in _flybyTable.Keys)
            {
                Vector3 direction = instance.GetDirection();
                Vector3 position = instance.Room.WorldPos + instance.Position;
                ushort rollTo65536 = (ushort)(65536 - Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Roll * (65536.0 / 360.0)))));
                _flyByCameras.Add(new tr4_flyby_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)Math.Round(-position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = _roomRemapping[instance.Room],
                    FOV = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Fov * (65536.0 / 360.0)))),
                    Roll = unchecked((short)rollTo65536),
                    Timer = (ushort)instance.Timer,
                    Speed = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Speed * 655.0f))),
                    Sequence = (byte)instance.Sequence,
                    Index = (byte)instance.Number,
                    Flags = instance.Flags,
                    DirectionX = (int) Math.Round(position.X + Level.BlockSizeUnit * direction.X),
                    DirectionY = (int)-Math.Round(position.Y + Level.BlockSizeUnit * direction.Y),
                    DirectionZ = (int) Math.Round(position.Z + Level.BlockSizeUnit * direction.Z),
                });
            }
            _flyByCameras.Sort(new tr4_flyby_camera.ComparerFlyBy());

            // Check camera duplicates
            int lastSeq   = -1;
            int lastIndex = -1;

            for (int i = 0; i < _flyByCameras.Count; i++)
            {
                if(_flyByCameras[i].Sequence != lastSeq)
                {
                    lastSeq = _flyByCameras[i].Sequence;
                    lastIndex = -1;
                }

                if (_flyByCameras[i].Index == lastIndex && _flyByCameras[i].Sequence == lastSeq)
                    _progressReporter.ReportWarn("Warning: flyby sequence " + _flyByCameras[i].Sequence + " has duplicated camera with ID " + lastIndex);
                lastIndex = _flyByCameras[i].Index;
            }

            ReportProgress(47, "    Number of cameras: " + _cameraTable.Count);
            ReportProgress(47, "    Number of flyby cameras: " + _flyByCameras.Count);
            ReportProgress(47, "    Number of sinks: " + _sinkTable.Count);
        }

        private static TombEngineRoomSector GetSector(TombEngineRoom room, int x, int z)
        {
            return room.Sectors[room.NumZSectors * x + z];
        }

        private static void SaveSector(TombEngineRoom room, int x, int z, TombEngineRoomSector sector)
        {
            room.Sectors[room.NumZSectors * x + z] = sector;
        }

        private void GetAllReachableRooms()
        {
            foreach (var room in _level.ExistingRooms)
                _tempRooms[room].ReachableRooms = new List<Room>();

            foreach (var room in _level.ExistingRooms)
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
            while (room.GetFloorRoomConnectionInfo(new VectorInt2(x, z)).TraversableType == Room.RoomConnectionType.FullPortal)
            {
                var sector = room.Blocks[x, z];
                x += room.Position.X - sector.FloorPortal.AdjoiningRoom.Position.X;
                z += room.Position.Z - sector.FloorPortal.AdjoiningRoom.Position.Z;
                room = sector.FloorPortal.AdjoiningRoom;
            }
        }

        private short GetMostDownFloor(Room room, int x, int z)
        {
            FindBottomFloor(ref room, ref x, ref z);
            return (short)(_tempRooms[room].AuxSectors[x, z].LowestFloor * (short)Level.HeightUnit);
        }

        private bool FindMonkeyFloor(Room room, int x, int z)
        {
            FindBottomFloor(ref room, ref x, ref z);
            return room.Blocks[x, z].HasFlag(BlockFlags.Monkey);
        }

        private void PrepareItems()
        {
            ReportProgress(42, "Building items table");

            _moveablesTable = new Dictionary<MoveableInstance, int>(new ReferenceEqualityComparer<MoveableInstance>());
            _aiObjectsTable = new Dictionary<MoveableInstance, int>(new ReferenceEqualityComparer<MoveableInstance>());

            foreach (Room room in _level.ExistingRooms)
                foreach (var instance in room.Objects.OfType<MoveableInstance>())
                {
                    WadMoveable wadMoveable = _level.Settings.WadTryGetMoveable(instance.WadObjectId);
                    if (wadMoveable == null)
                    {
                        _progressReporter.ReportWarn("Moveable '" + instance + "' was not included in the level because it is missing the *.wad file.");
                        continue;
                    }

                    Vector3 position = instance.Room.WorldPos + instance.Position;

                    // Split AI objects and normal objects
                    if (TrCatalog.IsMoveableAI(TRVersion.Game.TombEngine, wadMoveable.Id.TypeId))
                    {
                        // Box index data gets overwritten in runtime, but we write it for consistency
                        int xSector  = (int)Math.Floor(instance.Position.X / Level.BlockSizeUnit);
                        int zSector  = (int)Math.Floor(instance.Position.Z / Level.BlockSizeUnit);
                        var tempRoom = _tempRooms[instance.Room];
                        int boxIndex = tempRoom.Sectors[tempRoom.NumZSectors * xSector + zSector].BoxIndex;

                        _aiItems.Add(new TombEngineAiItem
                        {
                            X = (int)Math.Round(position.X),
                            Y = (int)-Math.Round(position.Y),
                            Z = (int)Math.Round(position.Z),
                            ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                            Room = (ushort)_roomRemapping[instance.Room],
                            Yaw = ToTrAngle(instance.RotationY),
                            Pitch = ToTrAngle(instance.RotationX),
                            Roll = ToTrAngle(-instance.Roll),
                            OCB = instance.Ocb,
                            BoxIndex = boxIndex,
                            Flags = (ushort)(instance.CodeBits << 1),
                            LuaName = instance.LuaName ?? string.Empty
                        });
                        _aiObjectsTable.Add(instance, _aiObjectsTable.Count);
                    }
                    else
                    {
                        int flags = (instance.CodeBits << 9) | (instance.ClearBody ? 0x80 : 0) | (instance.Invisible ? 0x100 : 0);

                        _items.Add(new TombEngineItem
                        {
                            X = (int)Math.Round(position.X),
                            Y = (int)-Math.Round(position.Y),
                            Z = (int)Math.Round(position.Z),
                            ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                            Room = (short)_roomRemapping[instance.Room],
                            Yaw = ToTrAngle(instance.RotationY),
                            Pitch = ToTrAngle(instance.RotationX),
                            Roll = ToTrAngle(-instance.Roll),
                            Color = new Vector4(instance.Color.X, instance.Color.Y, instance.Color.Z, 1.0f),
                            OCB = instance.Ocb,
                            Flags = unchecked((ushort)flags),
                            LuaName = instance.LuaName ?? string.Empty
                        });
                        _moveablesTable.Add(instance, _moveablesTable.Count);
                    }
                }

            // Sort AI objects and put all LARA_START_POS objects (last AI object by ID) in front
            _aiItems = _aiItems.OrderByDescending(item => item.ObjectID).ThenBy(item => item.OCB).ToList();

            ReportProgress(45, "    Number of items: " + _items.Count);
            ReportProgress(45, "    Number of AI objects: " + _aiItems.Count);
        }

        private void CopyNodeScripts()
        {
            var scriptDirectory = Path.Combine(_level.Settings.MakeAbsolute(_level.Settings.GameDirectory), 
                                               ScriptingUtils.GameNodeScriptPath);

            if (!Directory.Exists(ScriptingUtils.NodeScriptPath))
            {
                _progressReporter.ReportWarn("Node script catalog was not found. Node scripts were not copied to game folder.");
                return;
            }

            if (!Directory.Exists(scriptDirectory))
			{
				Directory.CreateDirectory(scriptDirectory);
			}
            else
            {
                foreach (var file in Directory.GetFiles(scriptDirectory))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }

            ReportProgress(99, "\nCopying node catalogs to level script folder...");

            Directory.GetFiles(ScriptingUtils.NodeScriptPath).Where(p => p.EndsWith(".lua")).ToList().ForEach(src =>
            {
                var dest = Path.Combine(scriptDirectory, Path.GetFileName(src));

                if (File.Exists(dest))
                    File.SetAttributes(dest, FileAttributes.Normal);

                File.Copy(src, dest, true);
            });
        }

        public string DetectTombEngineVersion(bool getTargetEditorVersion)
        {
            var defaultVersion = "1.0.0.0";
            var buffer = _level.Settings.MakeAbsolute(_level.Settings.GameExecutableFilePath);

            if (File.Exists(buffer))
            {
                var version = FileVersionInfo.GetVersionInfo(buffer);
                if (getTargetEditorVersion)
                {
                    if (string.IsNullOrEmpty(version.ProductVersion))
                    {
                        _progressReporter.ReportWarn("Tomb Engine target editor version is missing. Probably not a Tomb Engine executable?");
                        return defaultVersion;
                    }

                    buffer = version.ProductVersion.Replace(",", ".");
                    _progressReporter.ReportInfo("Target Tomb Editor version is " + buffer);

                    int currentVersion = 0;
                    int.TryParse(FileVersionInfo
                        .GetVersionInfo(Assembly.GetExecutingAssembly().Location)
                        .ProductVersion
                        .Replace(".", string.Empty)
                        .PadRight(4, '0'), out currentVersion);

                    int targetVersion = 0;
                    int.TryParse(buffer
                        .Replace(".", string.Empty)
                        .PadRight(4, '0'), out targetVersion);

                    if (targetVersion > currentVersion)
                    {
                        _progressReporter.ReportWarn("Tomb Engine target editor version is higher than this Tomb Editor version. " +
                            "Please update Tomb Editor.");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(version.FileVersion))
                    {
                        _progressReporter.ReportWarn("Tomb Engine version is missing. Probably not a Tomb Engine executable?");
                        return defaultVersion;
                    }

                    buffer = version.FileVersion.Replace(",", ".");
                    _progressReporter.ReportInfo("Tomb Engine version is " + buffer);
                }

            }
            else
            {
                if (!getTargetEditorVersion)
                {
                    _progressReporter.ReportWarn("Tomb Engine executable was not found in game directory.");
                }

                return defaultVersion;
            }

            return buffer;
        }
    }
}
