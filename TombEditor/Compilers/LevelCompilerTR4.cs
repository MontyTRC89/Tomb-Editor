using System;
using System.Collections.Generic;
using System.Linq;
using TombEditor.Geometry;
using TombLib.IO;
using System.IO;
using SharpDX;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4 : LevelCompiler
    {
        private readonly Dictionary<Room, TrRoom> _tempRooms = new Dictionary<Room, TrRoom>();

        private class ComparerFlyBy : IComparer<Tr4FlybyCamera>
        {
            public int Compare(Tr4FlybyCamera x, Tr4FlybyCamera y)
            {
                if (x.Sequence != y.Sequence)
                    return x.Sequence > y.Sequence ? 1 : -1;

                if (x.Index == y.Index)
                    return 0;

                return x.Index > y.Index ? 1 : -1;
            }
        }

        private ushort _numRoomTextureTiles;
        private ushort _numObjectTextureTiles;
        private const ushort NumBumpTextureTiles = 0;

        private uint _texture32UncompressedSize;
        private uint _texture32CompressedSize;
        private byte[] _texture32;

        private uint _texture16UncompressedSize;
        private uint _texture16CompressedSize;
        private byte[] _texture16;

        private uint _miscTextureUncompressedSize;
        private uint _miscTextureCompressedSize;
        private byte[] _miscTexture;

        private uint _levelUncompressedSize;
        private uint _levelCompressedSize;

        private ushort[] _floorData;

        private TrMesh[] _meshes;

        private uint[] _meshPointers;
        private TrAnimation[] _animations;
        private TrStateChange[] _stateChanges;
        private TrAnimDispatch[] _animDispatches;
        private short[] _animCommands;
        private int[] _meshTrees;
        private short[] _frames;
        private TrMoveable[] _moveables;
        private TrStaticmesh[] _staticMeshes;

        private TrSpriteTexture[] _spriteTextures;
        private TrSpriteSequence[] _spriteSequences;
        private TrCamera[] _cameras;
        private Tr4FlybyCamera[] _flyByCameras;
        private TrSoundSource[] _soundSources;
        private TrBox[] _boxes;
        private ushort[] _overlaps;
        private TrZone[] _zones;
        private readonly TrAnimatedTexturesSet[] _animatedTextures = new TrAnimatedTexturesSet[0];

        private TrItem[] _items;
        private TrAiItem[] _aiItems;

        private uint _numSoundDetails;

        // texture data
        private readonly Util.ObjectTextureManager _objectTextureManager = new Util.ObjectTextureManager();

        // Temporary dictionaries for mapping editor IDs to level IDs
        private Dictionary<MoveableInstance, int> _moveablesTable;

        private Dictionary<CameraInstance, int> _cameraTable;
        private Dictionary<SinkInstance, int> _sinkTable;
        private Dictionary<MoveableInstance, int> _aiObjectsTable;
        private Dictionary<SoundSourceInstance, int> _soundSourcesTable;
        private Dictionary<FlybyCameraInstance, int> _flybyTable;

        private byte[] _bufferSamples;

        public LevelCompilerTr4(Level level, IProgressReporter progressReporter)
            : base(level, progressReporter)
        {
        }

        private void CompileLevelTask1()
        {
            ConvertWadMeshes();
            BuildRooms();
            PrepareItems();
            PrepareSounds();
            BuildCamerasAndSinks();
            GetAllReachableRooms();
            BuildPathFindingData();
            BuildFloorData();
            CopyWadData();
        }

        private void CompileLevelTask2()
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriterEx(stream))
                foreach (var sound in Level.Wad.OriginalWad.Sounds)
                {
                    byte[] soundData = Level.Settings.ReadSound(sound, Level.Settings.IgnoreMissingSounds);
                    writer.Write(soundData.GetLength(0));
                    writer.Write(soundData.GetLength(0));
                    writer.Write(soundData);
                }

            _bufferSamples = stream.ToArray();
        }

        public void CompileLevel()
        {
            var watch = new Stopwatch();
            watch.Start();

            ReportProgress(0, "Tomb Raider IV Level Compiler by MontyTRC");

            if (Level.Wad == null)
                throw new NotSupportedException("A wad must be loaded to compile to *.tr4.");

            PrepareFontAndSkyTexture();

            // Build level data in multiple threads
            using (Task task1 = Task.Factory.StartNew(CompileLevelTask1))
                using (Task task2 = Task.Factory.StartNew(CompileLevelTask2))
                    Task.WaitAll(task1, task2);

            // Now combine all texture data in the final texture map
            PrepareTextures();

            //Write the final level
            WriteLevelTr4();

            watch.Stop();
            long mills = watch.ElapsedMilliseconds;

            ReportProgress(100, "Elapsed time: " + (mills / 1000.0f));

            // Force garbage collector to compact memory
            GC.Collect();
        }

        private void PrepareSounds()
        {
            ReportProgress(40, "Building sound sources");

            _soundSourcesTable = new Dictionary<SoundSourceInstance, int>();

            foreach (var room in Level.Rooms.Where(room => room != null))
                foreach (var obj in room.Objects.OfType<SoundSourceInstance>())
                    _soundSourcesTable.Add(obj, _soundSourcesTable.Count);

            var tempSoundSources = new List<TrSoundSource>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _soundSourcesTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var source = new TrSoundSource
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    SoundID = (ushort)instance.SoundId,
                    Flags = 0x80
                };

                tempSoundSources.Add(source);
            }

            _soundSources = tempSoundSources.ToArray();

            ReportProgress(41, "    Number of sound sources: " + _soundSources.Length);
        }

        private void BuildCamerasAndSinks()
        {
            ReportProgress(42, "Building cameras and sinks");

            {
                int cameraSinkId = 0;
                int flybyId = 0;
                _cameraTable = new Dictionary<CameraInstance, int>();
                _sinkTable = new Dictionary<SinkInstance, int>();
                _flybyTable = new Dictionary<FlybyCameraInstance, int>();
                foreach (var room in Level.Rooms.Where(room => room != null))
                {
                    foreach (var obj in room.Objects.OfType<CameraInstance>())
                        _cameraTable.Add(obj, cameraSinkId++);
                    foreach (var obj in room.Objects.OfType<SinkInstance>())
                        _sinkTable.Add(obj, cameraSinkId++);
                    foreach (var obj in room.Objects.OfType<FlybyCameraInstance>())
                        _flybyTable.Add(obj, flybyId++);
                }
            }

            var tempCameras = new List<TrCamera>();

            foreach (var instance in _cameraTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var camera = new TrCamera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = (short)_roomsRemappingDictionary[instance.Room]
                };

                if (instance.Fixed)
                    camera.Flags = 0x01;

                tempCameras.Add(camera);
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _sinkTable.Keys)
            {
                int xSector = (int)Math.Floor(instance.Position.X / 1024);
                int zSector = (int)Math.Floor(instance.Position.Z / 1024);

                var tempRoom = _tempRooms[instance.Room];
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var camera = new TrCamera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = instance.Strength,
                    Flags = (ushort)((tempRoom.Sectors[tempRoom.NumZSectors * xSector + zSector].BoxIndex &
                                      0x7f00) >> 4)
                };

                tempCameras.Add(camera);
            }

            _cameras = tempCameras.ToArray();

            var tempFlyby = new List<Tr4FlybyCamera>();

            foreach (var instance in _flybyTable.Keys)
            {
                Vector3 direction = instance.GetDirection();
                Vector3 position = instance.Room.WorldPos + instance.Position;
                ushort rollTo65536 =
                    (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Roll * (65536.0 / 360.0))));
                var flyby = new Tr4FlybyCamera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)Math.Round(-position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = _roomsRemappingDictionary[instance.Room],
                    FOV = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Fov * (65536.0 / 360.0)))),
                    Roll = unchecked((short)rollTo65536),
                    Timer = instance.Timer,
                    Speed = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Speed * 655.0f))),
                    Sequence = instance.Sequence,
                    Index = instance.Number,
                    Flags = instance.Flags,
                    DirectionX = (int)Math.Round(position.X + 1024 * direction.X),
                    DirectionY = (int)-Math.Round(position.Y + 1024 * direction.Y),
                    DirectionZ = (int)Math.Round(position.Z + 1024 * direction.Z),
                };
                tempFlyby.Add(flyby);
            }

            tempFlyby.Sort(new ComparerFlyBy());

            _flyByCameras = tempFlyby.ToArray();

            ReportProgress(45, "    Number of cameras: " + _cameraTable.Count);
            ReportProgress(45, "    Number of flyby cameras: " + tempFlyby.Count);
            ReportProgress(45, "    Number of sinks: " + _sinkTable.Count);
        }

        private void CopyWadData()
        {
            ReportProgress(11, "Converting WAD data to TR4 format");

            var wad = Level.Wad.OriginalWad;

            ReportProgress(12, "    Number of animations: " + wad.Animations.Count);

            _animations = new TrAnimation[wad.Animations.Count];
            for (int i = 0; i < _animations.Length; i++)
            {
                _animations[i] = new TrAnimation
                {
                    AnimCommand = wad.Animations[i].CommandOffset,
                    FrameEnd = wad.Animations[i].FrameEnd,
                    FrameStart = wad.Animations[i].FrameStart,
                    FrameOffset = wad.Animations[i].KeyFrameOffset,
                    FrameSize = wad.Animations[i].KeyFrameSize,
                    FrameRate = wad.Animations[i].FrameDuration,
                    NextAnimation = wad.Animations[i].NextAnimation,
                    NextFrame = wad.Animations[i].NextFrame,
                    NumAnimCommands = wad.Animations[i].NumCommands,
                    NumStateChanges = wad.Animations[i].NumStateChanges,
                    StateChangeOffset = wad.Animations[i].ChangesIndex,
                    StateID = wad.Animations[i].StateId,
                    Speed = wad.Animations[i].Speed,
                    Accel = wad.Animations[i].Accel,
                    SpeedLateral = wad.Animations[i].SpeedLateral,
                    AccelLateral = wad.Animations[i].AccelLateral
                };
            }

            ReportProgress(13, "    Number of state changes: " + wad.Changes.Count);

            _stateChanges = new TrStateChange[wad.Changes.Count];
            for (int i = 0; i < _stateChanges.Length; i++)
            {
                _stateChanges[i] = new TrStateChange
                {
                    AnimDispatch = wad.Changes[i].DispatchesIndex,
                    NumAnimDispatches = wad.Changes[i].NumDispatches,
                    StateID = wad.Changes[i].StateId
                };
            }

            ReportProgress(14, "    Number of animation dispatches: " + wad.Dispatches.Count);

            _animDispatches = new TrAnimDispatch[wad.Dispatches.Count];
            for (int i = 0; i < _animDispatches.Length; i++)
            {
                _animDispatches[i] = new TrAnimDispatch
                {
                    High = wad.Dispatches[i].High,
                    Low = wad.Dispatches[i].Low,
                    NextAnimation = wad.Dispatches[i].NextAnimation,
                    NextFrame = wad.Dispatches[i].NextFrame
                };
            }

            ReportProgress(15, "    Number of animation commands: " + wad.Commands.Count);

            _animCommands = new short[wad.Commands.Count];
            for (int i = 0; i < _animCommands.Length; i++)
            {
                _animCommands[i] = wad.Commands[i];
            }

            _meshTrees = new int[wad.Links.Count];
            for (int i = 0; i < _meshTrees.Length; i++)
            {
                _meshTrees[i] = wad.Links[i];
            }

            ReportProgress(16, "    Number of keyframes: " + wad.KeyFrames.Count);

            _frames = new short[wad.KeyFrames.Count];
            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = wad.KeyFrames[i];
            }

            _meshPointers = new uint[wad.Pointers.Count];
            for (int i = 0; i < _meshPointers.Length; i++)
            {
                _meshPointers[i] = wad.Pointers[i];
            }

            ReportProgress(17, "    Number of moveables: " + wad.Moveables.Count);

            _moveables = new TrMoveable[wad.Moveables.Count];
            for (int i = 0; i < _moveables.Length; i++)
            {
                _moveables[i] = new TrMoveable
                {
                    Animation = (ushort)wad.Moveables[i].AnimationIndex,
                    FrameOffset = wad.Moveables[i].KeyFrameOffset,
                    MeshTree = wad.Moveables[i].LinksIndex,
                    NumMeshes = wad.Moveables[i].NumPointers,
                    ObjectID = wad.Moveables[i].ObjectID,
                    StartingMesh = wad.Moveables[i].PointerIndex
                };
            }

            ReportProgress(18, "    Number of static meshes: " + wad.StaticMeshes.Count);

            _staticMeshes = new TrStaticmesh[wad.StaticMeshes.Count];
            for (int i = 0; i < _staticMeshes.Length; i++)
            {
                _staticMeshes[i] = new TrStaticmesh
                {
                    CollisionBox = new TrBoundingBox
                    {
                        X1 = wad.StaticMeshes[i].CollisionX1,
                        Y1 = wad.StaticMeshes[i].CollisionY1,
                        Z1 = wad.StaticMeshes[i].CollisionZ1,
                        X2 = wad.StaticMeshes[i].CollisionX2,
                        Y2 = wad.StaticMeshes[i].CollisionY2,
                        Z2 = wad.StaticMeshes[i].CollisionZ2
                    },
                    VisibilityBox = new TrBoundingBox
                    {
                        X1 = wad.StaticMeshes[i].VisibilityX1,
                        Y1 = wad.StaticMeshes[i].VisibilityY1,
                        Z1 = wad.StaticMeshes[i].VisibilityZ1,
                        X2 = wad.StaticMeshes[i].VisibilityX2,
                        Y2 = wad.StaticMeshes[i].VisibilityY2,
                        Z2 = wad.StaticMeshes[i].VisibilityZ2
                    },
                    Flags = wad.StaticMeshes[i].Flags,
                    Mesh = wad.StaticMeshes[i].PointersIndex,
                    ObjectID = wad.StaticMeshes[i].ObjectId
                };
            }
        }

        private static TrRoomSector GetSector(TrRoom room, int x, int z)
        {
            return room.Sectors[room.NumZSectors * x + z];
        }

        private static void SaveSector(TrRoom room, int x, int z, TrRoomSector sector)
        {
            room.Sectors[room.NumZSectors * x + z] = sector;
        }

        private void GetAllReachableRooms()
        {
            foreach (var room in Level.Rooms.Where(r => r != null))
                _tempRooms[room].ReachableRooms = new List<Room>();

            foreach (var room in Level.Rooms.Where(r => r != null))
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

        private short GetMostDownFloor(Room room, int x, int z)
        {
            do
            {
                var sector = room.Blocks[x, z];
                if (room.IsFloorSolid(new DrawingPoint(x, z)))
                    return (short)(_tempRooms[room].AuxSectors[x, z].LowestFloor * 256);

                x += (int)(room.Position.X - sector.FloorPortal.AdjoiningRoom.Position.X);
                z += (int)(room.Position.Z - sector.FloorPortal.AdjoiningRoom.Position.Z);
                room = sector.FloorPortal.AdjoiningRoom;
            } while (true);
        }

        private bool FindMonkeyFloor(Room room, int x, int z)
        {
            do
            {
                var sector = room.Blocks[x, z];
                if (room.IsFloorSolid(new DrawingPoint(x, z)))
                    return sector.Flags.HasFlag(BlockFlags.Monkey);

                x += (int)(room.Position.X - sector.FloorPortal.AdjoiningRoom.Position.X);
                z += (int)(room.Position.Z - sector.FloorPortal.AdjoiningRoom.Position.Z);
                room = sector.FloorPortal.AdjoiningRoom;
            } while (true);
        }

        private void ConvertWadMeshes()
        {
            ReportProgress(11, "Converting WAD meshes to TR4 format");

            var wad = Level.Wad.OriginalWad;

            ReportProgress(12, "    Number of meshes: " + wad.Meshes.Count);

            var tempMeshes = new List<TrMesh>();

            foreach (var oldMesh in wad.Meshes)
            {
                var newMesh = new TrMesh
                {
                    Center = new TrVertex
                    {
                        X = oldMesh.SphereX,
                        Y = oldMesh.SphereY,
                        Z = oldMesh.SphereZ
                    },
                    Radius = oldMesh.Radius,
                    NumNormals = oldMesh.NumNormals
                };

                if (newMesh.NumNormals > 0)
                {
                    newMesh.Normals = new TrVertex[newMesh.NumNormals];
                    for (int k = 0; k < newMesh.NumNormals; k++)
                    {
                        newMesh.Normals[k] = new TrVertex
                        {
                            X = oldMesh.Normals[k].X,
                            Y = oldMesh.Normals[k].Y,
                            Z = oldMesh.Normals[k].Z
                        };
                    }
                }
                else
                {
                    newMesh.Lights = new short[-newMesh.NumNormals];
                    for (int k = 0; k < -newMesh.NumNormals; k++)
                    {
                        newMesh.Lights[k] = oldMesh.Shades[k];
                    }
                }

                newMesh.NumVertices = (short)oldMesh.NumVertices;
                newMesh.Vertices = new TrVertex[newMesh.NumVertices];

                for (int j = 0; j < newMesh.NumVertices; j++)
                {
                    newMesh.Vertices[j] = new TrVertex
                    {
                        X = oldMesh.Vertices[j].X,
                        Y = oldMesh.Vertices[j].Y,
                        Z = oldMesh.Vertices[j].Z
                    };
                }

                var tempQuads = new List<TrFace4>();
                var tempTriangles = new List<TrFace3>();

                for (int j = 0; j < oldMesh.NumPolygons; j++)
                {
                    var poly = oldMesh.Polygons[j];

                    ushort lightingEffect = 0;
                    if ((poly.Attributes & 0x02) == 0x02)
                    {
                        // Shine effect
                        short shine = (short)((poly.Attributes & 0x7c) >> 2);
                        lightingEffect |= (ushort)(shine << 1);
                    }

                    if ((poly.Attributes & 0x01) == 0x01)
                    {
                        // Alpha trasparency
                        lightingEffect |= 0x01;
                    }

                    if (poly.Shape == 9)
                    {
                        newMesh.NumTexturedQuads++;

                        var result =
                            _objectTextureManager.AddTexture(
                                Level.Wad.GetTextureArea(poly.Texture, false, poly.Attributes), false, false);
                        tempQuads.Add(result.CreateFace4(poly.V1, poly.V2, poly.V3, poly.V4, lightingEffect));
                    }
                    else
                    {
                        newMesh.NumTexturedTriangles++;

                        var result =
                            _objectTextureManager.AddTexture(
                                Level.Wad.GetTextureArea(poly.Texture, true, poly.Attributes), true, false);
                        tempTriangles.Add(result.CreateFace3(poly.V1, poly.V2, poly.V3, lightingEffect));
                    }
                }

                newMesh.TexturedQuads = tempQuads.ToArray();
                newMesh.TexturedTriangles = tempTriangles.ToArray();

                tempMeshes.Add(newMesh);
            }

            _meshes = tempMeshes.ToArray();
        }

        private void PrepareItems()
        {
            ReportProgress(18, "Building items table");

            _moveablesTable = new Dictionary<MoveableInstance, int>();
            _aiObjectsTable = new Dictionary<MoveableInstance, int>();

            foreach (Room room in Level.Rooms.Where(room => room != null))
                foreach (var obj in room.Objects.OfType<MoveableInstance>())
                    if (obj.WadObjectId >= 398 && obj.WadObjectId <= 406)
                        _aiObjectsTable.Add(obj, _aiObjectsTable.Count);
                    else
                        _moveablesTable.Add(obj, _moveablesTable.Count);

            var tempItems = new List<TrItem>();
            var tempAiObjects = new List<TrAiItem>();

            foreach (var instance in _moveablesTable.Keys)
            {
                double angle = Math.Round(instance.RotationY * (65536.0 / 360.0));
                Vector3 position = instance.Room.WorldPos + instance.Position;

                var item = new TrItem
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    ObjectID = (short)instance.WadObjectId,
                    Room = (short)_roomsRemappingDictionary[instance.Room],
                    Angle = unchecked((short)((ushort)(Math.Max(0, Math.Min(ushort.MaxValue, angle))))),
                    Intensity1 = -1,
                    Ocb = instance.Ocb
                };

                if (instance.ClearBody)
                    item.Flags |= 0x80;
                if (instance.Invisible)
                    item.Flags |= 0x100;

                item.Flags |= (ushort)(instance.CodeBits << 9);

                tempItems.Add(item);
            }

            _items = tempItems.ToArray();

            foreach (var instance in _aiObjectsTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var item = new TrAiItem
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    ObjectID = (ushort)instance.WadObjectId,
                    Room = (ushort)_roomsRemappingDictionary[instance.Room]
                };

                double angle = Math.Round(instance.RotationY * (65536.0 / 360.0));
                item.Angle = unchecked((short)(ushort)(Math.Max(0, Math.Min(ushort.MaxValue, angle))));
                item.OCB = (ushort)instance.Ocb;
                item.Flags |= (ushort)(instance.CodeBits << 1);

                tempAiObjects.Add(item);
            }

            _aiItems = tempAiObjects.ToArray();

            ReportProgress(30, "    Number of items: " + _items.Length);
            ReportProgress(31, "    Number of AI objects: " + _aiItems.Length);
        }
    }
}
