using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TombEditor.Geometry;
using TombLib.Wad;
using TombLib.IO;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using SharpDX;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTr4 : LevelCompiler
    {
        private class ComparerFlyBy : IComparer<tr4_flyby_camera>
        {
            public int Compare(tr4_flyby_camera x, tr4_flyby_camera y)
            {
                if (x.Sequence != y.Sequence)
                {
                    return (x.Sequence > y.Sequence ? 1 : -1);
                }
                else
                {
                    if (x.Index == y.Index)
                        return 0;
                    return (x.Index > y.Index ? 1 : -1);
                }
            }
        }

        public int _version;

        public ushort _numRoomTextureTiles;
        public ushort _numObjectTextureTiles;
        public ushort _numBumpTextureTiles;

        public uint _texture32UncompressedSize;
        public uint _texture32CompressedSize;
        public byte[] _texture32;

        public uint _texture16UncompressedSize;
        public uint _texture16CompressedSize;
        public byte[] _texture16;

        public uint _miscTextureUncompressedSize;
        public uint _miscTextureCompressedSize;
        public byte[] _miscTexture;

        public uint _levelUncompressedSize;
        public uint _levelCompressedSize;

        public int _unused;

        public ushort _numRooms;
        public tr_room[] _rooms;

        public uint _numFloorData;
        public ushort[] _floorData;

        public uint _numMeshData;
        public uint _numMeshes;
        public tr_mesh[] _meshes;

        public uint _numMeshPointers;
        public uint[] _meshPointers;
        public uint _numAnimations;
        public tr_animation[] _animations;
        public uint _numStateChanges;
        public tr_state_change[] _stateChanges;
        public uint _numAnimDispatches;
        public tr_anim_dispatch[] _animDispatches;
        public uint _numAnimCommands;
        public short[] _animCommands;
        public uint _numMeshTrees;
        public int[] _meshTrees;
        public uint _numFrames;
        public short[] _frames;
        public uint _numMoveables;
        public tr_moveable[] _moveables;
        public uint _numStaticMeshes;
        public tr_staticmesh[] _staticMeshes;

        public byte[] _spr;

        public uint _numSpriteTextures;
        public tr_sprite_texture[] _spriteTextures;
        public uint _numSpriteSequences;
        public tr_sprite_sequence[] _spriteSequences;
        public uint _numCameras;
        public tr_camera[] _cameras;
        public uint _numFlyByCameras;
        public tr4_flyby_camera[] _flyByCameras;
        public uint _numSoundSources;
        public tr_sound_source[] _soundSources;
        public uint _numBoxes;
        public tr_box[] _boxes;
        public uint _numOverlaps;
        public ushort[] _overlaps;
        public tr_zone[] _zones;
        public uint _numAnimatedTextures;
        public tr_animatedTextures_set[] _animatedTextures;

        public byte[] _tex;

        public uint _numObjectTextures;
        public tr_object_texture[] _objectTextures;
        public uint _numItems;
        public tr_item[] _items;
        public uint _numAiItems;
        public tr_ai_item[] _aiItems;

        public short[] _soundMap;
        public uint _numSoundDetails;
        public tr_sound_details[] _soundDetails;

        // texture data
        private List<tr_object_texture> _tempObjectTextures;
        private LevelTexture[] _tempTexturesArray;
        private Dictionary<int, int> _texturesIdTable;
        private Dictionary<int, int> _roomsIdTable;

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
        private List<AnimatedTextureSequenceVariant> _tempAnimatedTextures;
        private List<int> _animTexturesRooms = new List<int>();
        private List<int> _animTexturesGeneral = new List<int>();

        private byte[] _bufferSamples;

        private List<tr_box_aux> _tempBoxes;

        public LevelCompilerTr4(Level level, string dest, BackgroundWorker bw = null) : base(level, dest, bw)
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
        }

        private void CompileLevelTask2()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriterEx writer = new BinaryWriterEx(stream);

            for (int i = 0; i < _editor.Level.Wad.OriginalWad.Sounds.Count; i++)
            {
                if (!File.Exists("Sounds\\Samples\\" + _editor.Level.Wad.OriginalWad.Sounds[i]))
                {
                    int sampleUncompressedSize = 0;
                    writer.Write(sampleUncompressedSize);
                    writer.Write(sampleUncompressedSize);
                }
                else
                {
                    BinaryReaderEx readerSound = new BinaryReaderEx(File.OpenRead("Sounds\\Samples\\" + _editor.Level.Wad.OriginalWad.Sounds[i]));
                    int sampleUncompressedSize = (int)readerSound.BaseStream.Length;
                    byte[] sample = readerSound.ReadBytes(sampleUncompressedSize);
                    writer.Write(sampleUncompressedSize);
                    writer.Write(sampleUncompressedSize);
                    writer.WriteBlockArray(sample);
                }
            }

            writer.Flush();
            writer.Close();

            _bufferSamples = stream.ToArray();
        }

        private void CompileLevelTask3()
        {
            CopyWadData();
        }

        public virtual bool CompileLevel()
        {
            // Force garbage collector to compact memory
            GC.Collect();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            ReportProgress(0, "Tomb Raider IV Level Compiler by MontyTRC");

            // Prepare textures in four threads
            Task task1 = Task.Factory.StartNew(PrepareRoomTextures);
            Task task2 = Task.Factory.StartNew(BuildWadTexturePages);
            //Task task3 = Task.Factory.StartNew(BuildSprites);
            Task task4 = Task.Factory.StartNew(PrepareFontAndSkyTexture);

            // Wait for texture threads
            Task.WaitAll(task1, task2, task4);

            BuildSprites();

            // Now combine all texture data in the final texture map
            PrepareTextures();

            // Build all level data in three threads
            Task task5 = Task.Factory.StartNew(CompileLevelTask1);
            Task task6 = Task.Factory.StartNew(CompileLevelTask2);
            Task task7 = Task.Factory.StartNew(CompileLevelTask3);

            // Wait for all threads to complete
            Task.WaitAll(task5, task6, task7);

            //Write the final level
            WriteLevelTR4();

            watch.Stop();
            long mills = watch.ElapsedMilliseconds;

            ReportProgress(100, "Elapsed time: " + (mills / 1000.0f));

            // Force garbage collector to compact memory
            GC.Collect();

            return true;
        }

        private void PrepareSounds()
        {
            ReportProgress(40, "Building sound sources");

            int i;

            _soundSourcesTable = new Dictionary<int, int>();

            int k = 0;
            for (i = 0; i < _editor.Level.Objects.Count; i++)
            {
                if (_editor.Level.Objects.ElementAt(i).Value.Type == ObjectInstanceType.Sound)
                {
                    _soundSourcesTable.Add(_editor.Level.Objects.ElementAt(i).Key, k);
                    k++;
                }
            }

            List<tr_sound_source> tempSoundSources = new List<tr_sound_source>();

            for (i = 0; i < _soundSourcesTable.Count; i++)
            {
                tr_sound_source source = new tr_sound_source();
                SoundInstance instance = (SoundInstance)_editor.Level.Objects[_soundSourcesTable.ElementAt(i).Key];
                tr_room newRoom = _rooms[_roomsIdTable[instance.Room]];

                source.X = (int)(_rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                source.Y = (int)(_rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                source.Z = (int)(_rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                source.SoundID = (ushort)instance.SoundID;
                source.Flags = 0x80; // (ushort)instance.Flags;

                tempSoundSources.Add(source);
            }

            _numSoundSources = (uint)tempSoundSources.Count;
            _soundSources = tempSoundSources.ToArray();

            ReportProgress(41, "    Number of sound sources: " + _numSoundSources);

        }

        private void BuildCamerasAndSinks()
        {
            ReportProgress(42, "Building cameras and sinks");

            _cameraTable = new Dictionary<int, int>();
            _sinkTable = new Dictionary<int, int>();
            _flybyTable = new Dictionary<int, int>();

            int k = 0;
            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                if (_editor.Level.Objects.ElementAt(i).Value.Type == ObjectInstanceType.Camera)
                {
                    _cameraTable.Add(_editor.Level.Objects.ElementAt(i).Key, k);
                    k++;
                }
            }

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                if (_editor.Level.Objects.ElementAt(i).Value.Type == ObjectInstanceType.Sink)
                {
                    _sinkTable.Add(_editor.Level.Objects.ElementAt(i).Key, k);
                    k++;
                }
            }

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                if (_editor.Level.Objects.ElementAt(i).Value.Type == ObjectInstanceType.FlyByCamera)
                {
                    _flybyTable.Add(_editor.Level.Objects.ElementAt(i).Key, k);
                    k++;
                }
            }

            List<tr_camera> tempCameras = new List<tr_camera>();

            for (int i = 0; i < _cameraTable.Count; i++)
            {
                tr_camera camera = new tr_camera();
                CameraInstance instance = (CameraInstance)_editor.Level.Objects[_cameraTable.ElementAt(i).Key];
                tr_room newRoom = _rooms[_roomsIdTable[instance.Room]];

                camera.X = (int)(_rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                camera.Y = (int)(_rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                camera.Z = (int)(_rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                camera.Room = (short)_roomsIdTable[instance.Room];
                if (instance.Fixed)
                    camera.Flags = 0x01;

                tempCameras.Add(camera);
            }

            for (int i = 0; i < _sinkTable.Count; i++)
            {
                tr_camera camera = new tr_camera();
                SinkInstance instance = (SinkInstance)_editor.Level.Objects[_sinkTable.ElementAt(i).Key];
                tr_room newRoom = _rooms[_roomsIdTable[instance.Room]];

                int xSector = (int)Math.Floor(instance.Position.X / 1024);
                int zSector = (int)Math.Floor(instance.Position.Z / 1024);

                camera.X = (int)(_rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                camera.Y = (int)(_rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                camera.Z = (int)(_rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                camera.Room = instance.Strength;
                camera.Flags = (ushort)((newRoom.Sectors[newRoom.NumZSectors * xSector + zSector].BoxIndex & 0x7f00) >> 4);

                tempCameras.Add(camera);
            }

            _numCameras = (uint)tempCameras.Count;
            _cameras = tempCameras.ToArray();

            List<tr4_flyby_camera> tempFlyby = new List<tr4_flyby_camera>();

            for (int i = 0; i < _flybyTable.Count; i++)
            {
                tr4_flyby_camera flyby = new tr4_flyby_camera();
                FlybyCameraInstance instance = (FlybyCameraInstance)_editor.Level.Objects[_flybyTable.ElementAt(i).Key];
                tr_room newRoom = _rooms[_roomsIdTable[instance.Room]];

                flyby.X = (int)(_rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                flyby.Y = (int)(_rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                flyby.Z = (int)(_rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                flyby.DirectionX = (int)(flyby.X + 1024 * Math.Cos(MathUtil.DegreesToRadians(instance.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(instance.DirectionY)));
                flyby.DirectionY = (int)(flyby.Y - 1024 * Math.Sin(MathUtil.DegreesToRadians(instance.DirectionX)));
                flyby.DirectionZ = (int)(flyby.Z + 1024 * Math.Cos(MathUtil.DegreesToRadians(instance.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(instance.DirectionY)));

                flyby.Room = _roomsIdTable[instance.Room];

                flyby.FOV = (ushort)(182 * instance.FOV);
                flyby.Roll = (short)(182 * instance.Roll);
                flyby.Timer = (ushort)instance.Timer;
                flyby.Speed = (ushort)(instance.Speed * 655);
                flyby.Sequence = (byte)instance.Sequence;
                flyby.Index = (byte)instance.Number;

                for (int j = 0; j < 16; j++)
                {
                    flyby.Flags |= (ushort)((instance.Flags[j] ? 1 : 0) << j);
                }

                tempFlyby.Add(flyby);
            }

            tempFlyby.Sort(new ComparerFlyBy());

            _numFlyByCameras = (uint)tempFlyby.Count;
            _flyByCameras = tempFlyby.ToArray();

            ReportProgress(45, "    Number of cameras: " + _cameraTable.Count);
            ReportProgress(45, "    Number of flyby cameras: " + tempFlyby.Count);
            ReportProgress(45, "    Number of sinks: " + _sinkTable.Count);

        }

        private bool BuildSprites()
        {
            try
            {
                ReportProgress(9, "Building sprites");
                ReportProgress(9, "Reading " + _editor.Level.Wad.OriginalWad.BaseName + ".swd");

                BinaryReaderEx reader = new BinaryReaderEx(File.OpenRead(_editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".swd"));

                // Version
                reader.ReadUInt32();

                //Sprite texture array
                _numSpriteTextures = reader.ReadUInt32();
                _spriteTextures = new tr_sprite_texture[_numSpriteTextures];
                for (int i = 0; i < _numSpriteTextures; i++)
                {
                    byte[] buffer = new byte[16];
                    reader.ReadBlockArray(out buffer, 16);

                    tr_sprite_texture texture = new tr_sprite_texture();

                    texture.Tile = (ushort)(_numRoomTexturePages + _numobjectTexturePages);
                    texture.X = buffer[0];
                    texture.Y = buffer[1];
                    texture.Width = (ushort)(buffer[5] * 256);
                    texture.Height = (ushort)(buffer[7] * 256);
                    texture.LeftSide = buffer[0];
                    texture.TopSide = buffer[1];
                    texture.RightSide = (short)(buffer[0] + buffer[5] + 1);
                    texture.BottomSide = (short)(buffer[1] + buffer[7] + 1);

                    _spriteTextures[i] = texture;
                }

                // Unknown value
                int spriteDataSize = reader.ReadInt32();

                // Load the real sprite texture data
                _numSpriteTexturePages = spriteDataSize / (65536 * 3);
                if ((spriteDataSize % (65536 * 3)) != 0)
                    _numSpriteTexturePages++;

                _spriteTexturePages = new byte[256 * 256 * _numSpriteTexturePages * 4];

                int x;
                int y;
                int bytesRead = 0;

                for (y = 0; y < _numSpriteTexturePages * 256; y++)
                {
                    if (bytesRead == spriteDataSize)
                        break;

                    for (x = 0; x < 256; x++)
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
                _numSpriteSequences = reader.ReadUInt32();
                _spriteSequences = new tr_sprite_sequence[_numSpriteSequences];
                reader.ReadBlockArray(out _spriteSequences, _numSpriteSequences);

                reader.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void CopyWadData()
        {
            ReportProgress(11, "Converting WAD data to TR4 format");

            TR4Wad wad = _editor.Level.Wad.OriginalWad;

            ReportProgress(12, "    Number of animations: " + wad.Animations.Count);

            _numAnimations = (uint)wad.Animations.Count;
            _animations = new tr_animation[wad.Animations.Count];
            for (int i = 0; i < _numAnimations; i++)
            {
                tr_animation animation = new tr_animation();
                animation.AnimCommand = wad.Animations[i].CommandOffset;
                animation.FrameEnd = wad.Animations[i].FrameEnd;
                animation.FrameStart = wad.Animations[i].FrameStart;
                animation.FrameOffset = wad.Animations[i].KeyFrameOffset;
                animation.FrameSize = wad.Animations[i].KeyFrameSize;
                animation.FrameRate = wad.Animations[i].FrameDuration;
                animation.NextAnimation = wad.Animations[i].NextAnimation;
                animation.NextFrame = wad.Animations[i].NextFrame;
                animation.NumAnimCommands = wad.Animations[i].NumCommands;
                animation.NumStateChanges = wad.Animations[i].NumStateChanges;
                animation.StateChangeOffset = wad.Animations[i].ChangesIndex;
                animation.StateID = wad.Animations[i].StateId;
                animation.Speed = wad.Animations[i].Speed;
                animation.Accel = wad.Animations[i].Accel;
                animation.SpeedLateral = wad.Animations[i].SpeedLateral;
                animation.AccelLateral = wad.Animations[i].AccelLateral;

                _animations[i] = animation;
            }

            ReportProgress(13, "    Number of state changes: " + wad.Changes.Count);

            _numStateChanges = (uint)wad.Changes.Count;
            _stateChanges = new tr_state_change[wad.Changes.Count];
            for (int i = 0; i < _numStateChanges; i++)
            {
                tr_state_change state = new tr_state_change();

                state.AnimDispatch = wad.Changes[i].DispatchesIndex;
                state.NumAnimDispatches = wad.Changes[i].NumDispatches;
                state.StateID = wad.Changes[i].StateId;

                _stateChanges[i] = state;
            }

            ReportProgress(14, "    Number of animation dispatches: " + wad.Dispatches.Count);

            _numAnimDispatches = (uint)wad.Dispatches.Count;
            _animDispatches = new tr_anim_dispatch[wad.Dispatches.Count];
            for (int i = 0; i < _numAnimDispatches; i++)
            {
                tr_anim_dispatch disp = new tr_anim_dispatch();

                disp.High = wad.Dispatches[i].High;
                disp.Low = wad.Dispatches[i].Low;
                disp.NextAnimation = wad.Dispatches[i].NextAnimation;
                disp.NextFrame = wad.Dispatches[i].NextFrame;

                _animDispatches[i] = disp;
            }

            ReportProgress(15, "    Number of animation commands: " + wad.Commands.Count);

            _numAnimCommands = (uint)wad.Commands.Count;
            _animCommands = new short[wad.Commands.Count];
            for (int i = 0; i < _numAnimCommands; i++)
            {
                _animCommands[i] = wad.Commands[i];
            }

            _numMeshTrees = (uint)wad.Links.Count;
            _meshTrees = new int[wad.Links.Count];
            for (int i = 0; i < _numMeshTrees; i++)
            {
                _meshTrees[i] = wad.Links[i];
            }

            ReportProgress(16, "    Number of keyframes: " + wad.KeyFrames.Count);

            _numFrames = (uint)wad.KeyFrames.Count;
            _frames = new short[wad.KeyFrames.Count];
            for (int i = 0; i < _numFrames; i++)
            {
                _frames[i] = wad.KeyFrames[i];
            }

            _numMeshPointers = (uint)wad.Pointers.Count;
            _meshPointers = new uint[wad.Pointers.Count];
            for (int i = 0; i < _numMeshPointers; i++)
            {
                _meshPointers[i] = wad.Pointers[i];
            }

            ReportProgress(17, "    Number of moveables: " + wad.Moveables.Count);

            _numMoveables = (uint)wad.Moveables.Count;
            _moveables = new tr_moveable[wad.Moveables.Count];
            for (int i = 0; i < _numMoveables; i++)
            {
                tr_moveable mov = new tr_moveable();

                mov.Animation = (ushort)wad.Moveables[i].AnimationIndex;
                mov.FrameOffset = wad.Moveables[i].KeyFrameOffset;
                mov.MeshTree = wad.Moveables[i].LinksIndex;
                mov.NumMeshes = wad.Moveables[i].NumPointers;
                mov.ObjectID = wad.Moveables[i].ObjectID;
                mov.StartingMesh = wad.Moveables[i].PointerIndex;

                _moveables[i] = mov;
            }

            ReportProgress(18, "    Number of static meshes: " + wad.StaticMeshes.Count);

            _numStaticMeshes = (uint)wad.StaticMeshes.Count;
            _staticMeshes = new tr_staticmesh[wad.StaticMeshes.Count];
            for (int i = 0; i < _numStaticMeshes; i++)
            {
                tr_staticmesh sm = new tr_staticmesh();

                tr_bounding_box visibility = new tr_bounding_box();
                tr_bounding_box collision = new tr_bounding_box();

                collision.X1 = wad.StaticMeshes[i].CollisionX1;
                collision.Y1 = wad.StaticMeshes[i].CollisionY1;
                collision.Z1 = wad.StaticMeshes[i].CollisionZ1;

                collision.X2 = wad.StaticMeshes[i].CollisionX2;
                collision.Y2 = wad.StaticMeshes[i].CollisionY2;
                collision.Z2 = wad.StaticMeshes[i].CollisionZ2;

                visibility.X1 = wad.StaticMeshes[i].VisibilityX1;
                visibility.Y1 = wad.StaticMeshes[i].VisibilityY1;
                visibility.Z1 = wad.StaticMeshes[i].VisibilityZ1;
                visibility.X2 = wad.StaticMeshes[i].VisibilityX2;
                visibility.Y2 = wad.StaticMeshes[i].VisibilityY2;
                visibility.Z2 = wad.StaticMeshes[i].VisibilityZ2;

                sm.CollisionBox = collision;
                sm.VisibilityBox = visibility;
                sm.Flags = wad.StaticMeshes[i].Flags;
                sm.Mesh = wad.StaticMeshes[i].PointersIndex;
                sm.ObjectID = wad.StaticMeshes[i].ObjectId;

                _staticMeshes[i] = sm;
            }
        }

        private short BuildTextureInfo(BlockFace face)
        {
            tr_object_texture tile = new tr_object_texture();
            LevelTexture tex = _tempTexturesArray[_texturesIdTable[face.Texture]];

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
                    if (face.TextureTriangle == TextureTileType.TriangleNW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x00;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleNE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth - 1);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x01;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleSE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x02;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleSW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x03;
                    }
                }
                else
                {
                    if (face.TextureTriangle == TextureTileType.TriangleNW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight - 1);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x04;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleNE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x05;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleSE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x06;
                    }
                    else if (face.TextureTriangle == TextureTileType.TriangleSW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x07;
                    }
                }

                tile.Vertices[3] = new tr_object_texture_vert();
                tile.Vertices[3].Xcoordinate = 0;
                tile.Vertices[3].Xpixel = 0;
                tile.Vertices[3].Ycoordinate = 0;
                tile.Vertices[3].Ypixel = 0;
            }
            else
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!face.Flipped)
                {
                    tile.Vertices[0] = new tr_object_texture_vert();
                    tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[0].Xpixel = 0;
                    tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[0].Ypixel = 0;

                    tile.Vertices[1] = new tr_object_texture_vert();
                    tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[1].Xpixel = 255;
                    tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[1].Ypixel = 0;

                    tile.Vertices[2] = new tr_object_texture_vert();
                    tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[2].Xpixel = 255;
                    tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[2].Ypixel = 255;

                    tile.Vertices[3] = new tr_object_texture_vert();
                    tile.Vertices[3].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[3].Xpixel = 0;
                    tile.Vertices[3].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[3].Ypixel = 255;
                }
                else
                {
                    tile.Vertices[0] = new tr_object_texture_vert();
                    tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[0].Xpixel = 255;
                    tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[0].Ypixel = 0;

                    tile.Vertices[1] = new tr_object_texture_vert();
                    tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[1].Xpixel = 0;
                    tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[1].Ypixel = 0;

                    tile.Vertices[2] = new tr_object_texture_vert();
                    tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[2].Xpixel = 0;
                    tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[2].Ypixel = 255;

                    tile.Vertices[3] = new tr_object_texture_vert();
                    tile.Vertices[3].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[3].Xpixel = 255;
                    tile.Vertices[3].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[3].Ypixel = 255;

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
                if ((tex.Width == 64 && tex.Height == 64) || (tex.Width == 32 && tex.Height == 32))
                {
                    int animatedSet = -1;
                    int animatedTextureTile = -1;

                    for (int i = 0; i < _level.AnimatedTextures.Count; i++)
                    {
                        for (int j = 0; j < _level.AnimatedTextures[i].Textures.Count; j++)
                        {
                            AnimatedTexture current = _level.AnimatedTextures[i].Textures[j];

                            if (current.X == tex.X && current.Y == tex.Y && current.Page == tex.Page)
                            {
                                animatedSet = i;
                                animatedTextureTile = j;

                                break;
                            }
                        }
                    }

                    if (animatedSet != -1)
                    {
                        if (!_animTexturesRooms.Contains(newId & 0x7fff))
                            _animTexturesRooms.Add(newId & 0x7fff);

                        // Search for a compatible variant
                        int foundVariant = -1;

                        for (int i = 0; i < _level.AnimatedTextures[animatedSet].Variants.Count; i++)
                        {
                            AnimatedTextureSequenceVariant variant = _level.AnimatedTextures[animatedSet].Variants[i];
                            bool isTriangle = face.Shape == BlockFaceShape.Triangle;

                            if (variant.Flipped == face.Flipped && variant.Size == tex.Width && variant.Transparent == face.Transparent &&
                                ((variant.IsTriangle == true && isTriangle && variant.Triangle == face.TextureTriangle) ||
                                 (variant.IsTriangle == false && !isTriangle)))
                            {
                                foundVariant = i;
                                break;
                            }
                        }

                        if (foundVariant == -1)
                        {
                            AnimatedTextureSequenceVariant newVariant = new AnimatedTextureSequenceVariant();
                            newVariant.Size = tex.Width;
                            newVariant.Flipped = face.Flipped;
                            newVariant.Triangle = face.TextureTriangle;
                            newVariant.IsTriangle = face.Shape == BlockFaceShape.Triangle;
                            newVariant.Transparent = face.Transparent;

                            for (int j = 0; j < _level.AnimatedTextures[animatedSet].Textures.Count; j++)
                            {
                                AnimatedTextureVariantTile aTile = new AnimatedTextureVariantTile(j, -1);
                                newVariant.Tiles.Add(aTile);
                            }

                            _level.AnimatedTextures[animatedSet].Variants.Add(newVariant);

                            foundVariant = _level.AnimatedTextures[animatedSet].Variants.Count - 1;
                        }

                        if (_level.AnimatedTextures[animatedSet].Variants[foundVariant].Tiles[animatedTextureTile].NewID == -1)
                        {
                            _level.AnimatedTextures[animatedSet].Variants[foundVariant].Tiles[animatedTextureTile].NewID = (short)(newId & 0x7fff);
                        }
                    }
                }

                return (short)newId;
            }
            else
            {
                if (face.DoubleSided)
                    test = test | 0x8000;
                return (short)test;
            }
        }

        private short BuildAnimatedTextureInfo(AnimatedTextureSequenceVariant aSet, AnimatedTextureVariantTile aTile,
                                               LevelTexture tex)
        {
            tr_object_texture tile = new tr_object_texture();

            // Texture page
            tile.Tile = (ushort)tex.NewPage;
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

            tile.Xsize = (uint)tex.Width - 1;
            tile.Ysize = (uint)tex.Height - 1;

            int tmpWidth = tex.Width - 1;
            int tmpHeight = tex.Height - 1;

            // Texture UV
            if (aSet.IsTriangle)
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!aSet.Flipped)
                {
                    if (aSet.Triangle == TextureTileType.TriangleNW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x00;
                    }
                    else if (aSet.Triangle == TextureTileType.TriangleNE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth - 1);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x01;
                    }
                    else if (aSet.Triangle == TextureTileType.TriangleSE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x02;
                    }
                    else if (aSet.Triangle == TextureTileType.TriangleSW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x03;
                    }
                }
                else
                {
                    if (aSet.Triangle == TextureTileType.TriangleNW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight - 1);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x04;
                    }
                    else if (aSet.Triangle == TextureTileType.TriangleNE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x05;
                    }
                    else if (aSet.Triangle == TextureTileType.TriangleSE)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags |= 0x06;
                    }
                    else if (aSet.Triangle == TextureTileType.TriangleSW)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags |= 0x07;
                    }
                }

                tile.Vertices[3] = new tr_object_texture_vert();
                tile.Vertices[3].Xcoordinate = 0;
                tile.Vertices[3].Xpixel = 0;
                tile.Vertices[3].Ycoordinate = 0;
                tile.Vertices[3].Ypixel = 0;
            }
            else
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!aSet.Flipped)
                {
                    tile.Vertices[0] = new tr_object_texture_vert();
                    tile.Vertices[0].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[0].Xpixel = 0;
                    tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[0].Ypixel = 0;

                    tile.Vertices[1] = new tr_object_texture_vert();
                    tile.Vertices[1].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[1].Xpixel = 255;
                    tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[1].Ypixel = 0;

                    tile.Vertices[2] = new tr_object_texture_vert();
                    tile.Vertices[2].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[2].Xpixel = 255;
                    tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[2].Ypixel = 255;

                    tile.Vertices[3] = new tr_object_texture_vert();
                    tile.Vertices[3].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[3].Xpixel = 0;
                    tile.Vertices[3].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[3].Ypixel = 255;
                }
                else
                {
                    tile.Vertices[0] = new tr_object_texture_vert();
                    tile.Vertices[0].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[0].Xpixel = 255;
                    tile.Vertices[0].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[0].Ypixel = 0;

                    tile.Vertices[1] = new tr_object_texture_vert();
                    tile.Vertices[1].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[1].Xpixel = 0;
                    tile.Vertices[1].Ycoordinate = (byte)tex.NewY;
                    tile.Vertices[1].Ypixel = 0;

                    tile.Vertices[2] = new tr_object_texture_vert();
                    tile.Vertices[2].Xcoordinate = (byte)tex.NewX;
                    tile.Vertices[2].Xpixel = 0;
                    tile.Vertices[2].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[2].Ypixel = 255;

                    tile.Vertices[3] = new tr_object_texture_vert();
                    tile.Vertices[3].Xcoordinate = (byte)(tex.NewX + tmpWidth);
                    tile.Vertices[3].Xpixel = 255;
                    tile.Vertices[3].Ycoordinate = (byte)(tex.NewY + tmpHeight);
                    tile.Vertices[3].Ypixel = 255;

                    tile.Flags |= 0x01;
                }
            }

            int test = TextureInfoExists(tile);
            if (test == -1)
            {
                _tempObjectTextures.Add(tile);
                int newId = _tempObjectTextures.Count - 1;

                return (short)newId;
            }
            else
            {
                return (short)test;
            }
        }

        private tr_room_sector GetSector(int room, int x, int z)
        {
            return _rooms[room].Sectors[_rooms[room].NumZSectors * x + z];
        }

        private void SaveSector(int room, int x, int z, tr_room_sector sector)
        {
            _rooms[room].Sectors[_rooms[room].NumZSectors * x + z] = sector;
        }

        private bool ValueInRange(int value, int min, int max)
        {
            return (value >= min) && (value <= max);
        }

        private void GetAllReachableRooms()
        {
            for (int i = 0; i < _level.Rooms.Length; i++)
            {
                if (_level.Rooms[i] == null)
                    continue;

                _level.Rooms[i].Visited = false;
                _rooms[_roomsIdTable[i]].ReachableRooms = new List<int>();
            }

            for (int i = 0; i < _level.Rooms.Length; i++)
            {
                if (_level.Rooms[i] == null)
                    continue;

                GetAllReachableRoomsUp(i, i);
                GetAllReachableRoomsDown(i, i);
            }
        }

        private void GetAllReachableRoomsUp(int baseRoom, int currentRoom)
        {
            Room room = _level.Rooms[currentRoom];

            _level.Rooms[currentRoom].Visited = true;

            // Wall portals
            for (int i = 0; i < _level.Portals.Count; i++)
            {
                Portal p = _level.Portals.ElementAt(i).Value;
                if (p.Room == currentRoom)
                {
                    if (p.Direction != PortalDirection.Floor && p.Direction != PortalDirection.Ceiling)
                    {
                        if (!_rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                            _rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
                    }
                }
            }

            // Ceiling portals
            for (int i = 0; i < _level.Portals.Count; i++)
            {
                Portal p = _level.Portals.ElementAt(i).Value;
                if (p.Room == currentRoom)
                {
                    if (p.Direction == PortalDirection.Ceiling)
                    {
                        if (!_rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                        {
                            _rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
                            GetAllReachableRoomsUp(baseRoom, p.AdjoiningRoom);
                        }
                    }
                }
            }
        }

        private void GetAllReachableRoomsDown(int baseRoom, int currentRoom)
        {
            Room room = _level.Rooms[currentRoom];
            //if (room.Visited) return;

            _level.Rooms[currentRoom].Visited = true;

            // portali laterali
            for (int i = 0; i < _level.Portals.Count; i++)
            {
                Portal p = _level.Portals.ElementAt(i).Value;
                if (p.Room == currentRoom)
                {
                    if (p.Direction != PortalDirection.Floor && p.Direction != PortalDirection.Ceiling)
                    {
                        if (!_rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                            _rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
                    }
                }
            }

            for (int i = 0; i < _level.Portals.Count; i++)
            {
                Portal p = _level.Portals.ElementAt(i).Value;
                if (p.Room == currentRoom)
                {
                    if (p.Direction == PortalDirection.Floor)
                    {
                        if (!_rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                        {
                            _rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
                            GetAllReachableRoomsDown(baseRoom, p.AdjoiningRoom);
                        }
                    }
                }
            }
        }

        private bool BuildBox(int i, int x, int z, int xm, int xM, int zm, int zM, out tr_box_aux box)
        {
            tr_room room = _rooms[i];
            tr_room_sector sector = GetSector(i, x, z);
            tr_sector_aux aux = room.AuxSectors[x, z];

            int xMin = 0;
            int xMax = 0;
            int zMin = 0;
            int zMax = 255;

            int xc = x;
            int zc = z;

            // Find box corners in direction -X
            for (int x2 = xc; x2 > 0; x2--)
            {
                tr_sector_aux aux2 = room.AuxSectors[x2, zc];
                tr_room_sector sector2 = GetSector(i, x2, zc);

                if (aux2.WallPortal != -1)
                {
                    xMin = x2;
                    break;
                }

                if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope))
                    break;
                xMin = x2;
            }

            // Find box corners in direction +X
            for (int x2 = xc; x2 < room.NumXSectors - 1; x2++)
            {
                tr_sector_aux aux2 = room.AuxSectors[x2, zc];
                tr_room_sector sector2 = GetSector(i, x2, zc);

                if (aux2.WallPortal != -1)
                {
                    xMax = x2;
                    break;
                }

                if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope))
                    break;
                xMax = x2;
            }

            // Find box corners in direction -Z
            for (int x2 = xMin; x2 <= xMax; x2++)
            {
                int tmpZ = 0;
                for (int z2 = zc; z2 > 0; z2--)
                {
                    tr_sector_aux aux2 = room.AuxSectors[x2, z2];
                    tr_room_sector sector2 = GetSector(i, x2, z2);

                    if (aux2.WallPortal != -1)
                    {
                        tmpZ = z2;
                        break;
                    }

                    if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope))
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
                    tr_sector_aux aux2 = room.AuxSectors[x2, z2];
                    tr_room_sector sector2 = GetSector(i, x2, z2);

                    if (aux2.WallPortal != -1)
                    {
                        tmpZ = z2;
                        break;
                    }

                    if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope))
                        break;

                    tmpZ = z2;
                }

                if (tmpZ < zMax)
                    zMax = tmpZ;
            }

            box = new tr_box_aux();
            box.Xmin = (byte)(xMin + room.Info.X / 1024);
            box.Xmax = (byte)(xMax + room.Info.X / 1024 + 1);
            box.Zmin = (byte)(zMin + room.Info.Z / 1024);
            box.Zmax = (byte)(zMax + room.Info.Z / 1024 + 1);
            box.TrueFloor = (short)(GetMostDownFloor(i, x, z));  //(short)GetBoxFloorHeight(i, x, z); //
            box.IsolatedBox = aux.Box;
            box.Monkey = aux.Monkey;
            box.Portal = aux.Portal;
            box.Room = (short)i;

            // Cut the box if needed
            if (xm != 0 && zm != 0 && xM != 0 && zM != 0)
            {
                if (box.Xmin < xm)
                    box.Xmin = (byte)xm;
                if (box.Xmax > xM)
                    box.Xmax = (byte)xM;
                if (box.Zmin < zm)
                    box.Zmin = (byte)zm;
                if (box.Zmax > zM)
                    box.Zmax = (byte)zM;

                if (box.Xmax - box.Xmin <= 0)
                    return false;
                if (box.Zmax - box.Zmin <= 0)
                    return false;
            }

            return true;
        }

        private short GetMostDownBox(int room, int x, int z)
        {
            tr_room_sector sector = GetSector(room, x, z);
            if (sector.RoomBelow == 255)
                return -1;

            tr_room room1 = _rooms[room];
            tr_room room2 = _rooms[sector.RoomBelow];

            int x2 = room1.Info.X / 1024 + x - room2.Info.X * 1024;
            int z2 = room1.Info.Z / 1024 + z - room2.Info.Z * 1024;

            tr_room_sector sector2 = GetSector(sector.RoomBelow, x2, z2);

            short result = 0;

            if (sector2.RoomBelow == 255)
                result = (short)(sector2.BoxIndex >> 4);
            else
                result = GetMostDownBox(sector.RoomBelow, x2, z2);

            return result;
        }

        private short GetMostDownFloor(int room, int x, int z)
        {
            tr_room_sector sector = GetSector(room, x, z);
            if (sector.RoomBelow == 255)
            {
                tr_sector_aux aux3 = _rooms[room].AuxSectors[x, z];
                return (short)(aux3.LowestFloor * 256);
            }

            tr_room room1 = _rooms[room];
            tr_room room2 = _rooms[sector.RoomBelow];

            int x2 = room1.Info.X / 1024 + x - room2.Info.X / 1024;
            int z2 = room1.Info.Z / 1024 + z - room2.Info.Z / 1024;

            tr_room_sector sector2 = GetSector(sector.RoomBelow, x2, z2);
            tr_sector_aux aux2 = _rooms[sector.RoomBelow].AuxSectors[x2, z2];

            //short result = 0;

            /*while (true)
            {
                if (sector2.RoomBelow == 255 || aux2.IsFloorSolid)
                {
                    result = (short)(aux2.LowestFloor * 256);
                    break;
                }
                else
                {
                    room = sector.RoomBelow;

                    sector = GetSector(room, x, z);
                    if (sector.RoomBelow == 255)
                    {
                        tr_sector_aux aux3 = Rooms[room].AuxSectors[x, z];
                        return (short)(aux3.LowestFloor * 256);
                    }

                    room1 = Rooms[room];
                    room2 = Rooms[sector.RoomBelow];

                    x2 = room1.Info.X / 1024 + x - room2.Info.X / 1024;
                    z2 = room1.Info.Z / 1024 + z - room2.Info.Z / 1024;

                    sector2 = GetSector(sector.RoomBelow, x2, z2);
                    aux2 = Rooms[sector.RoomBelow].AuxSectors[x2, z2];

                  //  result = GetMostDownFloor(sector.RoomBelow, x2, z2);
                }
            }*/

            short result = 0;

            if (sector2.RoomBelow == 255 || aux2.IsFloorSolid)
            {
                result = (short)(aux2.LowestFloor * 256);
            }
            else
            {
                result = GetMostDownFloor(sector.RoomBelow, x2, z2);
            }

            return result;
        }

        private bool GetMostDownFloorAndRoom(int room, int x, int z, out int roomIndex, out short floor)
        {
            tr_room_sector sector = GetSector(room, x, z);
            if (sector.RoomBelow == 255)
            {
                roomIndex = room;
                floor = sector.Floor;
                return true;
            }

            tr_room room1 = _rooms[room];
            tr_room room2 = _rooms[sector.RoomBelow];

            int x2 = room1.Info.X / 1024 + x - room2.Info.X / 1024;
            int z2 = room1.Info.Z / 1024 + z - room2.Info.Z / 1024;

            tr_room_sector sector2 = GetSector(sector.RoomBelow, x2, z2);

            bool result = false;

            if (sector2.RoomBelow == 255)
            {
                roomIndex = sector.RoomBelow;
                floor = sector2.Floor;
                result = true;
            }
            else
            {
                result = GetMostDownFloorAndRoom(sector.RoomBelow, x2, z2, out roomIndex, out floor);
            }

            return result;
        }

        private bool FindMonkeyFloor(int room, int x, int z)
        {
            tr_room_sector sector = GetSector(room, x, z);
            if (sector.RoomBelow == 255)
            {
                return _rooms[room].AuxSectors[x, z].Monkey;
            }

            tr_room room1 = _rooms[room];
            tr_room room2 = _rooms[sector.RoomBelow];

            int x2 = room1.Info.X / 1024 + x - room2.Info.X / 1024;
            int z2 = room1.Info.Z / 1024 + z - room2.Info.Z / 1024;

            tr_room_sector sector2 = GetSector(sector.RoomBelow, x2, z2);

            bool result = false;

            if (sector2.RoomBelow == 255)
            {
                result = _rooms[sector.RoomBelow].AuxSectors[x2, z2].Monkey;
            }
            else
            {
                result = FindMonkeyFloor(sector.RoomBelow, x2, z2);
            }

            return result;
        }

        public void ConvertWadMeshes()
        {
            ReportProgress(11, "Converting WAD meshes to TR4 format");

            TR4Wad wad = _editor.Level.Wad.OriginalWad;

            ReportProgress(12, "    Number of meshes: " + wad.Meshes.Count);

            List<tr_mesh> tempMeshes = new List<tr_mesh>();
            List<uint> tempMeshesPointers = new List<uint>();

            int totalMeshSize = 0;

            for (int i = 0; i < wad.Meshes.Count; i++)
            {
                int meshSize = 0;

                TR4Wad.wad_mesh oldMesh = wad.Meshes[i];
                tr_mesh newMesh = new tr_mesh();

                newMesh.Centre = new tr_vertex();
                newMesh.Centre.X = oldMesh.SphereX;
                newMesh.Centre.Y = oldMesh.SphereY;
                newMesh.Centre.Z = oldMesh.SphereZ;

                newMesh.Radius = oldMesh.Radius;

                meshSize += 10;

                newMesh.NumNormals = oldMesh.NumNormals;
                if (newMesh.NumNormals > 0)
                {
                    newMesh.Normals = new tr_vertex[newMesh.NumNormals];
                    for (int k = 0; k < newMesh.NumNormals; k++)
                    {
                        tr_vertex n = new tr_vertex();

                        n.X = oldMesh.Normals[k].X;
                        n.Y = oldMesh.Normals[k].Y;
                        n.Z = oldMesh.Normals[k].Z;

                        newMesh.Normals[k] = n;
                    }

                    meshSize += 2 + newMesh.NumNormals * 6;
                }
                else
                {
                    newMesh.Lights = new short[-newMesh.NumNormals];
                    for (int k = 0; k < -newMesh.NumNormals; k++)
                    {
                        newMesh.Lights[k] = oldMesh.Shades[k];
                    }

                    meshSize += 2 - newMesh.NumNormals * 2;
                }

                newMesh.NumVertices = (short)oldMesh.NumVertices;
                newMesh.Vertices = new tr_vertex[newMesh.NumVertices];

                for (int j = 0; j < newMesh.NumVertices; j++)
                {
                    tr_vertex v = new tr_vertex();

                    v.X = oldMesh.Vertices[j].X;
                    v.Y = oldMesh.Vertices[j].Y;
                    v.Z = oldMesh.Vertices[j].Z;

                    newMesh.Vertices[j] = v;
                }

                meshSize += 2 + newMesh.NumVertices * 6;

                List<tr_face4> tempRectangles = new List<tr_face4>();
                List<tr_face3> tempTriangles = new List<tr_face3>();

                for (int j = 0; j < oldMesh.NumPolygons; j++)
                {
                    TR4Wad.wad_polygon poly = oldMesh.Polygons[j];

                    if (poly.Shape == 9)
                    {
                        newMesh.NumTexturedRectangles++;

                        tr_face4 rectangle = new tr_face4();

                        rectangle.Vertices = new ushort[4];
                        rectangle.Vertices[0] = poly.V1;
                        rectangle.Vertices[1] = poly.V2;
                        rectangle.Vertices[2] = poly.V3;
                        rectangle.Vertices[3] = poly.V4;

                        rectangle.Texture = BuildWadTextureInfo((short)poly.Texture, false, poly.Attributes);

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

                        tr_face3 triangle = new tr_face3();

                        triangle.Vertices = new ushort[3];
                        triangle.Vertices[0] = poly.V1;
                        triangle.Vertices[1] = poly.V2;
                        triangle.Vertices[2] = poly.V3;

                        triangle.Texture = BuildWadTextureInfo((short)poly.Texture, true, poly.Attributes);

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

                meshSize += 2 + newMesh.NumTexturedRectangles * 12;
                meshSize += 2 + newMesh.NumTexturedTriangles * 10;

                if (meshSize % 4 != 0)
                    meshSize += 2;

                tempMeshesPointers.Add((uint)totalMeshSize);
                totalMeshSize += meshSize;

                tempMeshes.Add(newMesh);
            }

            _numMeshes = (uint)tempMeshes.Count;
            _meshes = tempMeshes.ToArray();
        }

        private void PrepareItems()
        {
            ReportProgress(18, "Building items table");

            _moveablesTable = new Dictionary<int, int>();
            _aiObjectsTable = new Dictionary<int, int>();

            int k = 0;
            int n = 0;

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                if (_editor.Level.Objects.ElementAt(i).Value.Type == ObjectInstanceType.Moveable)
                {
                    uint objectId = ((MoveableInstance)_editor.Level.Objects.ElementAt(i).Value).Model.ObjectID;

                    if (objectId >= 398 && objectId <= 406)
                    {
                        _aiObjectsTable.Add(_editor.Level.Objects.ElementAt(i).Key, n);
                        n++;
                    }
                    else
                    {
                        _moveablesTable.Add(_editor.Level.Objects.ElementAt(i).Key, k);
                        k++;
                    }
                }
            }

            List<tr_item> tempItems = new List<tr_item>();
            List<tr_ai_item> tempAiObjects = new List<tr_ai_item>();

            for (int i = 0; i < _moveablesTable.Count; i++)
            {
                //  if (i == 79) continue;
                tr_item item = new tr_item();
                MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[_moveablesTable.ElementAt(i).Key];
                tr_room newRoom = _rooms[_roomsIdTable[instance.Room]];

                item.X = (int)(_rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                item.Y = (int)(_rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                item.Z = (int)(_rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                item.ObjectID = (short)instance.Model.ObjectID;
                item.Room = (short)_roomsIdTable[instance.Room];
                item.Angle = (short)(instance.Rotation / 45 * 8192);
                item.Intensity1 = -1;
                item.Intensity2 = (short)instance.OCB;

                if (instance.ClearBody)
                    item.Flags |= 0x80;
                if (instance.Invisible)
                    item.Flags |= 0x100;

                ushort mask = 0;

                if (instance.Bits[0])
                    mask |= 0x01;
                if (instance.Bits[1])
                    mask |= 0x02;
                if (instance.Bits[2])
                    mask |= 0x04;
                if (instance.Bits[3])
                    mask |= 0x08;
                if (instance.Bits[4])
                    mask |= 0x10;

                item.Flags |= (ushort)(mask << 9);

                tempItems.Add(item);
            }

            _numItems = (uint)tempItems.Count;
            _items = tempItems.ToArray();

            for (int i = 0; i < _aiObjectsTable.Count; i++)
            {
                tr_ai_item item = new tr_ai_item();
                MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[_aiObjectsTable.ElementAt(i).Key];
                tr_room newRoom = _rooms[_roomsIdTable[instance.Room]];

                item.X = (int)(_rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                item.Y = (int)(_rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                item.Z = (int)(_rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                item.ObjectID = (ushort)instance.Model.ObjectID;
                item.Room = (ushort)_roomsIdTable[instance.Room];
                short angle = (short)(instance.Rotation);
                item.Angle = (short)(angle / 45 * 8192);
                item.OCB = (ushort)instance.OCB;

                ushort mask = 0;

                if (instance.Bits[0])
                    mask |= 0x01;
                if (instance.Bits[1])
                    mask |= 0x02;
                if (instance.Bits[2])
                    mask |= 0x04;
                if (instance.Bits[3])
                    mask |= 0x08;
                if (instance.Bits[4])
                    mask |= 0x10;

                item.Flags |= (ushort)(mask << 1);

                tempAiObjects.Add(item);
            }

            _numAiItems = (uint)tempAiObjects.Count;
            _aiItems = tempAiObjects.ToArray();

            ReportProgress(30, "    Number of items: " + _numItems);
            ReportProgress(31, "    Number of AI objects: " + _numAiItems);
        }

        private int TextureInfoExists(tr_object_texture txt)
        {
            for (int i = 0; i < _tempObjectTextures.Count; i++)
            {
                tr_object_texture txt2 = _tempObjectTextures[i];

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
            bool isFlipped;
            int shape = 0;
            int original = txt;
            TR4Wad wad = _editor.Level.Wad.OriginalWad;

            isFlipped = false;
            shape = (int)original & 0x7000;

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
                txt = (short)-txt;

            tr_object_texture tile = new tr_object_texture();
            TR4Wad.wad_object_texture tex = wad.Textures[txt];

            // Texture page
            tile.Tile = (ushort)(tex.Page + _numRoomTexturePages);
            if (triangle)
                tile.Tile |= 0x8000;

            // Attributes
            tile.Attributes = 0;

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

            tile.Xsize = (uint)tex.Width;
            tile.Ysize = (uint)tex.Height;

            // Texture UV
            if (triangle)
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!isFlipped)
                {
                    if (shape == 0x00)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.X;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)tex.Y;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)tex.Y;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.X;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags = 0;
                    }
                    else if (shape == 0x2000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)tex.Y;
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)tex.X;
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)tex.Y;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags = 1;
                    }
                    else if (shape == 0x4000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.X;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)tex.Y;
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags = 2;
                    }
                    else if (shape == 0x6000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)tex.X;
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)tex.X;
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)tex.Y;
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags = 3;
                    }
                }
                else
                {
                    if (shape == 0x00)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.Y);
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.X);
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.Y);
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags = 4;
                    }
                    else if (shape == 0x2000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.X);
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.Y);
                        tile.Vertices[0].Ypixel = 0;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.X);
                        tile.Vertices[1].Xpixel = 0;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[2].Xpixel = 255;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.Y);
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags = 5;
                    }
                    else if (shape == 0x4000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.X);
                        tile.Vertices[0].Xpixel = 0;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[1].Ypixel = 255;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.X);
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.Y);
                        tile.Vertices[2].Ypixel = 0;

                        tile.Flags = 6;
                    }
                    else if (shape == 0x6000)
                    {
                        tile.Vertices[0] = new tr_object_texture_vert();
                        tile.Vertices[0].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[0].Xpixel = 255;
                        tile.Vertices[0].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[0].Ypixel = 255;

                        tile.Vertices[1] = new tr_object_texture_vert();
                        tile.Vertices[1].Xcoordinate = (byte)(tex.X + tex.Width);
                        tile.Vertices[1].Xpixel = 255;
                        tile.Vertices[1].Ycoordinate = (byte)(tex.Y);
                        tile.Vertices[1].Ypixel = 0;

                        tile.Vertices[2] = new tr_object_texture_vert();
                        tile.Vertices[2].Xcoordinate = (byte)(tex.X);
                        tile.Vertices[2].Xpixel = 0;
                        tile.Vertices[2].Ycoordinate = (byte)(tex.Y + tex.Height);
                        tile.Vertices[2].Ypixel = 255;

                        tile.Flags = 7;
                    }
                }

                tile.Vertices[3] = new tr_object_texture_vert();
                tile.Vertices[3].Xcoordinate = 255;
                tile.Vertices[3].Xpixel = 0;
                tile.Vertices[3].Ycoordinate = 255;
                tile.Vertices[3].Ypixel = 0;
            }
            else
            {
                tile.Vertices = new tr_object_texture_vert[4];

                if (!isFlipped)
                {
                    tile.Vertices[0] = new tr_object_texture_vert();
                    tile.Vertices[0].Xcoordinate = (byte)tex.X;
                    tile.Vertices[0].Xpixel = 0;
                    tile.Vertices[0].Ycoordinate = (byte)tex.Y;
                    tile.Vertices[0].Ypixel = 0;

                    tile.Vertices[1] = new tr_object_texture_vert();
                    tile.Vertices[1].Xcoordinate = (byte)(tex.X + tex.Width);
                    tile.Vertices[1].Xpixel = 255;
                    tile.Vertices[1].Ycoordinate = (byte)tex.Y;
                    tile.Vertices[1].Ypixel = 0;

                    tile.Vertices[2] = new tr_object_texture_vert();
                    tile.Vertices[2].Xcoordinate = (byte)(tex.X + tex.Width);
                    tile.Vertices[2].Xpixel = 255;
                    tile.Vertices[2].Ycoordinate = (byte)(tex.Y + tex.Height);
                    tile.Vertices[2].Ypixel = 255;

                    tile.Vertices[3] = new tr_object_texture_vert();
                    tile.Vertices[3].Xcoordinate = (byte)tex.X;
                    tile.Vertices[3].Xpixel = 0;
                    tile.Vertices[3].Ycoordinate = (byte)(tex.Y + tex.Height);
                    tile.Vertices[3].Ypixel = 255;

                    tile.Flags = 0;
                }
                else
                {
                    tile.Vertices[0] = new tr_object_texture_vert();
                    tile.Vertices[0].Xcoordinate = (byte)(tex.X + tex.Width);
                    tile.Vertices[0].Xpixel = 255;
                    tile.Vertices[0].Ycoordinate = (byte)(tex.Y);
                    tile.Vertices[0].Ypixel = 0;

                    tile.Vertices[1] = new tr_object_texture_vert();
                    tile.Vertices[1].Xcoordinate = (byte)(tex.X);
                    tile.Vertices[1].Xpixel = 0;
                    tile.Vertices[1].Ycoordinate = (byte)(tex.Y);
                    tile.Vertices[1].Ypixel = 0;

                    tile.Vertices[2] = new tr_object_texture_vert();
                    tile.Vertices[2].Xcoordinate = (byte)(tex.X);
                    tile.Vertices[2].Xpixel = 0;
                    tile.Vertices[2].Ycoordinate = (byte)(tex.Y + tex.Height);
                    tile.Vertices[2].Ypixel = 255;

                    tile.Vertices[3] = new tr_object_texture_vert();
                    tile.Vertices[3].Xcoordinate = (byte)(tex.X + tex.Width);
                    tile.Vertices[3].Xpixel = 255;
                    tile.Vertices[3].Ycoordinate = (byte)(tex.Y + tex.Height);
                    tile.Vertices[3].Ypixel = 255;

                    tile.Flags = 1;
                }
            }

            tile.Unknown1 = tex.X;
            tile.Unknown2 = tex.Y;

            int test = TextureInfoExists(tile);
            if (test == -1)
            {
                _tempObjectTextures.Add(tile);
                int newId = _tempObjectTextures.Count - 1;

                return (short)newId;
            }
            else
            {
                return (short)test;
            }
        }
    }
}
