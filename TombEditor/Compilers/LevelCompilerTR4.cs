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

        private tr_mesh[] _meshes;

        private uint[] _meshPointers;
        private tr_animation[] _animations;
        private tr_state_change[] _stateChanges;
        private tr_anim_dispatch[] _animDispatches;
        private short[] _animCommands;
        private int[] _meshTrees;
        private short[] _frames;
        private tr_moveable[] _moveables;
        private tr_staticmesh[] _staticMeshes;

        private byte[] _spr;

        private tr_sprite_texture[] _spriteTextures;
        private tr_sprite_sequence[] _spriteSequences;
        private tr_camera[] _cameras;
        private tr4_flyby_camera[] _flyByCameras;
        private tr_sound_source[] _soundSources;
        private tr_box[] _boxes;
        private ushort[] _overlaps;
        private tr_zone[] _zones;
        private uint _numAnimatedTextures;
        private tr_animatedTextures_set[] _animatedTextures;

        private tr_object_texture[] _objectTextures;
        private tr_item[] _items;
        private tr_ai_item[] _aiItems;

        private uint _numSoundDetails;

        // texture data
        private List<tr_object_texture> _tempObjectTextures;

        private LevelTexture[] _tempTexturesArray;
        private Dictionary<int, int> _texturesIdTable;

        private byte[] _roomTexturePages;
        private byte[] _objectTexturePages;
        private byte[] _spriteTexturePages;
        private int _numRoomTexturePages;
        private int _numobjectTexturePages;
        private int _numSpriteTexturePages;

        // Temporary dictionaries for mapping editor IDs to level IDs
        private Dictionary<int, int> _moveablesTable;

        private Dictionary<int, int> _cameraTable;
        private Dictionary<int, int> _sinkTable;
        private Dictionary<int, int> _aiObjectsTable;
        private Dictionary<int, int> _soundSourcesTable;
        private Dictionary<int, int> _flybyTable;

        // Animated textures
        private List<int> _animTexturesRooms = new List<int>();
        private List<int> _animTexturesGeneral = new List<int>();

        private byte[] _bufferSamples;

        private Dictionary<string, int> _importedTexturesDictionary;

        public LevelCompilerTr4(Level level, string dest, IProgressReporter progressReporter) 
            : base(level, dest, progressReporter)
        {}

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
        }

        private void CompileLevelTask2()
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriterEx(stream))
                foreach (var sound in _level.Wad.OriginalWad.Sounds)
                {
                    byte[] soundData = _level.Settings.ReadSound(sound, _level.Settings.IgnoreMissingSounds);
                    writer.Write(soundData.GetLength(0));
                    writer.Write(soundData.GetLength(0));
                    writer.Write(soundData);
                }

            _bufferSamples = stream.ToArray();
        }

        private void CompileLevelTask3()
        {
            CopyWadData();
        }

        public void CompileLevel()
        {
            var watch = new Stopwatch();
            watch.Start();

            ReportProgress(0, "Tomb Raider IV Level Compiler by MontyTRC");

            if (_level.Wad == null)
                throw new NotSupportedException("A wad must be loaded to compile to *.tr4.");

            if (_level.TextureMap == null)
                throw new NotSupportedException("A texture must be loaded to compile to *.tr4.");

            // Prepare textures in four threads
            var task1 = Task.Factory.StartNew(PrepareRoomTextures);
            var task2 = Task.Factory.StartNew(BuildWadTexturePages);
            //Task task3 = Task.Factory.StartNew(BuildSprites);
            Task task4 = Task.Factory.StartNew(PrepareFontAndSkyTexture);

            // Wait for texture threads
            Task.WaitAll(task1, task2, task4);

            BuildSprites();

            // Now combine all texture data in the final texture map
            PrepareTextures();

            // Build all level data in three threads
            var task5 = Task.Factory.StartNew(CompileLevelTask1);
            var task6 = Task.Factory.StartNew(CompileLevelTask2);
            var task7 = Task.Factory.StartNew(CompileLevelTask3);

            // Wait for all threads to complete
            Task.WaitAll(task5, task6, task7);

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

            _soundSourcesTable = new Dictionary<int, int>();

            foreach (var obj in _level.Objects.Where(obj => obj.Value.Type == ObjectInstanceType.SoundSource).Select(obj => obj.Key))
            {
                _soundSourcesTable.Add(obj, _soundSourcesTable.Count);
            }

            var tempSoundSources = new List<tr_sound_source>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _soundSourcesTable.Keys.Select(src => (SoundSourceInstance) _level.Objects[src]))
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var source = new tr_sound_source
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

            int k = 0;
            _cameraTable = new Dictionary<int, int>();
            foreach (var obj in _level.Objects.Where(obj => obj.Value.Type == ObjectInstanceType.Camera).Select(obj => obj.Key))
            {
                _cameraTable.Add(obj, k++);
            }

            _sinkTable = new Dictionary<int, int>();
            foreach (var obj in _level.Objects.Where(obj => obj.Value.Type == ObjectInstanceType.Sink).Select(obj => obj.Key))
            {
                _sinkTable.Add(obj, k++);
            }

            _flybyTable = new Dictionary<int, int>();
            foreach (var obj in _level.Objects.Where(obj => obj.Value.Type == ObjectInstanceType.FlyByCamera).Select(obj => obj.Key))
            {
                _flybyTable.Add(obj, k++);
            }

            var tempCameras = new List<tr_camera>();

            foreach (var instance in _cameraTable.Keys.Select(cam => (CameraInstance) _level.Objects[cam]))
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var camera = new tr_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = (short)_roomsRemappingDictionary[_level.Rooms.ReferenceIndexOf(instance.Room)]
                };

                if (instance.Fixed)
                    camera.Flags = 0x01;

                tempCameras.Add(camera);
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _sinkTable.Keys.Select(sink => (SinkInstance) _level.Objects[sink]))
            {
                int xSector = (int) Math.Floor(instance.Position.X / 1024);
                int zSector = (int) Math.Floor(instance.Position.Z / 1024);

                var tempRoom = _tempRooms[instance.Room];
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var camera = new tr_camera
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

            var tempFlyby = new List<tr4_flyby_camera>();

            foreach (var instance in _flybyTable.Keys.Select(
                flyby => (FlybyCameraInstance) _level.Objects[flyby]))
            {
                Vector3 Direction = instance.GetDirection();
                Vector3 Position = instance.Room.WorldPos + instance.Position;
                var flyby = new tr4_flyby_camera
                {
                    X = (int)Math.Round(Position.X),
                    Y = (int)Math.Round(-Position.Y),
                    Z = (int)Math.Round(Position.Z),
                    Room = _roomsRemappingDictionary[_level.Rooms.ReferenceIndexOf(instance.Room)],
                    FOV = (ushort)(182 * instance.Fov),
                    Roll = (short)(182 * instance.Roll),
                    Timer = (ushort)instance.Timer,
                    Speed = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Speed * 655.0f))),
                    Sequence = (byte)instance.Sequence,
                    Index = (byte)instance.Number,
                    Flags = instance.Flags
                };

                flyby.DirectionX = (int)(flyby.X + 1024 * Math.Cos(MathUtil.DegreesToRadians(instance.RotationX)) * Math.Sin(MathUtil.DegreesToRadians(instance.RotationY)));
                flyby.DirectionY = (int)(flyby.Y - 1024 * Math.Sin(MathUtil.DegreesToRadians(instance.RotationX)));
                flyby.DirectionZ = (int)(flyby.Z + 1024 * Math.Cos(MathUtil.DegreesToRadians(instance.RotationX)) * Math.Cos(MathUtil.DegreesToRadians(instance.RotationY)));

                tempFlyby.Add(flyby);
            }

            tempFlyby.Sort(new ComparerFlyBy());

            _flyByCameras = tempFlyby.ToArray();

            ReportProgress(45, "    Number of cameras: " + _cameraTable.Count);
            ReportProgress(45, "    Number of flyby cameras: " + tempFlyby.Count);
            ReportProgress(45, "    Number of sinks: " + _sinkTable.Count);
        }

        private void BuildSprites()
        {
            ReportProgress(9, "Building sprites");
            ReportProgress(9, "Reading " + _level.Wad.OriginalWad.BaseName + ".swd");

            using (var reader = new BinaryReaderEx(new FileStream(
                _level.Wad.OriginalWad.BasePath + "\\" + _level.Wad.OriginalWad.BaseName + ".swd", FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                // Version
                reader.ReadUInt32();

                //Sprite texture array
                _spriteTextures = new tr_sprite_texture[reader.ReadUInt32()];
                for (int i = 0; i < _spriteTextures.Length; i++)
                {
                    byte[] buffer;
                    reader.ReadBlockArray(out buffer, 16);

                    _spriteTextures[i] = new tr_sprite_texture
                    {
                        Tile = (ushort)(_numRoomTexturePages + _numobjectTexturePages),
                        X = buffer[0],
                        Y = buffer[1],
                        Width = (ushort)(buffer[5] * 256),
                        Height = (ushort)(buffer[7] * 256),
                        LeftSide = buffer[0],
                        TopSide = buffer[1],
                        RightSide = (short)(buffer[0] + buffer[5] + 1),
                        BottomSide = (short)(buffer[1] + buffer[7] + 1)
                    };
                }

                // Unknown value
                int spriteDataSize = reader.ReadInt32();

                // Load the real sprite texture data
                _numSpriteTexturePages = spriteDataSize / (65536 * 3);
                if ((spriteDataSize % (65536 * 3)) != 0)
                    _numSpriteTexturePages++;

                _spriteTexturePages = new byte[256 * 256 * _numSpriteTexturePages * 4];

                int bytesRead = 0;

                for (int y = 0; y < _numSpriteTexturePages * 256; y++)
                {
                    if (bytesRead == spriteDataSize)
                        break;

                    for (int x = 0; x < 256; x++)
                    {
                        if (bytesRead == spriteDataSize)
                            break;

                        byte r = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte b = reader.ReadByte();

                        bytesRead += 3;

                        if (r == 255 & g == 0 && b == 255)
                        {
                            _spriteTexturePages[y * 1024 + 4 * x + 0] = 0;
                            _spriteTexturePages[y * 1024 + 4 * x + 1] = 0;
                            _spriteTexturePages[y * 1024 + 4 * x + 2] = 0;
                            _spriteTexturePages[y * 1024 + 4 * x + 3] = 0;
                        }
                        else
                        {
                            _spriteTexturePages[y * 1024 + 4 * x + 0] = b;
                            _spriteTexturePages[y * 1024 + 4 * x + 1] = g;
                            _spriteTexturePages[y * 1024 + 4 * x + 2] = r;
                            _spriteTexturePages[y * 1024 + 4 * x + 3] = 255;
                        }
                    }
                }

                // Sprite sequences
                reader.ReadBlockArray(out _spriteSequences, reader.ReadUInt32());
            }
        }

        private void CopyWadData()
        {
            ReportProgress(11, "Converting WAD data to TR4 format");

            var wad = _level.Wad.OriginalWad;

            ReportProgress(12, "    Number of animations: " + wad.Animations.Count);

            _animations = new tr_animation[wad.Animations.Count];
            for (int i = 0; i < _animations.Length; i++)
            {
                _animations[i] = new tr_animation
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

            _stateChanges = new tr_state_change[wad.Changes.Count];
            for (int i = 0; i < _stateChanges.Length; i++)
            {
                _stateChanges[i] = new tr_state_change
                {
                    AnimDispatch = wad.Changes[i].DispatchesIndex,
                    NumAnimDispatches = wad.Changes[i].NumDispatches,
                    StateID = wad.Changes[i].StateId
                };
            }

            ReportProgress(14, "    Number of animation dispatches: " + wad.Dispatches.Count);

            _animDispatches = new tr_anim_dispatch[wad.Dispatches.Count];
            for (int i = 0; i < _animDispatches.Length; i++)
            {
                _animDispatches[i] = new tr_anim_dispatch
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

            _moveables = new tr_moveable[wad.Moveables.Count];
            for (int i = 0; i < _moveables.Length; i++)
            {
                _moveables[i] = new tr_moveable
                {
                    Animation = (ushort) wad.Moveables[i].AnimationIndex,
                    FrameOffset = wad.Moveables[i].KeyFrameOffset,
                    MeshTree = wad.Moveables[i].LinksIndex,
                    NumMeshes = wad.Moveables[i].NumPointers,
                    ObjectID = wad.Moveables[i].ObjectID,
                    StartingMesh = wad.Moveables[i].PointerIndex
                };
            }

            ReportProgress(18, "    Number of static meshes: " + wad.StaticMeshes.Count);

            _staticMeshes = new tr_staticmesh[wad.StaticMeshes.Count];
            for (int i = 0; i < _staticMeshes.Length; i++)
            {
                _staticMeshes[i] = new tr_staticmesh
                {
                    CollisionBox = new tr_bounding_box
                    {
                        X1 = wad.StaticMeshes[i].CollisionX1,
                        Y1 = wad.StaticMeshes[i].CollisionY1,
                        Z1 = wad.StaticMeshes[i].CollisionZ1,
                        X2 = wad.StaticMeshes[i].CollisionX2,
                        Y2 = wad.StaticMeshes[i].CollisionY2,
                        Z2 = wad.StaticMeshes[i].CollisionZ2
                    },
                    VisibilityBox = new tr_bounding_box
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

        private short BuildRoomTextureInfo(BlockFace face)
        {
            var tile = new tr_object_texture();
            var tex = _tempTexturesArray[_texturesIdTable[face.Texture]];

            // Texture page
            tile.Tile = (ushort)tex.NewPage;
            if (face.Shape == BlockFaceShape.Triangle)
                tile.Tile |= 0x8000;

            // Attributes
            tile.Attributes = 0;
            if (tex.AlphaTest)
                tile.Attributes = 1;
            if (face.Transparent)
                tile.Attributes = 2;

            // Flags
            tile.Flags = 0x8000;

            tile.Xsize = (uint)tex.Width - 1;
            tile.Ysize = (uint)tex.Height - 1;

            int tmpWidth = tex.Width - 1;
            int tmpHeight = tex.Height - 1;

            // Texture UV
            if (face.Shape == BlockFaceShape.Triangle)
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!face.Flipped)
                {
                    switch (face.TextureTriangle)
                    {
                        case TextureTileType.TriangleNW:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)tex.NewX,
                                Xpixel = 0,
                                Ycoordinate = (byte)tex.NewY,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.NewX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)tex.NewY,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)tex.NewX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(tex.NewY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Flags |= 0x00;
                            break;
                        case TextureTileType.TriangleNE:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.NewX + tmpWidth - 1),
                                Xpixel = 255,
                                Ycoordinate = (byte)tex.NewY,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.NewX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(tex.NewY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)tex.NewX,
                                Xpixel = 0,
                                Ycoordinate = (byte)tex.NewY,
                                Ypixel = 0
                            };

                            tile.Flags |= 0x01;
                            break;
                        case TextureTileType.TriangleSE:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.NewX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(tex.NewY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)tex.NewX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(tex.NewY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.NewX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)tex.NewY,
                                Ypixel = 0
                            };

                            tile.Flags |= 0x02;
                            break;
                        case TextureTileType.TriangleSW:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)tex.NewX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(tex.NewY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)tex.NewX,
                                Xpixel = 0,
                                Ycoordinate = (byte)tex.NewY,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.NewX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(tex.NewY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Flags |= 0x03;
                            break;
                    }
                }
                else
                {
                    if (face.TextureTriangle == TextureTileType.TriangleNW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.NewX + tmpWidth),
                            Xpixel = 255,
                            Ycoordinate = (byte)tex.NewY,
                            Ypixel = 0
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)tex.NewX,
                            Xpixel = 0,
                            Ycoordinate = (byte)tex.NewY,
                            Ypixel = 0
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.NewX + tmpWidth),
                            Xpixel = 255,
                            Ycoordinate = (byte)(tex.NewY + tmpHeight - 1),
                            Ypixel = 255
                        };

                        tile.Flags |= 0x04;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleNE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)tex.NewX,
                            Xpixel = 0,
                            Ycoordinate = (byte)tex.NewY,
                            Ypixel = 0
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)tex.NewX,
                            Xpixel = 0,
                            Ycoordinate = (byte)(tex.NewY + tmpHeight),
                            Ypixel = 255
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.NewX + tmpWidth),
                            Xpixel = 255,
                            Ycoordinate = (byte)tex.NewY,
                            Ypixel = 0
                        };

                        tile.Flags |= 0x05;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleSE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)tex.NewX,
                            Xpixel = 0,
                            Ycoordinate = (byte)(tex.NewY + tmpHeight),
                            Ypixel = 255
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.NewX + tmpWidth),
                            Xpixel = 255,
                            Ycoordinate = (byte)(tex.NewY + tmpHeight),
                            Ypixel = 255
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)tex.NewX,
                            Xpixel = 0,
                            Ycoordinate = (byte)tex.NewY,
                            Ypixel = 0
                        };

                        tile.Flags |= 0x06;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleSW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.NewX + tmpWidth),
                            Xpixel = 255,
                            Ycoordinate = (byte)(tex.NewY + tmpHeight),
                            Ypixel = 255
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.NewX + tmpWidth),
                            Xpixel = 255,
                            Ycoordinate = (byte)tex.NewY,
                            Ypixel = 0
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)tex.NewX,
                            Xpixel = 0,
                            Ycoordinate = (byte)(tex.NewY + tmpHeight),
                            Ypixel = 255
                        };

                        tile.Flags |= 0x07;
                    }
                }

                tile.Vertices[3] = new tr_object_texture_vert
                {
                    Xcoordinate = 0,
                    Xpixel = 0,
                    Ycoordinate = 0,
                    Ypixel = 0
                };
            }
            else
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!face.Flipped)
                {
                    tile.Vertices[0] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)tex.NewX,
                        Xpixel = 0,
                        Ycoordinate = (byte)tex.NewY,
                        Ypixel = 0
                    };

                    tile.Vertices[1] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.NewX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)tex.NewY,
                        Ypixel = 0
                    };

                    tile.Vertices[2] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.NewX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)(tex.NewY + tmpHeight),
                        Ypixel = 255
                    };

                    tile.Vertices[3] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)tex.NewX,
                        Xpixel = 0,
                        Ycoordinate = (byte)(tex.NewY + tmpHeight),
                        Ypixel = 255
                    };
                }
                else
                {
                    tile.Vertices[0] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.NewX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)tex.NewY,
                        Ypixel = 0
                    };

                    tile.Vertices[1] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)tex.NewX,
                        Xpixel = 0,
                        Ycoordinate = (byte)tex.NewY,
                        Ypixel = 0
                    };

                    tile.Vertices[2] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)tex.NewX,
                        Xpixel = 0,
                        Ycoordinate = (byte)(tex.NewY + tmpHeight),
                        Ypixel = 255
                    };

                    tile.Vertices[3] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.NewX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)(tex.NewY + tmpHeight),
                        Ypixel = 255
                    };

                    tile.Flags |= 0x01;
                }
            }

            int test = TextureInfoExists(tile);
            if (test == -1)
            {
                _tempObjectTextures.Add(tile);
                int newId = _tempObjectTextures.Count - 1;
                if (face.DoubleSided)
                    newId |= 0x8000;

                // Is this texture part of an animated set?
                if ((tex.Width != 64 || tex.Height != 64) && (tex.Width != 32 || tex.Height != 32))
                    return (short)newId;

                int animatedSet = -1;
                int animatedTextureTile = -1;
                short deltaX = 0;
                short deltaY = 0;

                for (int i = 0; i < _level.AnimatedTextures.Count; i++)
                {
                    for (int j = 0; j < _level.AnimatedTextures[i].Textures.Count; j++)
                    {
                        var current = _level.AnimatedTextures[i].Textures[j];

                        // Check if texture is contained in an animated range
                        if (!(tex.X >= current.X && (tex.X + tex.Width) <= (current.X + 64) &&
                              tex.Y >= current.Y && (tex.Y + tex.Height) <= (current.Y + 64) && 
                              tex.Page == current.Page))
                            continue;
                                                
                        animatedSet = i;
                        animatedTextureTile = j;

                        deltaX = (short)(tex.X - current.X);
                        deltaY = (short)(tex.Y - current.Y);

                        break;
                    }
                }

                if (animatedSet == -1)
                    return (short)newId;

                {
                    if (!_animTexturesRooms.Contains(newId & 0x7fff))
                        _animTexturesRooms.Add(newId & 0x7fff);

                    // Search for a compatible variant
                    int foundVariant = -1;

                    for (int i = 0; i < _level.AnimatedTextures[animatedSet].Variants.Count; i++)
                    {
                        var variant = _level.AnimatedTextures[animatedSet].Variants[i];
                        bool isTriangle = face.Shape == BlockFaceShape.Triangle;

                        var current = _level.AnimatedTextures[animatedSet].Textures[animatedTextureTile];

                        if (variant.Flipped != face.Flipped || variant.Size != tex.Width ||
                            variant.DeltaX != (tex.X - current.X) || variant.DeltaY != (tex.Y - current.Y) ||
                            variant.Transparent != face.Transparent ||
                            ((!variant.IsTriangle || !isTriangle || variant.Triangle != face.TextureTriangle) &&
                             (variant.IsTriangle || isTriangle)))
                            continue;

                        foundVariant = i;
                        break;
                    }

                    if (foundVariant == -1)
                    {
                        var newVariant = new AnimatedTextureSequenceVariant
                        {
                            Size = tex.Width,
                            Flipped = face.Flipped,
                            Triangle = face.TextureTriangle,
                            IsTriangle = face.Shape == BlockFaceShape.Triangle,
                            Transparent = face.Transparent,
                            DeltaX = deltaX,
                            DeltaY = deltaY
                        };

                        for (int j = 0; j < _level.AnimatedTextures[animatedSet].Textures.Count; j++)
                        {
                            var aTile = new AnimatedTextureVariantTile(j, -1);
                            newVariant.Tiles.Add(aTile);
                        }

                        _level.AnimatedTextures[animatedSet].Variants.Add(newVariant);

                        foundVariant = _level.AnimatedTextures[animatedSet].Variants.Count - 1;
                    }

                    if (_level.AnimatedTextures[animatedSet].Variants[foundVariant].Tiles[animatedTextureTile]
                            .NewId == -1)
                    {
                        _level.AnimatedTextures[animatedSet].Variants[foundVariant].Tiles[animatedTextureTile].NewId
                            = (short)(newId & 0x7fff);
                    }
                }

                return (short)newId;
            }

            if (face.DoubleSided)
                test = test | 0x8000;
            return (short)test;
        }

        private short BuildAnimatedTextureInfo(AnimatedTextureSequenceVariant aSet, LevelTexture tex)
        {
            var tile = new tr_object_texture {Tile = (ushort)tex.NewPage};

            // Texture page
            if (aSet.IsTriangle)
                tile.Tile |= 0x8000;

            // Attributes
            tile.Attributes = 0;
            if (tex.AlphaTest)
                tile.Attributes = 1;
            if (aSet.Transparent)
                tile.Attributes = 2;

            // Flags
            tile.Flags = 0x8000;

            tile.Xsize = (uint)aSet.Size - 1; // tex.Width - 1;
            tile.Ysize = (uint)aSet.Size - 1; // tex.Height - 1;

            int tmpWidth = aSet.Size - 1; // tex.Width - 1;
            int tmpHeight = aSet.Size - 1; // tex.Height - 1;

            byte theX = (byte)(tex.NewX + aSet.DeltaX);
            byte theY = (byte)(tex.NewY + aSet.DeltaY);
            
            // Texture UV
            if (aSet.IsTriangle)
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!aSet.Flipped)
                {
                    switch (aSet.Triangle)
                    {
                        case TextureTileType.TriangleNW:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Flags |= 0x00;
                            break;
                        case TextureTileType.TriangleNE:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth - 1),
                                Xpixel = 255,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Flags |= 0x01;
                            break;
                        case TextureTileType.TriangleSE:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Flags |= 0x02;
                            break;
                        case TextureTileType.TriangleSW:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Flags |= 0x03;
                            break;
                    }
                }
                else
                {
                    switch (aSet.Triangle)
                    {
                        case TextureTileType.TriangleNW:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(theY + tmpHeight - 1),
                                Ypixel = 255
                            };

                            tile.Flags |= 0x04;
                            break;
                        case TextureTileType.TriangleNE:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Flags |= 0x05;
                            break;
                        case TextureTileType.TriangleSE:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Flags |= 0x06;
                            break;
                        case TextureTileType.TriangleSW:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(theX + tmpWidth),
                                Xpixel = 255,
                                Ycoordinate = (byte)theY,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)theX,
                                Xpixel = 0,
                                Ycoordinate = (byte)(theY + tmpHeight),
                                Ypixel = 255
                            };

                            tile.Flags |= 0x07;
                            break;
                    }
                }

                tile.Vertices[3] = new tr_object_texture_vert
                {
                    Xcoordinate = 0,
                    Xpixel = 0,
                    Ycoordinate = 0,
                    Ypixel = 0
                };
            }
            else
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!aSet.Flipped)
                {
                    tile.Vertices[0] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)theX,
                        Xpixel = 0,
                        Ycoordinate = (byte)theY,
                        Ypixel = 0
                    };

                    tile.Vertices[1] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(theX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)theY,
                        Ypixel = 0
                    };

                    tile.Vertices[2] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(theX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)(theY + tmpHeight),
                        Ypixel = 255
                    };

                    tile.Vertices[3] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)theX,
                        Xpixel = 0,
                        Ycoordinate = (byte)(theY + tmpHeight),
                        Ypixel = 255
                    };
                }
                else
                {
                    tile.Vertices[0] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(theX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)theY,
                        Ypixel = 0
                    };

                    tile.Vertices[1] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)theX,
                        Xpixel = 0,
                        Ycoordinate = (byte)theY,
                        Ypixel = 0
                    };

                    tile.Vertices[2] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)theX,
                        Xpixel = 0,
                        Ycoordinate = (byte)(theY + tmpHeight),
                        Ypixel = 255
                    };

                    tile.Vertices[3] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(theX + tmpWidth),
                        Xpixel = 255,
                        Ycoordinate = (byte)(theY + tmpHeight),
                        Ypixel = 255
                    };

                    tile.Flags |= 0x01;
                }
            }

            int test = TextureInfoExists(tile);
            if (test != -1)
                return (short)test;

            _tempObjectTextures.Add(tile);
            int newId = _tempObjectTextures.Count - 1;

            return (short)newId;
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

        private bool BuildBox(Room room, int x, int z, int xm, int xM, int zm, int zM, out tr_box_aux box)
        {
            var tempRoom = _tempRooms[room];
            var aux = tempRoom.AuxSectors[x, z];

            int xMin = 0;
            int xMax = 0;
            int zMin = 0;
            int zMax = 255;

            int xc = x;
            int zc = z;

            // Find box corners in direction -X
            for (int x2 = xc; x2 > 0; x2--)
            {
                var aux2 = tempRoom.AuxSectors[x2, zc];

                if (aux2.WallPortal != null)
                {
                    xMin = x2;
                    break;
                }

                if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) ||
                    (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor ||
                    (aux.SoftSlope != aux2.SoftSlope))
                    break;
                xMin = x2;
            }

            // Find box corners in direction +X
            for (int x2 = xc; x2 < room.NumXSectors - 1; x2++)
            {
                var aux2 = tempRoom.AuxSectors[x2, zc];

                if (aux2.WallPortal != null)
                {
                    xMax = x2;
                    break;
                }

                if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) ||
                    (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor ||
                    (aux.SoftSlope != aux2.SoftSlope))
                    break;
                xMax = x2;
            }

            // Find box corners in direction -Z
            for (int x2 = xMin; x2 <= xMax; x2++)
            {
                int tmpZ = 0;
                for (int z2 = zc; z2 > 0; z2--)
                {
                    var aux2 = tempRoom.AuxSectors[x2, z2];

                    if (aux2.WallPortal != null)
                    {
                        tmpZ = z2;
                        break;
                    }

                    if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) ||
                        (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) ||
                        aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope))
                        break;

                    tmpZ = z2;
                }

                if (tmpZ > zMin)
                    zMin = tmpZ;
            }

            // Find box corners in direction +Z
            for (int x2 = xMin; x2 <= xMax; x2++)
            {
                int tmpZ = 255;

                for (int z2 = zc; z2 < room.NumZSectors - 1; z2++)
                {
                    var aux2 = tempRoom.AuxSectors[x2, z2];

                    if (aux2.WallPortal != null)
                    {
                        tmpZ = z2;
                        break;
                    }

                    if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) ||
                        (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) ||
                        aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope))
                        break;

                    tmpZ = z2;
                }

                if (tmpZ < zMax)
                    zMax = tmpZ;
            }

            box = new tr_box_aux
            {
                Xmin = (byte)(xMin + tempRoom.Info.X / 1024),
                Xmax = (byte)(xMax + tempRoom.Info.X / 1024 + 1),
                Zmin = (byte)(zMin + tempRoom.Info.Z / 1024),
                Zmax = (byte)(zMax + tempRoom.Info.Z / 1024 + 1),
                TrueFloor = GetMostDownFloor(room, x, z),
                IsolatedBox = aux.Box,
                Monkey = aux.Monkey,
                Portal = aux.Portal,
                Room = (short) _level.Rooms.ReferenceIndexOf(room)
            };

            // Cut the box if needed
            if (xm == 0 || zm == 0 || xM == 0 || zM == 0)
                return true;

            if (box.Xmin < xm)
                box.Xmin = (byte) xm;
            if (box.Xmax > xM)
                box.Xmax = (byte) xM;
            if (box.Zmin < zm)
                box.Zmin = (byte) zm;
            if (box.Zmax > zM)
                box.Zmax = (byte) zM;

            if (box.Xmax - box.Xmin <= 0)
                return false;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (box.Zmax - box.Zmin <= 0)
                return false;

            return true;
        }

        private short GetMostDownFloor(Room room, int x, int z)
        {
            var tempRoom = _tempRooms[room];
            while (true)
            {
                var sector = GetSector(tempRoom, x, z);
                if (sector.RoomBelow == 255)
                {
                    var aux3 = tempRoom.AuxSectors[x, z];
                    return (short)(aux3.LowestFloor * 256);
                }

                var roomBelow = _level.Rooms[sector.RoomBelow];
                var tempRoomBelow = _tempRooms[roomBelow];

                int x2 = tempRoom.Info.X / 1024 + x - tempRoomBelow.Info.X / 1024;
                int z2 = tempRoom.Info.Z / 1024 + z - tempRoomBelow.Info.Z / 1024;

                var sector2 = GetSector(tempRoomBelow, x2, z2);
                var aux2 = tempRoomBelow.AuxSectors[x2, z2];

                if (sector2.RoomBelow == 255 || aux2.IsFloorSolid)
                {
                    return (short)(aux2.LowestFloor * 256);
                }

                room = roomBelow;
                tempRoom = tempRoomBelow;
                x = x2;
                z = z2;
            }
        }

        private bool GetMostDownFloorAndRoom(Room room, int x, int z, out Room roomIndex, out short floor)
        {
            var tempRoom = _tempRooms[room];
            while (true)
            {
                var sector = GetSector(tempRoom, x, z);
                if (sector.RoomBelow == 255)
                {
                    roomIndex = room;
                    floor = sector.Floor;
                    return true;
                }

                var roomBelow = _level.Rooms[sector.RoomBelow];
                var tempRoomBelow = _tempRooms[roomBelow];

                int x2 = tempRoom.Info.X / 1024 + x - tempRoomBelow.Info.X / 1024;
                int z2 = tempRoom.Info.Z / 1024 + z - tempRoomBelow.Info.Z / 1024;

                var sector2 = GetSector(tempRoomBelow, x2, z2);

                if (sector2.RoomBelow == 255)
                {
                    roomIndex = roomBelow;
                    floor = sector2.Floor;
                    return true;
                }

                room = roomBelow;
                tempRoom = tempRoomBelow;
                x = x2;
                z = z2;
            }
        }

        private bool FindMonkeyFloor(Room room, int x, int z)
        {
            var tempRoom = _tempRooms[room];
            while (true)
            {
                var sector = GetSector(tempRoom, x, z);
                if (sector.RoomBelow == 255)
                {
                    return tempRoom.AuxSectors[x, z].Monkey;
                }

                var roomBelow = _level.Rooms[sector.RoomBelow];
                var tempRoomBelow = _tempRooms[roomBelow];

                int x2 = tempRoom.Info.X / 1024 + x - tempRoomBelow.Info.X / 1024;
                int z2 = tempRoom.Info.Z / 1024 + z - tempRoomBelow.Info.Z / 1024;

                var sector2 = GetSector(tempRoomBelow, x2, z2);

                if (sector2.RoomBelow == 255)
                {
                    return tempRoomBelow.AuxSectors[x2, z2].Monkey;
                }

                room = roomBelow;
                tempRoom = tempRoomBelow;
                x = x2;
                z = z2;
            }
        }

        private void ConvertWadMeshes()
        {
            ReportProgress(11, "Converting WAD meshes to TR4 format");

            var wad = _level.Wad.OriginalWad;

            ReportProgress(12, "    Number of meshes: " + wad.Meshes.Count);

            var tempMeshes = new List<tr_mesh>();

            foreach (var oldMesh in wad.Meshes)
            {
                var newMesh = new tr_mesh
                {
                    Center = new tr_vertex
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
                    newMesh.Normals = new tr_vertex[newMesh.NumNormals];
                    for (int k = 0; k < newMesh.NumNormals; k++)
                    {
                        newMesh.Normals[k] = new tr_vertex
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

                newMesh.NumVertices = (short) oldMesh.NumVertices;
                newMesh.Vertices = new tr_vertex[newMesh.NumVertices];

                for (int j = 0; j < newMesh.NumVertices; j++)
                {
                    newMesh.Vertices[j] = new tr_vertex
                    {
                        X = oldMesh.Vertices[j].X,
                        Y = oldMesh.Vertices[j].Y,
                        Z = oldMesh.Vertices[j].Z
                    };
                }

                var tempRectangles = new List<tr_face4>();
                var tempTriangles = new List<tr_face3>();

                for (int j = 0; j < oldMesh.NumPolygons; j++)
                {
                    var poly = oldMesh.Polygons[j];

                    if (poly.Shape == 9)
                    {
                        newMesh.NumTexturedRectangles++;

                        var rectangle = new tr_face4 {Vertices = new ushort[4]};

                        rectangle.Vertices[0] = poly.V1;
                        rectangle.Vertices[1] = poly.V2;
                        rectangle.Vertices[2] = poly.V3;
                        rectangle.Vertices[3] = poly.V4;

                        rectangle.Texture = BuildWadTextureInfo((short) poly.Texture, false, poly.Attributes);

                        if ((poly.Attributes & 0x02) == 0x02)
                        {
                            // Shine effect
                            short shine = (short)((poly.Attributes & 0x7c) >> 2);
                            rectangle.LightingEffect |= (short)(shine << 1);
                        }

                        if ((poly.Attributes & 0x01) == 0x01)
                        {
                            // Alpha trasparency
                            rectangle.LightingEffect |= 0x01;
                        }

                        tempRectangles.Add(rectangle);
                    }
                    else
                    {
                        newMesh.NumTexturedTriangles++;

                        var triangle = new tr_face3 {Vertices = new ushort[3]};

                        triangle.Vertices[0] = poly.V1;
                        triangle.Vertices[1] = poly.V2;
                        triangle.Vertices[2] = poly.V3;

                        triangle.Texture = BuildWadTextureInfo((short) poly.Texture, true, poly.Attributes);

                        if ((poly.Attributes & 0x02) == 0x02)
                        {
                            // Shine effect
                            short shine = (short)((poly.Attributes & 0x7c) >> 2);
                            triangle.LightingEffect |= (short)(shine << 1);
                        }

                        if ((poly.Attributes & 0x01) == 0x01)
                        {
                            // Alpha trasparency
                            triangle.LightingEffect |= 0x01;
                        }

                        tempTriangles.Add(triangle);
                    }
                }

                newMesh.TexturedRectangles = tempRectangles.ToArray();
                newMesh.TexturedTriangles = tempTriangles.ToArray();

                tempMeshes.Add(newMesh);
            }

            _meshes = tempMeshes.ToArray();
        }

        private void PrepareItems()
        {
            ReportProgress(18, "Building items table");

            _moveablesTable = new Dictionary<int, int>();
            _aiObjectsTable = new Dictionary<int, int>();

            foreach (var obj in _level.Objects.Where(obj => obj.Value.Type == ObjectInstanceType.Moveable))
            {
                uint objectId = ((MoveableInstance)obj.Value).WadObjectId;

                if (objectId >= 398 && objectId <= 406)
                {
                    _aiObjectsTable.Add(obj.Key, _aiObjectsTable.Count);
                }
                else
                {
                    _moveablesTable.Add(obj.Key, _moveablesTable.Count);
                }
            }

            var tempItems = new List<tr_item>();
            var tempAiObjects = new List<tr_ai_item>();

            foreach (var instance in _moveablesTable.Keys.Select(obj => (MoveableInstance) _level.Objects[obj]))
            {
                double angle = Math.Round(instance.Rotation * (65536.0 / 360.0));
                Vector3 position = instance.Room.WorldPos + instance.Position;

                var item = new tr_item
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    ObjectID = (short)instance.WadObjectId,
                    Room = (short)_roomsRemappingDictionary[_level.Rooms.ReferenceIndexOf(instance.Room)],
                    Angle = unchecked((short)((ushort)(Math.Max(0, Math.Min(ushort.MaxValue, angle))))),
                    Intensity1 = -1,
                    Intensity2 = instance.Ocb
                };

                if (instance.ClearBody)
                    item.Flags |= 0x80;
                if (instance.Invisible)
                    item.Flags |= 0x100;
                
                item.Flags |= (ushort)(instance.CodeBits << 9);

                tempItems.Add(item);
            }

            _items = tempItems.ToArray();

            foreach (var instance in _aiObjectsTable.Keys.Select(obj => (MoveableInstance) _level.Objects[obj]))
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                var item = new tr_ai_item
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    ObjectID = (ushort) instance.WadObjectId,
                    Room = (ushort)_roomsRemappingDictionary[_level.Rooms.ReferenceIndexOf(instance.Room)]
                };

                double angle = Math.Round(instance.Rotation * (65536.0 / 360.0));
                item.Angle = unchecked((short)(ushort)(Math.Max(0, Math.Min(ushort.MaxValue, angle))));
                item.OCB = (ushort)instance.Ocb;
                item.Flags |= (ushort)(instance.CodeBits << 1);

                tempAiObjects.Add(item);
            }

            _aiItems = tempAiObjects.ToArray();

            ReportProgress(30, "    Number of items: " + _items.Length);
            ReportProgress(31, "    Number of AI objects: " + _aiItems.Length);
        }

        private int TextureInfoExists(tr_object_texture txt)
        {
            for (int i = 0; i < _tempObjectTextures.Count; i++)
            {
                var txt2 = _tempObjectTextures[i];

                if (txt2.Vertices[0].Xcoordinate == txt.Vertices[0].Xcoordinate &&
                    txt2.Vertices[0].Xpixel == txt.Vertices[0].Xpixel &&
                    txt2.Vertices[0].Ycoordinate == txt.Vertices[0].Ycoordinate &&
                    txt2.Vertices[0].Ypixel == txt.Vertices[0].Ypixel &&
                    txt2.Vertices[1].Xcoordinate == txt.Vertices[1].Xcoordinate &&
                    txt2.Vertices[1].Xpixel == txt.Vertices[1].Xpixel &&
                    txt2.Vertices[1].Ycoordinate == txt.Vertices[1].Ycoordinate &&
                    txt2.Vertices[1].Ypixel == txt.Vertices[1].Ypixel &&
                    txt2.Vertices[2].Xcoordinate == txt.Vertices[2].Xcoordinate &&
                    txt2.Vertices[2].Xpixel == txt.Vertices[2].Xpixel &&
                    txt2.Vertices[2].Ycoordinate == txt.Vertices[2].Ycoordinate &&
                    txt2.Vertices[2].Ypixel == txt.Vertices[2].Ypixel &&
                    txt2.Vertices[3].Xcoordinate == txt.Vertices[3].Xcoordinate &&
                    txt2.Vertices[3].Xpixel == txt.Vertices[3].Xpixel &&
                    txt2.Vertices[3].Ycoordinate == txt.Vertices[3].Ycoordinate &&
                    txt2.Vertices[3].Ypixel == txt.Vertices[3].Ypixel &&
                    txt2.Tile == txt.Tile && txt2.Flags == txt.Flags && txt2.Attributes == txt.Attributes)
                    return i;
            }

            return -1;
        }

        private short BuildWadTextureInfo(short txt, bool triangle, short attributes)
        {
            int original = txt;
            var wad = _level.Wad.OriginalWad;

            bool isFlipped = false;
            int shape = original & 0x7000;

            int sign = original & 0x8000;

            if (sign == 0x8000)
            {
                isFlipped = true;
            }

            if (triangle)
            {
                txt = (short)((original & 0xfff));
            }

            if (txt < 0)
                txt = (short) -txt;

            var tile = new tr_object_texture();
            var tex = wad.Textures[txt];

            // Texture page
            tile.Tile = (ushort)(tex.Page + _numRoomTexturePages);
            if (triangle)
                tile.Tile |= 0x8000;

            // Attributes
            tile.Attributes = 0;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if ((attributes & 0x01) == 0x01)
            {
                tile.Attributes = 2; // Alpha trasparency
            }
            else
            {
                // I must check for magenta color
                tile.Attributes = 1; // Magenta trasparency, but for speed I must implement a check in the texture map
            }

            //if (tex.AlphaTest) tile.Tile = 1;
            if ((attributes & 0x01) == 0x01)
                tile.Attributes = 2;

            // Flags
            tile.Flags = (ushort)(isFlipped ? 1 : 0);

            tile.Xsize = tex.Width;
            tile.Ysize = tex.Height;

            // Texture UV
            if (triangle)
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!isFlipped)
                {
                    switch (shape)
                    {
                        case 0x00:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = tex.X,
                                Xpixel = 0,
                                Ycoordinate = tex.Y,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.X + tex.Width),
                                Xpixel = 255,
                                Ycoordinate = tex.Y,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = tex.X,
                                Xpixel = 0,
                                Ycoordinate = (byte)(tex.Y + tex.Height),
                                Ypixel = 255
                            };

                            tile.Flags = 0;
                            break;
                        case 0x2000:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.X + tex.Width),
                                Xpixel = 255,
                                Ycoordinate = tex.Y,
                                Ypixel = 0
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.X + tex.Width),
                                Xpixel = 255,
                                Ycoordinate = (byte)(tex.Y + tex.Height),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = tex.X,
                                Xpixel = 0,
                                Ycoordinate = tex.Y,
                                Ypixel = 0
                            };

                            tile.Flags = 1;
                            break;
                        case 0x4000:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.X + tex.Width),
                                Xpixel = 255,
                                Ycoordinate = (byte)(tex.Y + tex.Height),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = tex.X,
                                Xpixel = 0,
                                Ycoordinate = (byte)(tex.Y + tex.Height),
                                Ypixel = 255
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.X + tex.Width),
                                Xpixel = 255,
                                Ycoordinate = tex.Y,
                                Ypixel = 0
                            };

                            tile.Flags = 2;
                            break;
                        case 0x6000:
                            tile.Vertices[0] = new tr_object_texture_vert
                            {
                                Xcoordinate = tex.X,
                                Xpixel = 0,
                                Ycoordinate = (byte)(tex.Y + tex.Height),
                                Ypixel = 255
                            };

                            tile.Vertices[1] = new tr_object_texture_vert
                            {
                                Xcoordinate = tex.X,
                                Xpixel = 0,
                                Ycoordinate = tex.Y,
                                Ypixel = 0
                            };

                            tile.Vertices[2] = new tr_object_texture_vert
                            {
                                Xcoordinate = (byte)(tex.X + tex.Width),
                                Xpixel = 255,
                                Ycoordinate = (byte)(tex.Y + tex.Height),
                                Ypixel = 255
                            };

                            tile.Flags = 3;
                            break;
                    }
                }
                else
                {
                    if (shape == 0x00)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.X + tex.Width),
                            Xpixel = 255,
                            Ycoordinate = tex.Y,
                            Ypixel = 0
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = tex.X,
                            Xpixel = 0,
                            Ycoordinate = tex.Y,
                            Ypixel = 0
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.X + tex.Width),
                            Xpixel = 255,
                            Ycoordinate = (byte)(tex.Y + tex.Height),
                            Ypixel = 255
                        };

                        tile.Flags = 4;
                    }
                    else if (shape == 0x2000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = tex.X,
                            Xpixel = 0,
                            Ycoordinate = tex.Y,
                            Ypixel = 0
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = tex.X,
                            Xpixel = 0,
                            Ycoordinate = (byte)(tex.Y + tex.Height),
                            Ypixel = 255
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.X + tex.Width),
                            Xpixel = 255,
                            Ycoordinate = tex.Y,
                            Ypixel = 0
                        };

                        tile.Flags = 5;
                    }
                    else if (shape == 0x4000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = tex.X,
                            Xpixel = 0,
                            Ycoordinate = (byte)(tex.Y + tex.Height),
                            Ypixel = 255
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.X + tex.Width),
                            Xpixel = 255,
                            Ycoordinate = (byte)(tex.Y + tex.Height),
                            Ypixel = 255
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = tex.X,
                            Xpixel = 0,
                            Ycoordinate = tex.Y,
                            Ypixel = 0
                        };

                        tile.Flags = 6;
                    }
                    else if (shape == 0x6000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.X + tex.Width),
                            Xpixel = 255,
                            Ycoordinate = (byte)(tex.Y + tex.Height),
                            Ypixel = 255
                        };

                        tile.Vertices[1] = new tr_object_texture_vert
                        {
                            Xcoordinate = (byte)(tex.X + tex.Width),
                            Xpixel = 255,
                            Ycoordinate = tex.Y,
                            Ypixel = 0
                        };

                        tile.Vertices[2] = new tr_object_texture_vert
                        {
                            Xcoordinate = tex.X,
                            Xpixel = 0,
                            Ycoordinate = (byte)(tex.Y + tex.Height),
                            Ypixel = 255
                        };

                        tile.Flags = 7;
                    }
                }

                tile.Vertices[3] = new tr_object_texture_vert
                {
                    Xcoordinate = 255,
                    Xpixel = 0,
                    Ycoordinate = 255,
                    Ypixel = 0
                };
            }
            else
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!isFlipped)
                {
                    tile.Vertices[0] = new tr_object_texture_vert
                    {
                        Xcoordinate = tex.X,
                        Xpixel = 0,
                        Ycoordinate = tex.Y,
                        Ypixel = 0
                    };

                    tile.Vertices[1] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.X + tex.Width),
                        Xpixel = 255,
                        Ycoordinate = tex.Y,
                        Ypixel = 0
                    };

                    tile.Vertices[2] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.X + tex.Width),
                        Xpixel = 255,
                        Ycoordinate = (byte)(tex.Y + tex.Height),
                        Ypixel = 255
                    };

                    tile.Vertices[3] = new tr_object_texture_vert
                    {
                        Xcoordinate = tex.X,
                        Xpixel = 0,
                        Ycoordinate = (byte)(tex.Y + tex.Height),
                        Ypixel = 255
                    };

                    tile.Flags = 0;
                }
                else
                {
                    tile.Vertices[0] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.X + tex.Width),
                        Xpixel = 255,
                        Ycoordinate = tex.Y,
                        Ypixel = 0
                    };

                    tile.Vertices[1] = new tr_object_texture_vert
                    {
                        Xcoordinate = tex.X,
                        Xpixel = 0,
                        Ycoordinate = tex.Y,
                        Ypixel = 0
                    };

                    tile.Vertices[2] = new tr_object_texture_vert
                    {
                        Xcoordinate = tex.X,
                        Xpixel = 0,
                        Ycoordinate = (byte)(tex.Y + tex.Height),
                        Ypixel = 255
                    };

                    tile.Vertices[3] = new tr_object_texture_vert
                    {
                        Xcoordinate = (byte)(tex.X + tex.Width),
                        Xpixel = 255,
                        Ycoordinate = (byte)(tex.Y + tex.Height),
                        Ypixel = 255
                    };

                    tile.Flags = 1;
                }
            }

            tile.Unknown1 = tex.X;
            tile.Unknown2 = tex.Y;

            int test = TextureInfoExists(tile);
            if (test != -1)
                return (short)test;

            _tempObjectTextures.Add(tile);
            int newId = _tempObjectTextures.Count - 1;

            return (short)newId;
        }
    }
}
