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
    public class LevelCompilerTR4 : ILevelCompiler
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
                    if (x.Index == y.Index) return 0;
                    return (x.Index > y.Index ? 1 : -1);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_color
        {
            public byte Red;
            public byte Green;
            public byte Blue;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_color4
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte Alpha;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_vertex
        {
            public short X;
            public short Y;
            public short Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_face4
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] Vertices;
            public short Texture;
            public short LightingEffect;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_face3
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ushort[] Vertices;
            public short Texture;
            public short LightingEffect;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_room_info
        {
            public int X;
            public int Z;
            public int YBottom;
            public int YTop;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_room_portal
        {
            public ushort AdjoiningRoom;
            public tr_vertex Normal;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public tr_vertex[] Vertices;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_room_sector
        {
            public ushort FloorDataIndex;
            public short BoxIndex;
            public byte RoomBelow;
            public sbyte Floor;
            public byte RoomAbove;
            public sbyte Ceiling;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr4_room_light
        {
            public int X;
            public int Y;
            public int Z;
            public tr_color Color;
            public byte LightType;
            public ushort Intensity;
            public float In;
            public float Out;
            public float Length;
            public float CutOff;
            public float DirectionX;
            public float DirectionY;
            public float DirectionZ;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_room_vertex
        {
            public tr_vertex Vertex;
            public short Lighting1;
            public ushort Attributes;
            public short Lighting2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_room_staticmesh
        {
            public uint X;
            public uint Y;
            public uint Z;
            public ushort Rotation;
            public ushort Intensity1;
            public ushort Intensity2;
            public ushort ObjectID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_room
        {
            public tr_room_info Info;
            public uint NumDataWords;
            public ushort NumVertices;
            public tr_room_vertex[] Vertices;
            public ushort NumRectangles;
            public tr_face4[] Rectangles;
            public ushort NumTriangles;
            public tr_face3[] Triangles;
            public ushort NumSprites;
            public ushort NumPortals;
            public tr_room_portal[] Portals;
            public ushort NumZSectors;
            public ushort NumXSectors;
            public tr_room_sector[] Sectors;
            public ushort AmbientIntensity1;
            public ushort AmbientIntensity2;
            public short LightMode;
            public ushort NumLights;
            public tr4_room_light[] Lights;
            public ushort NumStaticMeshes;
            public tr_room_staticmesh[] StaticMeshes;
            public short AlternateRoom;
            public short Flags;
            public byte WaterScheme;
            public byte ReverbInfo;
            public byte AlternateGroup;

            // Helper data
            public tr_sector_aux[,] AuxSectors;
            public TextureSounds[,] TextureSounds;
            public List<int> ReachableRooms;
            public bool Visited;
            public bool Flipped;
            public short FlippedRoom;
            public short BaseRoom;
            public int OriginalRoomId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_mesh
        {
            public tr_vertex Centre;
            public int Radius;
            public short NumVertices;
            public tr_vertex[] Vertices;
            public short NumNormals;
            public tr_vertex[] Normals;
            public short[] Lights;
            public short NumTexturedRectangles;
            public tr_face4[] TexturedRectangles;
            public short NumTexturedTriangles;
            public tr_face3[] TexturedTriangles;
            public short NumColoredRectangles;
            public tr_face4[] ColoredRectangles;
            public short NumColoredTriangles;
            public tr_face3[] ColoredTriangles;
            public int MeshSize;
            public int MeshPointer;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_staticmesh
        {
            public uint ObjectID;
            public ushort Mesh;
            public tr_bounding_box VisibilityBox;
            public tr_bounding_box CollisionBox;
            public ushort Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_animated_textures_set
        {
            public short NumTextures;
            public short[] Textures;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_bounding_box
        {
            public short X1;
            public short X2;
            public short Y1;
            public short Y2;
            public short Z1;
            public short Z2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_moveable
        {
            public uint ObjectID;
            public ushort NumMeshes;
            public ushort StartingMesh;
            public uint MeshTree;
            public uint FrameOffset;
            public ushort Animation;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_item
        {
            public short ObjectID;
            public short Room;
            public int X;
            public int Y;
            public int Z;
            public short Angle;
            public short Intensity1;
            public short Intensity2;
            public ushort Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_sprite_texture
        {
            public ushort Tile;
            public byte X;
            public byte Y;
            public ushort Width;
            public ushort Height;
            public short LeftSide;
            public short TopSide;
            public short RightSide;
            public short BottomSide;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_sprite_sequence
        {
            public int ObjectID;
            public short NegativeLength;
            public short Offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_animation
        {
            public uint FrameOffset;
            public byte FrameRate;
            public byte FrameSize;
            public ushort StateID;
            public int Speed;
            public int Accel;
            public int SpeedLateral;
            public int AccelLateral;
            public ushort FrameStart;
            public ushort FrameEnd;
            public ushort NextAnimation;
            public ushort NextFrame;
            public ushort NumStateChanges;
            public ushort StateChangeOffset;
            public ushort NumAnimCommands;
            public ushort AnimCommand;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_state_change
        {
            public ushort StateID;
            public ushort NumAnimDispatches;
            public ushort AnimDispatch;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_anim_dispatch
        {
            public short Low;
            public short High;
            public short NextAnimation;
            public short NextFrame;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_box
        {
            public byte Zmin;
            public byte Zmax;
            public byte Xmin;
            public byte Xmax;
            public short TrueFloor;
            public short OverlapIndex;
        }

        public struct tr_box_aux
        {
            public byte Zmin;
            public byte Zmax;
            public byte Xmin;
            public byte Xmax;
            public short TrueFloor;
            public short Clicks;
            public byte RoomBelow;
            public short OverlapIndex;
            public int ZoneID;
            public bool Border;
            public byte NumXSectors;
            public byte NumZSectors;
            public bool IsolatedBox;
            public bool NotWalkableBox;
            public bool Monkey;
            public bool Jump;
            public short Room;
            public short AlternateRoom;
            public bool Portal;
            public bool FlipMap;
        }

        public struct tr_overlap_aux
        {
            public int MainBox;
            public int Box;
            public bool Skeleton;
            public bool Monkey;
            public bool EndOfList;
            public bool IsEdge;
        }

        public struct tr_sector_aux
        {
            public bool Wall;
            public bool SoftSlope;
            public bool HardSlope;
            public bool Monkey;
            public bool Box;
            public bool BorderWall;
            public bool Portal;
            public bool IsFloorSolid;
            public bool NotWalkableFloor;
            public int WallPortal;
            public int FloorPortal;
            public int CeilingPortal;
            public int BoxIndex;
            public short LowestFloor;
            public int RoomAbove;
            public int RoomBelow;
            public short MeanFloorHeight;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_zone
        {
            public ushort GroundZone1_Normal;
            public ushort GroundZone2_Normal;
            public ushort GroundZone3_Normal;
            public ushort GroundZone4_Normal;
            public ushort FlyZone_Normal;
            public ushort GroundZone1_Alternate;
            public ushort GroundZone2_Alternate;
            public ushort GroundZone3_Alternate;
            public ushort GroundZone4_Alternate;
            public ushort FlyZone_Alternate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_sound_source
        {
            public int X;
            public int Y;
            public int Z;
            public ushort SoundID;
            public ushort Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_sound_details
        {
            public short Sample;
            public short Volume;
            public short Unknown1;
            public short Unknown2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_object_texture_vert
        {
            public byte Xpixel;
            public byte Xcoordinate;
            public byte Ypixel;
            public byte Ycoordinate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_object_texture
        {
            public ushort Attributes;
            public ushort Tile;
            public ushort Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public tr_object_texture_vert[] Vertices;
            public uint Unknown1;
            public uint Unknown2;
            public uint Xsize;
            public uint Ysize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_camera
        {
            public int X;
            public int Y;
            public int Z;
            public short Room;
            public ushort Flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_animated_textures
        {
            public short NumTextureID;
            public short[] TextureID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr_ai_item
        {
            public ushort ObjectID;
            public ushort Room;
            public int X;
            public int Y;
            public int Z;
            public ushort OCB;
            public ushort Flags;
            public int Angle;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct tr4_flyby_camera
        {
            public int X;
            public int Y;
            public int Z;
            public int DirectionX;
            public int DirectionY;
            public int DirectionZ;

            public byte Sequence;
            public byte Index;

            public ushort FOV;
            public short Roll;
            public ushort Timer;
            public ushort Speed;
            public ushort Flags;

            public int Room;
        }

        public int Version;

        public ushort NumRoomTextureTiles;
        public ushort NumObjectTextureTiles;
        public ushort NumBumpTextureTiles;

        public uint Texture32UncompressedSize;
        public uint Texture32CompressedSize;
        public byte[] Texture32;

        public uint Texture16UncompressedSize;
        public uint Texture16CompressedSize;
        public byte[] Texture16;

        public uint MiscTextureUncompressedSize;
        public uint MiscTextureCompressedSize;
        public byte[] MiscTexture;

        public uint LevelUncompressedSize;
        public uint LevelCompressedSize;

        public int Unused;

        public ushort NumRooms;
        public tr_room[] Rooms;

        public uint NumFloorData;
        public ushort[] FloorData;

        public uint NumMeshData;
        public uint NumMeshes;
        public tr_mesh[] Meshes;

        public uint NumMeshPointers;
        public uint[] MeshPointers;
        public uint NumAnimations;
        public tr_animation[] Animations;
        public uint NumStateChanges;
        public tr_state_change[] StateChanges;
        public uint NumAnimDispatches;
        public tr_anim_dispatch[] AnimDispatches;
        public uint NumAnimCommands;
        public short[] AnimCommands;
        public uint NumMeshTrees;
        public int[] MeshTrees;
        public uint NumFrames;
        public short[] Frames;
        public uint NumMoveables;
        public tr_moveable[] Moveables;
        public uint NumStaticMeshes;
        public tr_staticmesh[] StaticMeshes;

        public byte[] SPR;

        public uint NumSpriteTextures;
        public tr_sprite_texture[] SpriteTextures;
        public uint NumSpriteSequences;
        public tr_sprite_sequence[] SpriteSequences;
        public uint NumCameras;
        public tr_camera[] Cameras;
        public uint NumFlyByCameras;
        public tr4_flyby_camera[] FlyByCameras;
        public uint NumSoundSources;
        public tr_sound_source[] SoundSources;
        public uint NumBoxes;
        public tr_box[] Boxes;
        public uint NumOverlaps;
        public ushort[] Overlaps;
        public tr_zone[] Zones;
        public uint NumAnimatedTextures;
        public tr_animated_textures_set[] AnimatedTextures;

        public byte[] TEX;

        public uint NumObjectTextures;
        public tr_object_texture[] ObjectTextures;
        public uint NumItems;
        public tr_item[] Items;
        public uint NumAiItems;
        public tr_ai_item[] AiItems;

        public short[] SoundMap;
        public uint NumSoundDetails;
        public tr_sound_details[] SoundDetails;

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

        private byte[] bufferSamples;

        private List<tr_box_aux> tempBoxes;

        public LevelCompilerTR4(Level level, string dest, BackgroundWorker bw = null) : base(level, dest, bw)
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

            bufferSamples = stream.ToArray();
        }

        private void CompileLevelTask3()
        {
            CopyWadData();
        }

        public override bool CompileLevel()
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

        private bool WriteLevelTR4()
        {
            long offset;
            long offset2;

            TR4Wad wad = _editor.Level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            BinaryWriterEx writer = new BinaryWriterEx(File.OpenWrite("temp.bin"));

            ReportProgress(85, "Writing geometry data to memory buffer");

            try
            {
                int filler = 0;
                writer.Write(filler);

                NumRooms = (ushort)Rooms.Length;
                writer.Write(NumRooms);

                for (int i = 0; i < NumRooms; i++)
                {
                    writer.WriteBlock(Rooms[i].Info);

                    offset = writer.BaseStream.Position;

                    int numdw = 0;
                    writer.Write(numdw);

                    ushort tmp = 0;
                    tmp = (ushort)Rooms[i].Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(Rooms[i].Vertices);

                    tmp = (ushort)Rooms[i].Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (int k = 0; k < Rooms[i].Rectangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Rectangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[3]);
                            writer.Write(Rooms[i].Rectangles[k].Texture);
                        }
                    }

                    tmp = (ushort)Rooms[i].Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (int k = 0; k < Rooms[i].Triangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Triangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Triangles[k].Texture);
                        }
                    }

                    // For sprites, not used
                    tmp = 0;
                    writer.Write(tmp);

                    // Now save current offset and calculate the size of the geometry
                    offset2 = writer.BaseStream.Position;
                    ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

                    // Save the size of the geometry
                    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                    writer.Write(roomGeometrySize);
                    writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                    // Write portals
                    tmp = (ushort)Rooms[i].Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0) writer.WriteBlockArray(Rooms[i].Portals);

                    // Write sectors
                    writer.Write(Rooms[i].NumZSectors);
                    writer.Write(Rooms[i].NumXSectors);
                    writer.WriteBlockArray(Rooms[i].Sectors);

                    // Write room color
                    writer.Write(Rooms[i].AmbientIntensity1);
                    writer.Write(Rooms[i].AmbientIntensity2);

                    // Write lights
                    tmp = (ushort)Rooms[i].Lights.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0) writer.WriteBlockArray(Rooms[i].Lights);

                    // Write static meshes
                    tmp = (ushort)Rooms[i].StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0) writer.WriteBlockArray(Rooms[i].StaticMeshes);

                    // Write final data
                    writer.Write(Rooms[i].AlternateRoom);
                    writer.Write(Rooms[i].Flags);
                    writer.Write(Rooms[i].WaterScheme);
                    writer.Write(Rooms[i].ReverbInfo);
                    writer.Write(Rooms[i].AlternateGroup);
                }

                // Write floordata
                NumFloorData = (uint)FloorData.Length;
                writer.Write(NumFloorData);
                writer.WriteBlockArray(FloorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                NumMeshData = 0;
                writer.Write(NumMeshData);
                int totalMeshSize = 0;

                for (int i = 0; i < Meshes.Length; i++)
                {
                    long meshOffset1 = writer.BaseStream.Position;

                    writer.WriteBlock(Meshes[i].Centre);
                    writer.Write(Meshes[i].Radius);

                    writer.Write(Meshes[i].NumVertices);
                    writer.WriteBlockArray(Meshes[i].Vertices);

                    writer.Write(Meshes[i].NumNormals);
                    if (Meshes[i].NumNormals > 0)
                    {
                        writer.WriteBlockArray(Meshes[i].Normals);
                    }
                    else
                    {
                        writer.WriteBlockArray(Meshes[i].Lights);
                    }

                    writer.Write(Meshes[i].NumTexturedRectangles);
                    writer.WriteBlockArray(Meshes[i].TexturedRectangles);

                    writer.Write(Meshes[i].NumTexturedTriangles);
                    writer.WriteBlockArray(Meshes[i].TexturedTriangles);

                    long meshOffset2 = writer.BaseStream.Position;
                    long meshSize = (meshOffset2 - meshOffset1);
                    if (meshSize % 4 != 0)
                    {
                        ushort tempFiller = 0;
                        writer.Write(tempFiller);
                        meshSize += 2;
                    }

                    for (int n = 0; n < NumMeshPointers; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            MeshPointers[n] = (uint)totalMeshSize;
                        }
                    }

                    totalMeshSize += (int)meshSize;
                }

                offset2 = writer.BaseStream.Position;
                uint meshDataSize = (uint)((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write(NumMeshPointers);
                writer.WriteBlockArray(MeshPointers);

                // Write animations' data
                writer.Write(NumAnimations);
                writer.WriteBlockArray(Animations);

                writer.Write(NumStateChanges);
                writer.WriteBlockArray(StateChanges);

                writer.Write(NumAnimDispatches);
                writer.WriteBlockArray(AnimDispatches);

                writer.Write(NumAnimCommands);
                writer.WriteBlockArray(AnimCommands);

                writer.Write(NumMeshTrees);
                writer.WriteBlockArray(MeshTrees);

                writer.Write(NumFrames);
                writer.WriteBlockArray(Frames);

                writer.Write(NumMoveables);
                writer.WriteBlockArray(Moveables);

                writer.Write(NumStaticMeshes);
                writer.WriteBlockArray(StaticMeshes);

                // SPR block
                SPR = new byte[] { 0x53, 0x50, 0x52 };
                writer.WriteBlockArray(SPR);

                writer.Write(NumSpriteTextures);
                writer.WriteBlockArray(SpriteTextures);

                writer.Write(NumSpriteSequences);
                writer.WriteBlockArray(SpriteSequences);

                // Write camera, flyby and sound sources
                writer.Write(NumCameras);
                writer.WriteBlockArray(Cameras);

                writer.Write(NumFlyByCameras);
                writer.WriteBlockArray(FlyByCameras);

                writer.Write(NumSoundSources);
                writer.WriteBlockArray(SoundSources);

                // Write pathfinding data
                writer.Write(NumBoxes);
                writer.WriteBlockArray(Boxes);

                writer.Write(NumOverlaps);
                writer.WriteBlockArray(Overlaps);

                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone1_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone2_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone3_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone4_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].FlyZone_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone1_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone2_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone3_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone4_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].FlyZone_Alternate);

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(NumAnimatedTextures);

                short numSets = (short)AnimatedTextures.Length;
                writer.Write(numSets);

                for (int i = 0; i < AnimatedTextures.Length; i++)
                {
                    writer.Write(AnimatedTextures[i].NumTextures);

                    for (int k = 0; k < AnimatedTextures[i].Textures.Length; k++)
                    {
                        writer.Write(AnimatedTextures[i].Textures[k]);
                    }
                }

                byte uv = 0;

                // writer.Write(uv);

                // Write object textures
                byte[] tex = new byte[] { 0x00, 0x54, 0x45, 0x58 };
                writer.WriteBlockArray(tex);

                ObjectTextures = _tempObjectTextures.ToArray();
                NumObjectTextures = (uint)ObjectTextures.Length;

                writer.Write(NumObjectTextures);
                writer.WriteBlockArray(ObjectTextures);

                // Write items and AI objects
                writer.Write(NumItems);
                writer.WriteBlockArray(Items);

                writer.Write(NumAiItems);
                writer.WriteBlockArray(AiItems);

                short NumDemo = 0;
                writer.Write(NumDemo);

                // Write sound data
                BinaryReaderEx readerSounds = new BinaryReaderEx(File.OpenRead(_editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".sfx"));

                /*byte[] sfxBuffer = readerSounds.ReadBytes((int)readerSounds.BaseStream.Length);
                readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                readerSounds.ReadBytes(370 * 2);*/

                byte[] soundMap = readerSounds.ReadBytes(370 * 2);
                NumSoundDetails = (uint)readerSounds.ReadInt32();
                byte[] soundDetails = readerSounds.ReadBytes((int)NumSoundDetails * 8);
                uint numSampleIndices = (uint)readerSounds.ReadInt32();
                byte[] sampleIndices = readerSounds.ReadBytes((int)numSampleIndices * 4);
                readerSounds.Close();

                writer.Write(soundMap);
                writer.Write(NumSoundDetails);
                writer.Write(soundDetails);
                writer.Write(numSampleIndices);
                writer.Write(sampleIndices);
                // writer.WriteBlockArray(sfxBuffer);

                writer.Write(NumDemo);
                writer.Write(NumDemo);
                writer.Write(NumDemo);

                writer.Flush();
                writer.Close();

                writer = new BinaryWriterEx(File.OpenWrite(_dest));
                BinaryReaderEx reader = new BinaryReaderEx(File.OpenRead("temp.bin"));

                ReportProgress(90, "Writing final level");

                byte[] version = new byte[] { 0x54, 0x52, 0x34, 0x00 };
                writer.WriteBlockArray(version);

                ReportProgress(95, "Writing textures");

                writer.Write(NumRoomTextureTiles);
                writer.Write(NumObjectTextureTiles);
                writer.Write(NumBumpTextureTiles);

                writer.Write(Texture32UncompressedSize);
                writer.Write(Texture32CompressedSize);
                writer.WriteBlockArray(Texture32);

                writer.Write(Texture16UncompressedSize);
                writer.Write(Texture16CompressedSize);
                writer.WriteBlockArray(Texture16);

                writer.Write(MiscTextureUncompressedSize);
                writer.Write(MiscTextureCompressedSize);
                writer.WriteBlockArray(MiscTexture);

                ReportProgress(95, "Compressing geometry data");

                int geometrySize = (int)reader.BaseStream.Length;
                byte[] levelData = reader.ReadBytes(geometrySize);
                byte[] buffer = Utils.CompressDataZLIB(levelData);
                LevelUncompressedSize = (uint)geometrySize;
                LevelCompressedSize = (uint)buffer.Length;

                ReportProgress(80, "Writing goemetry data");

                writer.Write(LevelUncompressedSize);
                writer.Write(LevelCompressedSize);
                writer.WriteBlockArray(buffer);

                int numSamples = _editor.Level.Wad.OriginalWad.Sounds.Count;
                writer.WriteBlock(numSamples);

                ReportProgress(80, "Writing WAVE sounds");

                writer.Write(bufferSamples);

                ReportProgress(99, "Done");

                writer.Flush();
                writer.Close();

                reader.Close();
            }
            catch (Exception ex)
            {
                writer.Close();
            }

            return true;
        }

        private bool WriteLevelTR3()
        {
            long offset;
            long offset2;

            TR4Wad wad = _editor.Level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            BinaryWriterEx writer = new BinaryWriterEx(File.OpenWrite(_dest));

            ReportProgress(85, "Writing geometry data to memory buffer");

            try
            {
                // Write version
                byte[] version = new byte[] { 0x38, 0x00, 0x18, 0xFF };
                writer.WriteBlockArray(version);

                BinaryReader readerPalette = new BinaryReader(File.OpenRead("Editor\\palette.bin"));
                byte[] palette = readerPalette.ReadBytes(1792);
                readerPalette.Close();

                // Write palette
                writer.Write(palette);

                // Write textures
                int numTextureTiles = NumRoomTextureTiles + NumObjectTextureTiles + 1;
                writer.Write(numTextureTiles);

                // Fake 8 bit textures
                byte[] fakeTextures = new byte[256 * 256 * numTextureTiles];
                writer.Write(fakeTextures);

                // 16 bit textures
                writer.Write(_textures16);

                BinaryReader readerRaw = new BinaryReader(File.OpenRead("sprites3.raw"));
                byte[] raw = readerRaw.ReadBytes(131072);
                readerRaw.Close();

                writer.Write(raw);

                int filler = 0;
                writer.Write(filler);

                NumRooms = (ushort)Rooms.Length;
                writer.Write(NumRooms);

                for (int i = 0; i < NumRooms; i++)
                {
                    writer.WriteBlock(Rooms[i].Info);

                    offset = writer.BaseStream.Position;

                    int numdw = 0;
                    writer.Write(numdw);

                    ushort tmp = 0;
                    tmp = (ushort)Rooms[i].Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(Rooms[i].Vertices);

                    tmp = (ushort)Rooms[i].Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (int k = 0; k < Rooms[i].Rectangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Rectangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[3]);
                            writer.Write(Rooms[i].Rectangles[k].Texture);
                        }
                    }

                    tmp = (ushort)Rooms[i].Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (int k = 0; k < Rooms[i].Triangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Triangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Triangles[k].Texture);
                        }
                    }

                    // For sprites, not used
                    tmp = 0;
                    writer.Write(tmp);

                    // Now save current offset and calculate the size of the geometry
                    offset2 = writer.BaseStream.Position;
                    ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

                    // Save the size of the geometry
                    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                    writer.Write(roomGeometrySize);
                    writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                    // Write portals
                    tmp = (ushort)Rooms[i].Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0) writer.WriteBlockArray(Rooms[i].Portals);

                    // Write sectors
                    writer.Write(Rooms[i].NumZSectors);
                    writer.Write(Rooms[i].NumXSectors);
                    writer.WriteBlockArray(Rooms[i].Sectors);

                    // Write room color
                    writer.Write(Rooms[i].AmbientIntensity1);
                    writer.Write(Rooms[i].AmbientIntensity2);

                    // Write lights
                    tmp = (ushort)Rooms[i].Lights.Length;
                    writer.WriteBlock(tmp);

                    for (int j = 0; j < tmp; j++)
                    {
                        tr4_room_light light = Rooms[i].Lights[j];
                        writer.Write(light.X);
                        writer.Write(light.Y);
                        writer.Write(light.Z);

                        int intensity = light.Intensity;
                        int falloff = (int)light.Out;

                        writer.Write(intensity);
                        writer.Write(falloff);
                    }

                    // Write static meshes
                    tmp = (ushort)Rooms[i].StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0) writer.WriteBlockArray(Rooms[i].StaticMeshes);

                    // Write final data
                    writer.Write(Rooms[i].AlternateRoom);
                    writer.Write(Rooms[i].Flags);
                    writer.Write(Rooms[i].WaterScheme);
                    writer.Write(Rooms[i].ReverbInfo);
                    writer.Write(Rooms[i].AlternateGroup);
                }

                // Write floordata
                NumFloorData = (uint)FloorData.Length;
                writer.Write(NumFloorData);
                writer.WriteBlockArray(FloorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                NumMeshData = 0;
                writer.Write(NumMeshData);
                int totalMeshSize = 0;

                for (int i = 0; i < Meshes.Length; i++)
                {
                    long meshOffset1 = writer.BaseStream.Position;

                    writer.WriteBlock(Meshes[i].Centre);
                    writer.Write(Meshes[i].Radius);

                    writer.Write(Meshes[i].NumVertices);
                    writer.WriteBlockArray(Meshes[i].Vertices);

                    writer.Write(Meshes[i].NumNormals);
                    if (Meshes[i].NumNormals > 0)
                    {
                        writer.WriteBlockArray(Meshes[i].Normals);
                    }
                    else
                    {
                        writer.WriteBlockArray(Meshes[i].Lights);
                    }

                    writer.Write(Meshes[i].NumTexturedRectangles);
                    for (int k = 0; k < Meshes[i].NumTexturedRectangles; k++)
                    {
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[0]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[1]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[2]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[3]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Texture);

                    }
                    // writer.WriteBlockArray(Meshes[i].TexturedRectangles);

                    writer.Write(Meshes[i].NumTexturedTriangles);
                    for (int k = 0; k < Meshes[i].NumTexturedTriangles; k++)
                    {
                        writer.Write(Meshes[i].TexturedTriangles[k].Vertices[0]);
                        writer.Write(Meshes[i].TexturedTriangles[k].Vertices[1]);
                        writer.Write(Meshes[i].TexturedTriangles[k].Vertices[2]);
                        writer.Write(Meshes[i].TexturedTriangles[k].Texture);

                    }

                    //  writer.WriteBlockArray(Meshes[i].TexturedTriangles);

                    writer.Write(Meshes[i].NumColoredRectangles);
                    //writer.WriteBlockArray(Meshes[i].ColoredRectangles);

                    writer.Write(Meshes[i].NumColoredTriangles);
                    //writer.WriteBlockArray(Meshes[i].ColoredTriangles);

                    long meshOffset2 = writer.BaseStream.Position;
                    long meshSize = (meshOffset2 - meshOffset1);
                    if (meshSize % 4 != 0)
                    {
                        ushort tempFiller = 0;
                        writer.Write(tempFiller);
                        meshSize += 2;
                    }

                    for (int n = 0; n < NumMeshPointers; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            MeshPointers[n] = (uint)totalMeshSize;
                        }
                    }

                    totalMeshSize += (int)meshSize;

                    //if (i < NumMeshes - 1) MeshPointers[i + 1] = MeshPointers[i] + (uint)meshSize;
                }

                offset2 = writer.BaseStream.Position;
                uint meshDataSize = (uint)((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write(NumMeshPointers);
                writer.WriteBlockArray(MeshPointers);

                // Write animations' data
                writer.Write(NumAnimations);
                for (int j = 0; j < Animations.Length; j++)
                {
                    tr_animation anim = Animations[j];

                    writer.Write(anim.FrameOffset);
                    writer.Write(anim.FrameRate);
                    writer.Write(anim.FrameSize);
                    writer.Write(anim.StateID);
                    writer.Write(anim.Speed);
                    writer.Write(anim.Accel);
                    writer.Write(anim.FrameStart);
                    writer.Write(anim.FrameEnd);
                    writer.Write(anim.NextAnimation);
                    writer.Write(anim.NextFrame);
                    writer.Write(anim.NumStateChanges);
                    writer.Write(anim.StateChangeOffset);
                    writer.Write(anim.NumAnimCommands);
                    writer.Write(anim.AnimCommand);
                }

                writer.Write(NumStateChanges);
                writer.WriteBlockArray(StateChanges);

                writer.Write(NumAnimDispatches);
                writer.WriteBlockArray(AnimDispatches);

                writer.Write(NumAnimCommands);
                writer.WriteBlockArray(AnimCommands);

                writer.Write(NumMeshTrees);
                writer.WriteBlockArray(MeshTrees);

                writer.Write(NumFrames);
                writer.WriteBlockArray(Frames);

                writer.Write(NumMoveables);
                writer.WriteBlockArray(Moveables);

                writer.Write(NumStaticMeshes);
                writer.WriteBlockArray(StaticMeshes);

                // SPR block
                BinaryReader readerSprites = new BinaryReader(File.OpenRead("sprites3.bin"));
                byte[] bufferSprites = readerSprites.ReadBytes((int)readerSprites.BaseStream.Length);
                readerSprites.Close();
                writer.Write(bufferSprites);


                /*writer.Write(NumSpriteTextures);
                writer.WriteBlockArray(SpriteTextures);

                writer.Write(NumSpriteSequences);
                writer.WriteBlockArray(SpriteSequences);
                */
                // Write camera, flyby and sound sources
                writer.Write(NumCameras);
                writer.WriteBlockArray(Cameras);

                writer.Write(NumSoundSources);
                writer.WriteBlockArray(SoundSources);

                // Write pathfinding data
                writer.Write(NumBoxes);
                writer.WriteBlockArray(Boxes);

                writer.Write(NumOverlaps);
                writer.WriteBlockArray(Overlaps);

                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone1_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone2_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone3_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone4_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].FlyZone_Normal);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone1_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone2_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone3_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].GroundZone4_Alternate);
                for (int i = 0; i < NumBoxes; i++) writer.Write(Zones[i].FlyZone_Alternate);

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(NumAnimatedTextures);

                short numSets = (short)AnimatedTextures.Length;
                writer.Write(numSets);

                for (int i = 0; i < AnimatedTextures.Length; i++)
                {
                    writer.Write(AnimatedTextures[i].NumTextures);

                    for (int k = 0; k < AnimatedTextures[i].Textures.Length; k++)
                    {
                        writer.Write(AnimatedTextures[i].Textures[k]);
                    }
                }

                byte uv = 0;

                // writer.Write(uv);

                // Write object textures
                ObjectTextures = _tempObjectTextures.ToArray();
                NumObjectTextures = (uint)ObjectTextures.Length;

                writer.Write(NumObjectTextures);
                for (int j = 0; j < NumObjectTextures; j++)
                {
                    writer.Write(ObjectTextures[j].Attributes);
                    writer.Write(ObjectTextures[j].Tile);
                    writer.WriteBlockArray(ObjectTextures[j].Vertices);
                }

                // Write items and AI objects
                writer.Write(NumItems);
                writer.WriteBlockArray(Items);

                byte[] lightmap = new byte[8192];
                writer.Write(lightmap);

                short NumDemo = 0;
                writer.Write(NumDemo);
                writer.Write(NumDemo);

                // Write sound data
                BinaryReaderEx readerSounds = new BinaryReaderEx(File.OpenRead("Graphics\\Wads\\" + _editor.Level.Wad.OriginalWad.BaseName + ".sfx"));
                byte[] sfxBuffer = readerSounds.ReadBytes((int)readerSounds.BaseStream.Length);
                readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                readerSounds.ReadBytes(370 * 2);
                NumSoundDetails = (uint)readerSounds.ReadInt16();
                readerSounds.Close();

                writer.WriteBlockArray(sfxBuffer);


                writer.Flush();
                writer.Close();

            }
            catch (Exception ex)
            {
                writer.Close();
            }

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
                tr_sound_source source = new Compilers.LevelCompilerTR4.tr_sound_source();
                SoundInstance instance = (SoundInstance)_editor.Level.Objects[_soundSourcesTable.ElementAt(i).Key];
                tr_room newRoom = Rooms[_roomsIdTable[instance.Room]];

                source.X = (int)(Rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                source.Y = (int)(Rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                source.Z = (int)(Rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                source.SoundID = (ushort)instance.SoundID;
                source.Flags = 0x80; // (ushort)instance.Flags;

                tempSoundSources.Add(source);
            }

            NumSoundSources = (uint)tempSoundSources.Count;
            SoundSources = tempSoundSources.ToArray();

            ReportProgress(41, "    Number of sound sources: " + NumSoundSources);

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
                tr_camera camera = new Compilers.LevelCompilerTR4.tr_camera();
                CameraInstance instance = (CameraInstance)_editor.Level.Objects[_cameraTable.ElementAt(i).Key];
                tr_room newRoom = Rooms[_roomsIdTable[instance.Room]];

                camera.X = (int)(Rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                camera.Y = (int)(Rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                camera.Z = (int)(Rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                camera.Room = (short)_roomsIdTable[instance.Room];
                if (instance.Fixed) camera.Flags = 0x01;

                tempCameras.Add(camera);
            }

            for (int i = 0; i < _sinkTable.Count; i++)
            {
                tr_camera camera = new Compilers.LevelCompilerTR4.tr_camera();
                SinkInstance instance = (SinkInstance)_editor.Level.Objects[_sinkTable.ElementAt(i).Key];
                tr_room newRoom = Rooms[_roomsIdTable[instance.Room]];

                int xSector = (int)Math.Floor(instance.Position.X / 1024);
                int zSector = (int)Math.Floor(instance.Position.Z / 1024);

                camera.X = (int)(Rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                camera.Y = (int)(Rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                camera.Z = (int)(Rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                camera.Room = instance.Strength;
                camera.Flags = (ushort)((newRoom.Sectors[newRoom.NumZSectors * xSector + zSector].BoxIndex & 0x7f00) >> 4);

                tempCameras.Add(camera);
            }

            NumCameras = (uint)tempCameras.Count;
            Cameras = tempCameras.ToArray();

            List<tr4_flyby_camera> tempFlyby = new List<Compilers.LevelCompilerTR4.tr4_flyby_camera>();

            for (int i = 0; i < _flybyTable.Count; i++)
            {
                tr4_flyby_camera flyby = new Compilers.LevelCompilerTR4.tr4_flyby_camera();
                FlybyCameraInstance instance = (FlybyCameraInstance)_editor.Level.Objects[_flybyTable.ElementAt(i).Key];
                tr_room newRoom = Rooms[_roomsIdTable[instance.Room]];

                flyby.X = (int)(Rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                flyby.Y = (int)(Rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                flyby.Z = (int)(Rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

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

            NumFlyByCameras = (uint)tempFlyby.Count;
            FlyByCameras = tempFlyby.ToArray();

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
                NumSpriteTextures = reader.ReadUInt32();
                SpriteTextures = new tr_sprite_texture[NumSpriteTextures];
                for (int i = 0; i < NumSpriteTextures; i++)
                {
                    byte[] buffer = new byte[16];
                    reader.ReadBlockArray(out buffer, 16);

                    tr_sprite_texture texture = new Compilers.LevelCompilerTR4.tr_sprite_texture();

                    texture.Tile = (ushort)(_numRoomTexturePages + _numobjectTexturePages);
                    texture.X = buffer[0];
                    texture.Y = buffer[1];
                    texture.Width = (ushort)(buffer[5] * 256);
                    texture.Height = (ushort)(buffer[7] * 256);
                    texture.LeftSide = buffer[0];
                    texture.TopSide = buffer[1];
                    texture.RightSide = (short)(buffer[0] + buffer[5] + 1);
                    texture.BottomSide = (short)(buffer[1] + buffer[7] + 1);

                    SpriteTextures[i] = texture;
                }

                // Unknown value
                int spriteDataSize = reader.ReadInt32();

                // Load the real sprite texture data
                _numSpriteTexturePages = spriteDataSize / (65536 * 3);
                if ((spriteDataSize % (65536 * 3)) != 0) _numSpriteTexturePages++;

                _spriteTexturePages = new byte[256 * 256 * _numSpriteTexturePages * 4];

                int x;
                int y;
                int bytesRead = 0;

                for (y = 0; y < _numSpriteTexturePages * 256; y++)
                {
                    if (bytesRead == spriteDataSize) break;

                    for (x = 0; x < 256; x++)
                    {
                        if (bytesRead == spriteDataSize) break;

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
                NumSpriteSequences = reader.ReadUInt32();
                SpriteSequences = new tr_sprite_sequence[NumSpriteSequences];
                reader.ReadBlockArray(out SpriteSequences, NumSpriteSequences);

                reader.Close();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private bool PrepareFontAndSkyTexture()
        {
            try
            {
                ReportProgress(18, "Building font & sky textures");

                BinaryReaderEx reader = new BinaryReaderEx(File.OpenRead("Graphics\\Common\\font.pc"));
                byte[] uncMiscTexture = new byte[256 * 256 * 4 * 2];

                byte[] buffer = new byte[256 * 256 * 4];

                reader.ReadBlockArray(out buffer, 256 * 256 * 4);
                reader.Close();

                Array.Copy(buffer, 0, uncMiscTexture, 0, 256 * 256 * 4);

                buffer = new byte[256 * 256 * 4];

                // If exists a sky with the same name of WAD, use it, otherwise take the default sky
                string skyFileName;
                if (File.Exists(_editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".raw"))
                    skyFileName = _editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".raw";
                else
                    skyFileName = "Graphics\\Common\\pcsky.raw";

                ReportProgress(18, "Reading sky texture: " + skyFileName);


                reader = new BinaryReaderEx(File.OpenRead(skyFileName));

                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        byte r = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte b = reader.ReadByte();

                        buffer[y * 1024 + 4 * x] = b;
                        buffer[y * 1024 + 4 * x + 1] = g;
                        buffer[y * 1024 + 4 * x + 2] = r;
                        buffer[y * 1024 + 4 * x + 3] = 255;

                    }
                }

                Array.Copy(buffer, 0, uncMiscTexture, 256 * 256 * 4, 256 * 256 * 4);

                ReportProgress(80, "Compressing font & sky textures");

                MiscTexture = Utils.CompressDataZLIB(uncMiscTexture);
                MiscTextureUncompressedSize = (uint)uncMiscTexture.Length;
                MiscTextureCompressedSize = (uint)MiscTexture.Length;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private byte[] _textures16;

        private void PrepareTextures()
        {
            ReportProgress(10, "Building final texture map");

            NumRoomTextureTiles = (ushort)_numRoomTexturePages;
            NumObjectTextureTiles = (ushort)(_numobjectTexturePages + _numSpriteTexturePages);

            byte[] uncTexture32 = new byte[_roomTexturePages.Length + _objectTexturePages.Length + _spriteTexturePages.Length];

            Array.Copy(_roomTexturePages, 0, uncTexture32, 0, _roomTexturePages.Length);
            Array.Copy(_objectTexturePages, 0, uncTexture32, _roomTexturePages.Length, _objectTexturePages.Length);
            Array.Copy(_spriteTexturePages, 0, uncTexture32, _roomTexturePages.Length + _objectTexturePages.Length, _spriteTexturePages.Length);

            ReportProgress(80, "Packing 32 bit textures to 16 bit");
            byte[] uncTexture16 = PackTextureMap32To16bit(uncTexture32, 256, (_numRoomTexturePages + _numobjectTexturePages + _numSpriteTexturePages) * 256);

            ReportProgress(80, "Compressing 32 bit textures");
            Texture32 = Utils.CompressDataZLIB(uncTexture32);
            Texture32UncompressedSize = (uint)uncTexture32.Length;
            Texture32CompressedSize = (uint)Texture32.Length;

            _textures16 = uncTexture16;

            ReportProgress(80, "Compressing 16 bit textures");
            Texture16 = Utils.CompressDataZLIB(uncTexture16);
            Texture16UncompressedSize = (uint)uncTexture16.Length;
            Texture16CompressedSize = (uint)Texture16.Length;
        }

        private void PrepareRoomTextures()
        {
            int i, k;
            _animTexturesRooms = new List<int>();
            _animTexturesGeneral = new List<int>();

            ReportProgress(1, "Preparing room textures");

            // Reset animated textures
            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                _level.AnimatedTextures[i].Variants = new List<AnimatedTextureSequenceVariant>();

                for (k = 0; k < _level.AnimatedTextures[i].Textures.Count; k++)
                {
                    _level.AnimatedTextures[i].Textures[k].NewID = -1;
                }
            }

            _tempAnimatedTextures = new List<AnimatedTextureSequenceVariant>();

            _tempObjectTextures = new List<tr_object_texture>();

            // I start with a 128 pages texture map (32 MB in memory)
            byte[,] _roomTextureMap = new byte[1024, 32768];

            // First, I have to filter only used textures and sort them (for now I use bubble sort, in the future a tree)
            List<LevelTexture> _tempTexturesList = new List<LevelTexture>();
            _texturesIdTable = new Dictionary<int, int>();

            for (i = 0; i < _editor.Level.TextureSamples.Count; i++)
            {
                LevelTexture oldSample = _editor.Level.TextureSamples.ElementAt(i).Value;

                // don't count for unused textures
                // if (oldSample.UsageCount <= 0) continue;

                oldSample.OldID = oldSample.ID;

                _tempTexturesList.Add(oldSample);
            }

            ReportProgress(2, "Sorting room textures");

            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                for (k = 0; k < _level.AnimatedTextures[i].Textures.Count; k++)
                {
                    LevelTexture newText = new LevelTexture();

                    newText.X = _level.AnimatedTextures[i].Textures[k].X;
                    newText.Y = _level.AnimatedTextures[i].Textures[k].Y;
                    newText.Width = 64;
                    newText.Height = 64;
                    newText.Page = _level.AnimatedTextures[i].Textures[k].Page;
                    newText.Animated = true;
                    newText.AnimatedSequence = i;
                    newText.AnimatedTexture = k;

                    _tempTexturesList.Add(newText);
                }
            }

            _tempTexturesArray = _tempTexturesList.ToArray();

            ReportProgress(3, "Building room texture map");

            // I've sorted the textures by height, now I build the texture map
            int numRoomTexturePages = 1;
            int x;
            int y;
            int z;
            int j;

            numRoomTexturePages = _editor.Level.TextureMap.Height / 256;
            for (x = 0; x < 256; x++)
            {
                for (y = 0; y < _editor.Level.TextureMap.Height; y++)
                {
                    System.Drawing.Color c = _editor.Level.TextureMap.GetPixel(x, y);

                    if (c.R == 255 & c.G == 0 && c.B == 255)
                    {
                        _roomTextureMap[x * 4 + 0, y] = 0;
                        _roomTextureMap[x * 4 + 1, y] = 0;
                        _roomTextureMap[x * 4 + 2, y] = 0;
                        _roomTextureMap[x * 4 + 3, y] = 0;
                    }
                    else
                    {
                        _roomTextureMap[x * 4 + 0, y] = c.B;
                        _roomTextureMap[x * 4 + 1, y] = c.G;
                        _roomTextureMap[x * 4 + 2, y] = c.R;
                        _roomTextureMap[x * 4 + 3, y] = 255;
                    }
                }
            }

            // Rebuild the ID table
            for (i = 0; i < _tempTexturesArray.Length; i++)
            {
                LevelTexture tex = _tempTexturesArray[i];
                tex.AlphaTest = true;
                tex.NewX = tex.X;
                tex.NewY = tex.Y;
                tex.NewPage = tex.Page;

                if (!tex.Animated)
                    _texturesIdTable.Add(tex.OldID, i);
                else
                    _level.AnimatedTextures[tex.AnimatedSequence].Textures[tex.AnimatedTexture].Texture = tex;
            }

            // Build the TR4 texture tiles
            List<tr_object_texture> _textureTiles = new List<Compilers.LevelCompilerTR4.tr_object_texture>();

            for (i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] == null) continue;

                Room room = _editor.Level.Rooms[i];

                for (x = 0; x < room.NumXSectors; x++)
                {
                    for (z = 0; z < room.NumZSectors; z++)
                    {
                        for (int f = 0; f < room.Blocks[x, z].Faces.Length; f++)
                        {
                            BlockFace face = room.Blocks[x, z].Faces[f];

                            // Ignore undefined faces and untextured faces
                            if (!face.Defined || face.Texture == -1) continue;

                            // Build (or get) the texture info
                            face.NewTexture = BuildTextureInfo(face);
                        }
                    }
                }
            }

            _roomTexturePages = new byte[numRoomTexturePages * 256 * 256 * 4];
            for (y = 0; y < 256 * numRoomTexturePages; y++)
            {
                for (x = 0; x < 1024; x++)
                {
                    _roomTexturePages[y * 1024 + x] = _roomTextureMap[x, y];
                }
            }

            _numRoomTexturePages = numRoomTexturePages;

            ReportProgress(5, "    Room texture pages: " + _numRoomTexturePages);

            ReportProgress(6, "Building animated textures table");

            // Prepare animated textures

            // Build remaining tiles
            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                for (j = 0; j < _level.AnimatedTextures[i].Variants.Count; j++)
                {
                    for (k = 0; k < _level.AnimatedTextures[i].Variants[j].Tiles.Count; k++)
                    {
                        if (_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID == -1)
                        {
                            _level.AnimatedTextures[i].Variants[j].Tiles[k].NewID = BuildAnimatedTextureInfo(_level.AnimatedTextures[i].Variants[j],
                                                                                                             _level.AnimatedTextures[i].Variants[j].Tiles[k],
                                                                                                             _level.AnimatedTextures[i].Textures[k].Texture);
                        }
                    }
                }
            }

            NumAnimatedTextures = 0;
            List<tr_animated_textures_set> tempAnimatedTextures = new List<tr_animated_textures_set>();
            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                for (j = 0; j < _level.AnimatedTextures[i].Variants.Count; j++)
                {
                    tr_animated_textures_set newSet = new tr_animated_textures_set();

                    List<short> tempTextureIds = new List<short>();
                    for (k = 0; k < _level.AnimatedTextures[i].Variants[j].Tiles.Count; k++)
                    {
                        tempTextureIds.Add((short)_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID);
                        if (!_animTexturesGeneral.Contains(_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID))
                            _animTexturesGeneral.Add(_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID);

                        NumAnimatedTextures++;
                    }

                    if (tempTextureIds.Count > 0)
                    {
                        newSet.Textures = tempTextureIds.ToArray();
                        newSet.NumTextures = (short)(newSet.Textures.Length - 1);
                        tempAnimatedTextures.Add(newSet);
                        NumAnimatedTextures++;
                    }
                }
            }

            // This because between NumAnimatedTextures and the array itself there's an extra short with the number of sets
            NumAnimatedTextures++;

            AnimatedTextures = tempAnimatedTextures.ToArray();

            _animTexturesGeneral.Sort();
            _animTexturesRooms.Sort();
        }

        public void BuildWadTexturePages()
        {
            ReportProgress(7, "Building WAD textures pages");

            TR4Wad wad = _editor.Level.Wad.OriginalWad;
            int x;
            int y;

            _objectTexturePages = new byte[256 * 256 * 4 * wad.NumTexturePages];
            _numobjectTexturePages = wad.NumTexturePages;

            for (y = 0; y < wad.NumTexturePages * 256; y++)
            {
                for (x = 0; x < 256; x++)
                {
                    byte r = wad.TexturePages[y, 3 * x + 0];
                    byte g = wad.TexturePages[y, 3 * x + 1];
                    byte b = wad.TexturePages[y, 3 * x + 2];

                    if (r == 255 && g == 0 && b == 255)
                    {
                        _objectTexturePages[y * 1024 + 4 * x + 0] = 0;
                        _objectTexturePages[y * 1024 + 4 * x + 1] = 0;
                        _objectTexturePages[y * 1024 + 4 * x + 2] = 0;
                        _objectTexturePages[y * 1024 + 4 * x + 3] = 0;
                    }
                    else
                    {
                        _objectTexturePages[y * 1024 + 4 * x + 0] = b;
                        _objectTexturePages[y * 1024 + 4 * x + 1] = g;
                        _objectTexturePages[y * 1024 + 4 * x + 2] = r;
                        _objectTexturePages[y * 1024 + 4 * x + 3] = 255;
                    }
                }
            }

            ReportProgress(8, "    WAD texture pages: " + wad.NumTexturePages);
        }

        private TextureSounds GetTextureSound(int texture)
        {
            LevelTexture txt = _level.TextureSamples[texture];

            for (int i = 0; i < _level.TextureSounds.Count; i++)
            {
                TextureSound txtSound = _level.TextureSounds[i];
                if (txt.X >= txtSound.X && txt.Y >= txtSound.Y && txt.X <= txtSound.X + 64 && txt.Y <= txtSound.Y + 64 &&
                    txt.Page == txtSound.Page)
                {
                    return txtSound.Sound;
                }
            }

            return TextureSounds.Stone;
        }

        public void CopyWadData()
        {
            ReportProgress(11, "Converting WAD data to TR4 format");

            TR4Wad wad = _editor.Level.Wad.OriginalWad;

            ReportProgress(12, "    Number of animations: " + wad.Animations.Count);

            NumAnimations = (uint)wad.Animations.Count;
            Animations = new tr_animation[wad.Animations.Count];
            for (int i = 0; i < NumAnimations; i++)
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

                Animations[i] = animation;
            }

            ReportProgress(13, "    Number of state changes: " + wad.Changes.Count);

            NumStateChanges = (uint)wad.Changes.Count;
            StateChanges = new tr_state_change[wad.Changes.Count];
            for (int i = 0; i < NumStateChanges; i++)
            {
                tr_state_change state = new tr_state_change();

                state.AnimDispatch = wad.Changes[i].DispatchesIndex;
                state.NumAnimDispatches = wad.Changes[i].NumDispatches;
                state.StateID = wad.Changes[i].StateId;

                StateChanges[i] = state;
            }

            ReportProgress(14, "    Number of animation dispatches: " + wad.Dispatches.Count);

            NumAnimDispatches = (uint)wad.Dispatches.Count;
            AnimDispatches = new tr_anim_dispatch[wad.Dispatches.Count];
            for (int i = 0; i < NumAnimDispatches; i++)
            {
                tr_anim_dispatch disp = new tr_anim_dispatch();

                disp.High = wad.Dispatches[i].High;
                disp.Low = wad.Dispatches[i].Low;
                disp.NextAnimation = wad.Dispatches[i].NextAnimation;
                disp.NextFrame = wad.Dispatches[i].NextFrame;

                AnimDispatches[i] = disp;
            }

            ReportProgress(15, "    Number of animation commands: " + wad.Commands.Count);

            NumAnimCommands = (uint)wad.Commands.Count;
            AnimCommands = new short[wad.Commands.Count];
            for (int i = 0; i < NumAnimCommands; i++)
            {
                AnimCommands[i] = wad.Commands[i];
            }

            NumMeshTrees = (uint)wad.Links.Count;
            MeshTrees = new int[wad.Links.Count];
            for (int i = 0; i < NumMeshTrees; i++)
            {
                MeshTrees[i] = wad.Links[i];
            }

            ReportProgress(16, "    Number of keyframes: " + wad.KeyFrames.Count);

            NumFrames = (uint)wad.KeyFrames.Count;
            Frames = new short[wad.KeyFrames.Count];
            for (int i = 0; i < NumFrames; i++)
            {
                Frames[i] = wad.KeyFrames[i];
            }

            NumMeshPointers = (uint)wad.Pointers.Count;
            MeshPointers = new uint[wad.Pointers.Count];
            for (int i = 0; i < NumMeshPointers; i++)
            {
                MeshPointers[i] = wad.Pointers[i];
            }

            ReportProgress(17, "    Number of moveables: " + wad.Moveables.Count);

            NumMoveables = (uint)wad.Moveables.Count;
            Moveables = new tr_moveable[wad.Moveables.Count];
            for (int i = 0; i < NumMoveables; i++)
            {
                tr_moveable mov = new tr_moveable();

                mov.Animation = (ushort)wad.Moveables[i].AnimationIndex;
                mov.FrameOffset = wad.Moveables[i].KeyFrameOffset;
                mov.MeshTree = wad.Moveables[i].LinksIndex;
                mov.NumMeshes = wad.Moveables[i].NumPointers;
                mov.ObjectID = wad.Moveables[i].ObjectID;
                mov.StartingMesh = wad.Moveables[i].PointerIndex;

                Moveables[i] = mov;
            }

            ReportProgress(18, "    Number of static meshes: " + wad.StaticMeshes.Count);

            NumStaticMeshes = (uint)wad.StaticMeshes.Count;
            StaticMeshes = new tr_staticmesh[wad.StaticMeshes.Count];
            for (int i = 0; i < NumStaticMeshes; i++)
            {
                tr_staticmesh sm = new tr_staticmesh();

                tr_bounding_box visibility = new Compilers.LevelCompilerTR4.tr_bounding_box();
                tr_bounding_box collision = new Compilers.LevelCompilerTR4.tr_bounding_box();

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

                StaticMeshes[i] = sm;
            }
        }

        private short BuildTextureInfo(BlockFace face)
        {
            tr_object_texture tile = new tr_object_texture();
            LevelTexture tex = _tempTexturesArray[_texturesIdTable[face.Texture]];

            // Texture page
            tile.Tile = (ushort)tex.NewPage;
            if (face.Shape == BlockFaceShape.Triangle) tile.Tile |= 0x8000;

            // Attributes
            tile.Attributes = 0;
            if (tex.AlphaTest) tile.Attributes = 1;
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
                if (face.DoubleSided) newId |= 0x8000;

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
                        if (!_animTexturesRooms.Contains(newId & 0x7fff)) _animTexturesRooms.Add(newId & 0x7fff);

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
                if (face.DoubleSided) test = test | 0x8000;
                return (short)test;
            }
        }

        private short BuildAnimatedTextureInfo(AnimatedTextureSequenceVariant aSet, AnimatedTextureVariantTile aTile,
                                               LevelTexture tex)
        {
            tr_object_texture tile = new tr_object_texture();

            // Texture page
            tile.Tile = (ushort)tex.NewPage;
            if (aSet.IsTriangle) tile.Tile |= 0x8000;

            // Attributes
            tile.Attributes = 0;
            if (tex.AlphaTest) tile.Attributes = 1;
            if (aSet.Transparent) tile.Attributes = 2;

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

        private byte[] PackTextureMap32To16bit(byte[] map, int width, int height)
        {
            byte[] newMap = new byte[width * height * 2];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ushort a1 = map[y * 256 * 4 + x * 4 + 3];
                    ushort r1 = map[y * 256 * 4 + x * 4 + 2];
                    ushort g1 = map[y * 256 * 4 + x * 4 + 1];
                    ushort b1 = map[y * 256 * 4 + x * 4 + 0];

                    ushort r = (ushort)(r1 / 8);
                    ushort g = (ushort)(g1 / 8);
                    ushort b = (ushort)(b1 / 8);
                    ushort a = 0x8000;

                    if (a1 == 0)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                        a = 0;
                    }
                    else
                    {
                        a = 0x8000;
                    }

                    if (r1 < 8) r = 0;
                    if (g1 < 8) g = 0;
                    if (b1 < 8) b = 0;

                    ushort tmp = 0;

                    if (r1 == 255 && g1 == 255 && b1 == 255)
                    {
                        tmp = 0xffff;
                    }
                    else
                    {
                        tmp |= a;
                        tmp |= (ushort)(r << 10);
                        tmp |= (ushort)(g << 5);
                        tmp |= b;
                    }

                    newMap[y * 256 * 2 + 2 * x] = (byte)(tmp & 0xff);
                    newMap[y * 256 * 2 + 2 * x + 1] = (byte)((tmp & 0xff00) >> 8);
                }
            }

            return newMap;
        }

        private ushort Pack24BitColorTo16bit(System.Drawing.Color color)
        {
            ushort r1 = (ushort)(color.R);
            ushort g1 = (ushort)(color.G);
            ushort b1 = (ushort)(color.B);

            ushort r = (ushort)(color.R / 8);
            ushort g = (ushort)(color.G / 8);
            ushort b = (ushort)(color.B / 8);

            if (r1 < 8) r = 0;
            if (g1 < 8) g = 0;
            if (b1 < 8) b = 0;

            ushort tmp = 0;

            if (r1 == 255 && g1 == 255 && b1 == 255)
            {
                tmp = 0xffff;
            }
            else
            {
                tmp |= 0;
                tmp |= (ushort)((b << 10) & 0x7c00);
                tmp |= (ushort)((g << 5) & 0x03e0);
                tmp |= (ushort)(r & 0x1f);
            }

            return tmp;
        }

        private ushort Pack24BitColorTo16bit(Vector4 color)
        {
            ushort r1 = (ushort)(color.X );
            ushort g1 = (ushort)(color.Y );
            ushort b1 = (ushort)(color.Z );

            ushort r = (ushort)Math.Floor(color.X  / 8);
            ushort g = (ushort)Math.Floor(color.Y  / 8);
            ushort b = (ushort)Math.Floor(color.Z  / 8);

            if (r1 < 8) r = 0;
            if (g1 < 8) g = 0;
            if (b1 < 8) b = 0;

            ushort tmp = 0;

            if (r1 > 255) r1 = 255;
            if (g1 > 255) g1 = 255;
            if (b1 > 255) b1 = 255;

            if (r1 == 255 && g1 == 255 && b1 == 255)
            {
                tmp = 0x7fff;
            }
            else
            {
                tmp |= 0;
                tmp |= (ushort)(r << 10);
                tmp |= (ushort)(g << 5);
                tmp |= b;
            }

            if (tmp > 0x7fff)
            {
                int hhgjk = 0;
            }

            return tmp;
        }

        private tr_room_sector GetSector(int room, int x, int z)
        {
            return Rooms[room].Sectors[Rooms[room].NumZSectors * x + z];
        }

        private void SaveSector(int room, int x, int z, tr_room_sector sector)
        {
            Rooms[room].Sectors[Rooms[room].NumZSectors * x + z] = sector;
        }

        private bool valueInRange(int value, int min, int max)
        {
            return (value >= min) && (value <= max);
        }

        private bool BoxesOverlap(int b1, int b2, out bool jump)
        {
            jump = false;

            tr_box_aux a = tempBoxes[b1];
            tr_box_aux b = tempBoxes[b2];

            // Check if there's overlapping and store edge and type
            bool overlapping = false;
            bool xOverlap = false;
            bool zOverlap = false;
            bool overlapNorth = false;
            bool overlapSouth = false;
            bool overlapEast = false;
            bool overlapWest = false;
            bool edgeNorth = false;
            bool edgeSouth = false;
            bool edgeEast = false;
            bool edgeWest = false;

            // North overlap
            if (a.Zmin >= b.Zmin && a.Zmin <= b.Zmax)
            {
                overlapSouth = true;
                edgeSouth = (a.Zmin == b.Zmax);
                zOverlap = true;
            }

            // South overlap
            if (b.Zmin >= a.Zmin && b.Zmin <= a.Zmax)
            {
                overlapNorth = true;
                edgeNorth = (b.Zmin == a.Zmax);
                zOverlap = true;
            }

            // East overlap
            if (a.Xmin >= b.Xmin && a.Xmin <= b.Xmax)
            {
                overlapWest = true;
                edgeWest = (a.Xmin == b.Xmax);
                xOverlap = true;
            }

            // West overlap
            if (b.Xmin >= a.Xmin && b.Xmin <= a.Xmax)
            {
                overlapEast = true;
                edgeEast = (b.Xmin == a.Xmax);
                xOverlap = true;
            }

            if (b1==745 && b2==746)
            {
                int kffk = 0;
            }

            bool jumpX = CheckIfCanJumpX(b1, b2);
            bool jumpZ = CheckIfCanJumpZ(b1, b2);
            
            if (jumpX || jumpZ)
            {
                jump = true;
                return true;
            }

            // Check if enemy can jump
            // Boxes must have the same floor height
           /* if (a.TrueFloor == b.TrueFloor)
            {
                // I've four cases to study
                // In each case, the procedure checks if between boxes there's an hole of 1 or 2 sectors
                if (b.Xmin - a.Xmax == 1 || b.Xmin - a.Xmax == 2)
                {
                    int step = b.Xmin - a.Xmax;

                    int z1 = Math.Max(a.Zmin, b.Zmin);
                    int z2 = Math.Min(a.Zmax, b.Zmax);

                    for (int z = z1; z < z2; z++)
                    {
                        int currX = a.Xmax;

                        int relativeX = currX - Rooms[a.Room].Info.X / 1024;
                        int relativeZ = z - Rooms[a.Room].Info.Z / 1024;

                        tr_room currentRoom = Rooms[a.Room];

                        if (relativeX == currentRoom.NumXSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeX = 1;
                                relativeZ = z - currentRoom.Info.Z / 1024;
                                if (relativeZ < 1 || relativeZ > currentRoom.NumZSectors - 2) continue;

                            }
                            else
                            {
                                continue;
                            }
                        }
                        
                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            if (step == 1)
                            {
                                jump = true;
                                return true;
                            }
                        }

                        relativeX++;

                        if (relativeX == currentRoom.NumXSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeX = 1;
                                relativeZ = z - currentRoom.Info.Z / 1024;
                                if (relativeZ < 1 || relativeZ > currentRoom.NumZSectors - 2) continue;
                            }
                        }

                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            jump = true;
                            return true;
                        }
                    }
                }

                if (a.Xmin - b.Xmax == 1 || a.Xmin - b.Xmax == 2)
                {
                    int step = a.Xmin - b.Xmax;

                    int z1 = Math.Max(a.Zmin, b.Zmin);
                    int z2 = Math.Min(a.Zmax, b.Zmax);

                    for (int z = z1; z < z2; z++)
                    {
                        int currX = b.Xmax;

                        int relativeX = currX - Rooms[b.Room].Info.X / 1024;
                        int relativeZ = z - Rooms[b.Room].Info.Z / 1024;

                        tr_room currentRoom = Rooms[b.Room];

                        if (relativeX == currentRoom.NumXSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeX = 1;
                                relativeZ = z - currentRoom.Info.Z / 1024;
                                if (relativeZ < 1 || relativeZ > currentRoom.NumZSectors - 2) continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        
                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            if (step == 1)
                            {
                                jump = true;
                                return true;
                            }
                        }

                        relativeX++;

                        if (relativeX == currentRoom.NumXSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeX = 1;
                                relativeZ = z - currentRoom.Info.Z / 1024;
                                if (relativeZ < 1 || relativeZ > currentRoom.NumZSectors - 2) continue;
                            }
                        }

                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            jump = true;
                            return true;
                        }
                    }
                }
                
                if (b.Zmin - a.Zmax == 1 || b.Zmin - a.Zmax == 2)
                {
                    int step = b.Zmin - a.Zmax;

                    int x1 = Math.Max(a.Xmin, b.Xmin);
                    int x2 = Math.Min(a.Xmax, b.Xmax);

                    for (int x = x1; x < x2; x++)
                    {
                        int currZ = a.Zmax;

                        int relativeZ = currZ - Rooms[a.Room].Info.Z / 1024;
                        int relativeX = x - Rooms[a.Room].Info.X / 1024;

                        tr_room currentRoom = Rooms[a.Room];

                        if (relativeZ == currentRoom.NumZSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeZ = 1;
                                relativeX = x - currentRoom.Info.X / 1024;
                                if (relativeX < 1 || relativeX > currentRoom.NumXSectors - 2) continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            if (step == 1)
                            {
                                jump = true;
                                return true;
                            }
                        }

                        relativeZ++;

                        if (relativeZ == currentRoom.NumZSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeZ = 1;
                                relativeX = x - currentRoom.Info.X / 1024;
                                if (relativeX < 1 || relativeX > currentRoom.NumXSectors - 2) continue;
                            }
                        }

                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            jump = true;
                            return true;
                        }
                    }
                }
                
                if (a.Zmin - b.Zmax == 1 || a.Zmin - b.Zmax == 2)
                {
                    int step = a.Zmin - b.Zmax;

                    int x1 = Math.Max(a.Xmin, b.Xmin);
                    int x2 = Math.Min(a.Xmax, b.Xmax);

                    for (int x = x1; x < x2; x++)
                    {
                        int currZ = b.Zmax;

                        int relativeZ = currZ - Rooms[b.Room].Info.Z / 1024;
                        int relativeX = x - Rooms[b.Room].Info.X / 1024;

                        tr_room currentRoom = Rooms[b.Room];

                        if (relativeZ == currentRoom.NumZSectors - 1)
                        {
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                relativeZ = 1;
                                relativeX = x - currentRoom.Info.X / 1024;
                                if (relativeX < 1 || relativeX > currentRoom.NumXSectors - 2) continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            if (step == 1)
                            {
                                jump = true;
                                return true;
                            }
                        }

                        relativeZ++;

                        if (relativeZ == currentRoom.NumZSectors - 1)
                        { 
                            if (currentRoom.AuxSectors[relativeX, relativeZ].WallPortal != -1)
                            {
                                 currentRoom = Rooms[_roomsIdTable[_level.Portals[currentRoom.AuxSectors[relativeX, relativeZ].WallPortal].AdjoiningRoom]];
                                 relativeZ = 1;
                                relativeX = x - currentRoom.Info.X / 1024;
                                if (relativeX < 1 || relativeX > currentRoom.NumXSectors - 2) continue;
                            }
                        }

                        if (currentRoom.AuxSectors[relativeX, relativeZ].MeanFloorHeight <= a.TrueFloor || currentRoom.AuxSectors[relativeX, relativeZ].Wall)
                        {
                            continue;
                        }
                        else
                        {
                            jump = true;
                            return true;
                        }
                    }
                }
            }*/

            // If no overlapping then don't execute the rest of the function
            overlapping = xOverlap && zOverlap;
            if (!overlapping) return false;

            // Boxes that are touching on corners are not overlapping
            if ((a.Xmax == b.Xmin && a.Zmax == b.Zmin) ||
                (a.Xmax == b.Xmin && a.Zmin == b.Zmax) ||
                (a.Xmin == b.Xmax && a.Zmax == b.Zmin) ||
                (a.Xmin == b.Xmax && a.Zmin == b.Zmax))
            {
                return false;
            }

            // If boxes are overlapping and the rooms are the same, then boxes overlap
            if (a.Room == b.Room) return true;
            
            // Otherwise, we must check if rooms are vertically reachable with a chain of rooms and portals
            tr_room room = Rooms[a.Room];

            int xMin = a.Xmin;
            int xMax = a.Xmax;
            int zMin = a.Zmin - 1;
            int zMax = a.Zmax - 1;

            bool foundOverlap = false;

            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    sbyte direction = (sbyte)(Rooms[a.Room].Info.YBottom > Rooms[b.Room].Info.YBottom ? 1 : -1);

                    tr_room r1 = Rooms[a.Room];
                    tr_room r2 = Rooms[b.Room];

                    if (a.Room != b.Room && (IsVerticallyReachable(a.Room, b.Room, x, z, direction) ||
                        r1.BaseRoom == r2.FlippedRoom || r1.FlippedRoom == r2.BaseRoom))
                    {
                        foundOverlap = true;
                        break;
                    }
                }
            }

            if (!foundOverlap) return false;

            return true;
        }

        private void GetAllReachableRooms()
        {
            for (int i = 0; i < _level.Rooms.Length; i++)
            {
                if (_level.Rooms[i] == null) continue;

                _level.Rooms[i].Visited = false;
                Rooms[_roomsIdTable[i]].ReachableRooms = new List<int>();
            }

            for (int i = 0; i < _level.Rooms.Length; i++)
            {
                if (_level.Rooms[i] == null) continue;

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
                        if (!Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                            Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
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
                        if (!Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                        {
                            Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
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
                        if (!Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                            Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
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
                        if (!Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Contains(_roomsIdTable[p.AdjoiningRoom]))
                        {
                            Rooms[_roomsIdTable[baseRoom]].ReachableRooms.Add(_roomsIdTable[p.AdjoiningRoom]);
                            GetAllReachableRoomsDown(baseRoom, p.AdjoiningRoom);
                        }
                    }
                }
            }
        }

        private bool IsVerticallyReachable(int room, int destRoom, int x, int z, sbyte direction)
        {
            if (room == destRoom) return true;

            for (int i = 0; i < Rooms[room].ReachableRooms.Count; i++)
            {
                if (Rooms[room].ReachableRooms[i] == destRoom) return true;
            }

            return false;
        }

        private bool BuildBox(int i, int x, int z, int xm, int xM, int zm, int zM, out tr_box_aux box)
        {
            tr_room room = Rooms[i];
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

                if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope)) break;
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

                if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope)) break;
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

                    if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope)) break;

                    tmpZ = z2;
                }

                if (tmpZ > zMin) zMin = tmpZ;
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

                    if (aux2.NotWalkableFloor || aux2.BorderWall || aux2.Wall || (aux.Box != aux2.Box) || (aux.Monkey != aux2.Monkey) || (aux.Portal != aux2.Portal) || aux2.LowestFloor != aux.LowestFloor || (aux.SoftSlope != aux2.SoftSlope)) break;

                    tmpZ = z2;
                }

                if (tmpZ < zMax) zMax = tmpZ;
            }

            if (tempBoxes.Count==285)
            {
                int hhh = 0;
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
                if (box.Xmin < xm) box.Xmin = (byte)xm;
                if (box.Xmax > xM) box.Xmax = (byte)xM;
                if (box.Zmin < zm) box.Zmin = (byte)zm;
                if (box.Zmax > zM) box.Zmax = (byte)zM;

                if (box.Xmax - box.Xmin <= 0) return false;
                if (box.Zmax - box.Zmin <= 0) return false;
            }

            return true;
        }

        private void BuildPathFindingData()
        {
            ReportProgress(50, "Building pathfinding data");

            // Fix monkey on portals
            for (int i = 0; i < Rooms.Length; i++)
            {
                tr_room fixRoom = Rooms[i];

                for (int x = 0; x < fixRoom.NumXSectors; x++)
                {
                    for (int z = 0; z < fixRoom.NumZSectors; z++)
                    {
                        if (fixRoom.AuxSectors[x, z].FloorPortal != -1)
                        {
                            Rooms[i].AuxSectors[x, z].Monkey = FindMonkeyFloor(i, x, z);
                        }
                    }
                }
            }

            // Build boxes
            tempBoxes = new List<Compilers.LevelCompilerTR4.tr_box_aux>();

            // First build boxes except portal boxes
            for (int i = 0; i < Rooms.Length; i++)
            {
                tr_room room = Rooms[i];

                for (int x = 1; x < room.NumXSectors - 1; x++)
                {
                    for (int z = 1; z < room.NumZSectors - 1; z++)
                    {
                        tr_room_sector sector = GetSector(i, x, z);
                        tr_sector_aux aux = room.AuxSectors[x, z];

                        // If this room is excluded from pathfinding or this sector is a Not Walkable Floor
                        if (_level.Rooms[room.OriginalRoomId].ExcludeFromPathFinding || aux.NotWalkableFloor)
                        {
                            sector.BoxIndex = (short)(0x7ff0 | (byte)room.TextureSounds[x, z]);
                            SaveSector(i, x, z, sector);

                            continue;
                        }

                        if (aux.Wall || aux.Portal || aux.HardSlope) continue;

                        // Build the box
                        tr_box_aux box;
                        bool result = BuildBox(i, x, z, 0, 0, 0, 0, out box);
                        if (!result) continue;

                        // Check if box exists
                        int found = -1;
                        for (int j = 0; j < tempBoxes.Count; j++)
                        {
                            tr_box_aux box2 = tempBoxes[j];

                            tr_room r1 = Rooms[box.Room];
                            tr_room r2 = Rooms[box2.Room];

                            if (box.Xmin == box2.Xmin && box.Xmax == box2.Xmax && box.Zmin == box2.Zmin && box.Zmax == box2.Zmax &&
                                (box.Room == box2.Room || (box.Room != box2.Room && (r1.BaseRoom == r2.FlippedRoom || r1.FlippedRoom == r2.BaseRoom))) &&
                                box.TrueFloor == box2.TrueFloor)
                            {
                                found = j;
                                break;
                            }
                        }

                        // If box is not found, then add the new box
                        if (found == -1)
                        {
                            tempBoxes.Add(box);

                            for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                            {
                                for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                                {
                                    int xc = x2 - room.Info.X / 1024;
                                    int zc = z2 - room.Info.Z / 1024;

                                    tr_room_sector sect = GetSector(i, xc, zc);
                                    tr_sector_aux aux2 = room.AuxSectors[xc, zc];

                                    if (aux2.Wall)
                                    {
                                        sect.BoxIndex = 0x7ff6;
                                    }
                                    else
                                    {
                                        sect.BoxIndex = (short)(((tempBoxes.Count - 1) << 4) | (byte)room.TextureSounds[xc, zc]);
                                    }

                                    SaveSector(i, xc, zc, sect);
                                }
                            }
                        }
                        else
                        {
                            for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                            {
                                for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                                {
                                    int xc = x2 - room.Info.X / 1024;
                                    int zc = z2 - room.Info.Z / 1024;

                                    tr_room_sector sect = GetSector(i, xc, zc);
                                    tr_sector_aux aux2 = room.AuxSectors[xc, zc];

                                    if (aux2.Wall)
                                    {
                                        sect.BoxIndex = 0x7ff6;
                                    }
                                    else
                                    {
                                        sect.BoxIndex = (short)(((found) << 4) | (byte)room.TextureSounds[xc, zc]);
                                    }

                                    SaveSector(i, xc, zc, sect);
                                }
                            }
                        }
                    }
                }
            }

            int lastBox = tempBoxes.Count - 1;

            // Now build only boxes of horizontal portals
            for (int i = 0; i < Rooms.Length; i++)
            {
                tr_room room = Rooms[i];

                for (int x = 1; x < room.NumXSectors - 1; x++)
                {
                    for (int z = 1; z < room.NumZSectors - 1; z++)
                    {
                        tr_room_sector sector = GetSector(i, x, z);
                        tr_sector_aux aux = room.AuxSectors[x, z];

                        // If this room is excluded from pathfinding or this sector is a Not Walkable Floor
                        if (_level.Rooms[room.OriginalRoomId].ExcludeFromPathFinding || aux.NotWalkableFloor)
                        {
                            sector.BoxIndex = (short)(0x7ff0 | (byte)room.TextureSounds[x, z]);
                            SaveSector(i, x, z, sector);

                            continue;
                        }

                        if (aux.FloorPortal == -1) continue;

                        int xMin = room.Info.X / 1024 + _level.Portals[aux.FloorPortal].X;
                        int xMax = room.Info.X / 1024 + _level.Portals[aux.FloorPortal].X + _level.Portals[aux.FloorPortal].NumXBlocks;
                        int zMin = room.Info.Z / 1024 + _level.Portals[aux.FloorPortal].Z;
                        int zMax = room.Info.Z / 1024 + _level.Portals[aux.FloorPortal].Z + _level.Portals[aux.FloorPortal].NumZBlocks;

                        int idRoom = i;

                        // Find the lowest room and floor
                        int room2;
                        short floor2;
                        bool result = GetMostDownFloorAndRoom(idRoom, x, z, out room2, out floor2);
                        if (!result) continue;

                        // Build the box
                        tr_box_aux box;
                        result = BuildBox(room2, x + Rooms[i].Info.X / 1024 - Rooms[room2].Info.X / 1024,
                                                 z + Rooms[i].Info.Z / 1024 - Rooms[room2].Info.Z / 1024,
                                                 xMin, xMax, zMin, zMax, out box);
                        box.Room = (short)room2;
                        if (!result) continue;

                        // Check if box exists
                        int found = -1;
                        for (int j = 0; j < tempBoxes.Count; j++)
                        {
                            tr_box_aux box2 = tempBoxes[j];

                            tr_room r1 = Rooms[box.Room];
                            tr_room r2 = Rooms[box2.Room];

                            if (box.Xmin == box2.Xmin && box.Xmax == box2.Xmax && box.Zmin == box2.Zmin && box.Zmax == box2.Zmax &&
                                (box.Room == box2.Room || (box.Room != box2.Room && (r1.BaseRoom == r2.FlippedRoom || r1.FlippedRoom == r2.BaseRoom))) &&
                                box.TrueFloor == box2.TrueFloor)
                            {
                                found = j;
                                break;
                            }
                        }

                        // If box is not found, then add the new box
                        if (found == -1)
                        {
                            box.TrueFloor = (short)(GetMostDownFloor(i, x, z));

                            tempBoxes.Add(box);
                            found = tempBoxes.Count - 1;
                        }

                        for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                        {
                            for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                            {
                                int xc = x2 - room.Info.X / 1024;
                                int zc = z2 - room.Info.Z / 1024;

                                tr_room_sector sect = GetSector(i, xc, zc);
                                tr_sector_aux aux2 = room.AuxSectors[xc, zc];

                                if (aux.FloorPortal == aux2.FloorPortal)
                                {
                                    sect.BoxIndex = (short)((found << 4) | (byte)room.TextureSounds[xc, zc]);
                                    SaveSector(i, xc, zc, sect);
                                }
                            }
                        }
                    }
                }
            }

            // Build overlaps
            List<tr_overlap_aux> tempOverlaps = new List<tr_overlap_aux>();

            for (int i = 0; i < tempBoxes.Count; i++)
            {
                tr_box_aux box = tempBoxes[i];

                bool foundOverlaps = false;
                short baseOverlaps = (short)tempOverlaps.Count;

                for (int j = 0; j < tempBoxes.Count; j++)
                {
                    // if they are the same box don't do anything
                    if (i == j) continue;

                    tr_box_aux box2 = tempBoxes[j];

                    // Now we have to find overlaps and edges
                    bool jump;
                    bool monkey;

                    if (BoxesOverlap(i, j, out jump))
                    {
                        tr_overlap_aux overlap = new Compilers.LevelCompilerTR4.tr_overlap_aux();
                        overlap.Box = j;
                        overlap.IsEdge = (box.Xmax == box2.Xmin || box.Zmax == box2.Zmin || box.Xmin == box2.Xmax || box.Zmin == box2.Zmax);
                        overlap.Monkey = box2.Monkey;
                        overlap.MainBox = i;

                        tempOverlaps.Add(overlap);

                        if (box.Jump == false) box.Jump = jump;
                        if (box2.Jump == false) box2.Jump = jump;

                        tempBoxes[j] = box2;

                        foundOverlaps = true;
                    }
                }

                if (foundOverlaps)
                {
                    box.OverlapIndex = baseOverlaps;
                }
                else
                {
                    box.OverlapIndex = 2047;
                }

                tempBoxes[i] = box;

                if (!foundOverlaps) continue;

                if (box.IsolatedBox) box.OverlapIndex = (short)(box.OverlapIndex | 0x8000);

                // Mark the end of the list
                tr_overlap_aux last = tempOverlaps[tempOverlaps.Count - 1];
                last.EndOfList = true;
                tempOverlaps[tempOverlaps.Count - 1] = last;
            }

            // Build final overlaps
            Overlaps = new ushort[tempOverlaps.Count];
            for (int i = 0; i < tempOverlaps.Count; i++)
            {
                ushort ov = (ushort)tempOverlaps[i].Box;

                // Is the last overlap of the list?
                if (tempOverlaps[i].EndOfList) ov |= 0x8000;

                // Monkey flag
                bool canMonkey = tempBoxes[tempOverlaps[i].Box].Monkey && tempBoxes[tempOverlaps[i].MainBox].Monkey;
                int step = (int)Math.Abs(tempBoxes[tempOverlaps[i].Box].TrueFloor - tempBoxes[tempOverlaps[i].MainBox].TrueFloor);
                if (canMonkey) ov |= 0x2000;

                bool canJump = tempBoxes[tempOverlaps[i].Box].Jump;
                if (canJump) ov |= 0x800;

                Overlaps[i] = ov;
            }

            Boxes = new tr_box[tempBoxes.Count];
            Zones = new tr_zone[tempBoxes.Count];

            // Convert boxes to TR format
            for (int i = 0; i < tempBoxes.Count; i++)
            {
                tr_box newBox = new tr_box();
                tr_box_aux aux = tempBoxes[i];
                tr_zone zone = new tr_zone();

                newBox.Xmin = aux.Xmin;
                newBox.Xmax = aux.Xmax;
                newBox.Zmin = aux.Zmin;
                newBox.Zmax = aux.Zmax;
                newBox.TrueFloor = aux.TrueFloor;
                newBox.OverlapIndex = aux.OverlapIndex;

                Boxes[i] = newBox;
                Zones[i] = zone;
            }

            // Finally, build zones
            ushort groundZone1 = 1;
            ushort groundZone2 = 1;
            ushort groundZone3 = 1;
            ushort groundZone4 = 1;
            ushort flyZone = 1;
            ushort a_groundZone1 = 1;
            ushort a_groundZone2 = 1;
            ushort a_groundZone3 = 1;
            ushort a_groundZone4 = 1;
            ushort a_flyZone = 1;

            for (int i = 0; i < Boxes.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (Zones[i].GroundZone1_Normal == 0)
                {
                    Zones[i].GroundZone1_Normal = groundZone1;

                    foreach (int box in GetAllReachableBoxes(i, 1))
                    {
                        Zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (Zones[i].GroundZone2_Normal == 0)
                {
                    Zones[i].GroundZone2_Normal = groundZone2;

                    foreach (int box in GetAllReachableBoxes(i, 2))
                    {
                        Zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (Zones[i].GroundZone3_Normal == 0)
                {
                    Zones[i].GroundZone3_Normal = groundZone3;

                    foreach (int box in GetAllReachableBoxes(i, 3))
                    {
                        Zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (Zones[i].GroundZone4_Normal == 0)
                {
                    Zones[i].GroundZone4_Normal = groundZone4;

                    foreach (int box in GetAllReachableBoxes(i, 4))
                    {
                        Zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (Zones[i].FlyZone_Normal == 0)
                {
                    Zones[i].FlyZone_Normal = flyZone;

                    foreach (int box in GetAllReachableBoxes(i, 5))
                    {
                        Zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }

                // Flipped rooms------------------------------------------

                // Skeleton like enemis: in the future implement also jump
                if (Zones[i].GroundZone1_Alternate == 0)
                {
                    Zones[i].GroundZone1_Alternate = a_groundZone1;

                    foreach (int box in GetAllReachableBoxes(i, 101))
                    {
                        Zones[box].GroundZone1_Alternate = a_groundZone1;
                    }

                    a_groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (Zones[i].GroundZone2_Alternate == 0)
                {
                    Zones[i].GroundZone2_Alternate = a_groundZone2;

                    foreach (int box in GetAllReachableBoxes(i, 102))
                    {
                        Zones[box].GroundZone2_Alternate = a_groundZone2;
                    }

                    a_groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (Zones[i].GroundZone3_Alternate == 0)
                {
                    Zones[i].GroundZone3_Alternate = a_groundZone3;

                    foreach (int box in GetAllReachableBoxes(i, 103))
                    {
                        Zones[box].GroundZone3_Alternate = a_groundZone3;
                    }

                    a_groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (Zones[i].GroundZone4_Alternate == 0)
                {
                    Zones[i].GroundZone4_Alternate = a_groundZone4;

                    foreach (int box in GetAllReachableBoxes(i, 104))
                    {
                        Zones[box].GroundZone4_Alternate = a_groundZone4;
                    }

                    a_groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (Zones[i].FlyZone_Alternate == 0)
                {
                    Zones[i].FlyZone_Alternate = a_flyZone;

                    foreach (int box in GetAllReachableBoxes(i, 105))
                    {
                        Zones[box].FlyZone_Alternate = a_flyZone;
                    }

                    a_flyZone++;
                }
            }

            NumBoxes = (uint)Boxes.Length;
            NumOverlaps = (uint)Overlaps.Length;


            ReportProgress(60, "    Number of boxes: " + NumBoxes);
            ReportProgress(60, "    Number of overlaps: " + NumOverlaps);
            ReportProgress(60, "    Number of zones: " + NumBoxes);
        }

        private List<int> GetAllReachableBoxes(int box, int zoneType)
        {
            List<int> boxes = new List<int>();

            // This is a non-recursive version of the algorithm for finding all reachable boxes. 
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag
            bool isWater = (Rooms[tempBoxes[box].Room].Flags & 0x01) != 0;

            while (stack.Count > 0)
            {
                int next = stack.Pop();
                bool last = false;

                for (int i = Boxes[next].OverlapIndex; i < Overlaps.Length && !last; i++)
                {
                    last = (Overlaps[i] & 0x8000) != 0;
                    int boxIndex = Overlaps[i] & 0x7ff;

                    bool add = false;

                    // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                    if (zoneType == 1 || zoneType == 101)
                    {
                        bool water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        int step = (int)(Math.Abs(Boxes[next].TrueFloor - Boxes[boxIndex].TrueFloor));
                        if (water == isWater && step <= 256) add = true;
                    }

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 blocks.
                    if (zoneType == 2 || zoneType == 102)
                    {
                        bool water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        int step = (int)(Math.Abs(Boxes[next].TrueFloor - Boxes[boxIndex].TrueFloor));

                        // Check all possibilities
                        bool canJump = tempBoxes[boxIndex].Jump;
                        bool canClimb = Math.Abs(step) <= 256;

                        if (water == isWater && (canJump || canClimb)) add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step. In water they act like flying enemies.
                    if (zoneType == 3 || zoneType == 103)
                    {
                        bool water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        int step = (int)(Math.Abs(Boxes[next].TrueFloor - Boxes[boxIndex].TrueFloor));
                        if (((water == isWater && step <= 256) || water)) add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 blocks and monkey.
                    if (zoneType == 4 || zoneType == 104)
                    {
                        bool water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        int step = (int)(Boxes[boxIndex].TrueFloor - Boxes[next].TrueFloor);

                        // Check all possibilities
                        bool canJump = tempBoxes[boxIndex].Jump;
                        bool canClimb = Math.Abs(step) <= 1024;
                        bool canMonkey = tempBoxes[boxIndex].Monkey;

                        if (water == isWater && (canJump || canClimb || canMonkey)) add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5 || zoneType == 105)
                    {
                        bool water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        if (water == isWater) add = true;
                    }

                    if (!stack.Contains(boxIndex) && add)
                    {
                        if (!boxes.Contains(boxIndex)) stack.Push(boxIndex);
                        boxes.Add(boxIndex);
                    }
                }
            }

            return boxes;
        }

        private short GetMostDownBox(int room, int x, int z)
        {
            tr_room_sector sector = GetSector(room, x, z);
            if (sector.RoomBelow == 255) return -1;

            tr_room room1 = Rooms[room];
            tr_room room2 = Rooms[sector.RoomBelow];

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
                tr_sector_aux aux3 = Rooms[room].AuxSectors[x, z];
                return (short)(aux3.LowestFloor * 256);
            }

            tr_room room1 = Rooms[room];
            tr_room room2 = Rooms[sector.RoomBelow];

            int x2 = room1.Info.X / 1024 + x - room2.Info.X / 1024;
            int z2 = room1.Info.Z / 1024 + z - room2.Info.Z / 1024;

            tr_room_sector sector2 = GetSector(sector.RoomBelow, x2, z2);
            tr_sector_aux aux2 = Rooms[sector.RoomBelow].AuxSectors[x2, z2];

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

            tr_room room1 = Rooms[room];
            tr_room room2 = Rooms[sector.RoomBelow];

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
                return Rooms[room].AuxSectors[x, z].Monkey;
            }

            tr_room room1 = Rooms[room];
            tr_room room2 = Rooms[sector.RoomBelow];

            int x2 = room1.Info.X / 1024 + x - room2.Info.X / 1024;
            int z2 = room1.Info.Z / 1024 + z - room2.Info.Z / 1024;

            tr_room_sector sector2 = GetSector(sector.RoomBelow, x2, z2);

            bool result = false;

            if (sector2.RoomBelow == 255)
            {
                result = Rooms[sector.RoomBelow].AuxSectors[x2, z2].Monkey;
            }
            else
            {
                result = FindMonkeyFloor(sector.RoomBelow, x2, z2);
            }

            return result;
        }

        private void MatchPortalShades()
        {
            //  return; 

            for (int i = 0; i < _level.Portals.Count; i++)
            {
                _level.Portals.ElementAt(i).Value.LightAveraged = false;
            }

            for (int i = 0; i < _level.Portals.Count; i++)
            {
                // Get current portal and its paired portal
                Portal currentPortal = _level.Portals.ElementAt(i).Value;
                Portal otherPortal = _level.Portals[currentPortal.OtherID];

                // If the light was already averaged, then continue loop
                //if (currentPortal.LightAveraged) continue;

                // Get the rooms
                Room currentRoom = _level.Rooms[currentPortal.Room];
                Room otherRoom = _level.Rooms[otherPortal.Room];

                if (currentPortal.Direction == PortalDirection.North)
                {
                    for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                    {
                        int facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[x, currentRoom.NumZSectors - 1]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[x, currentRoom.NumZSectors - 1, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[facingX, 1]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[facingX, 1, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                    v1.FaceColor.X = meanR;
                                    v1.FaceColor.Y = meanG;
                                    v1.FaceColor.Z = meanB;

                                    v2.FaceColor.X = meanR;
                                    v2.FaceColor.Y = meanG;
                                    v2.FaceColor.Z = meanB;

                                    currentRoom.VerticesGrid[x, currentRoom.NumZSectors - 1, m] = v1;
                                    otherRoom.VerticesGrid[facingX, 1, n] = v2;

                                    break;
                                }
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.South)
                {
                    for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                    {
                        int facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[x, 1]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[x, 1, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[facingX, otherRoom.NumZSectors - 1]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[facingX, otherRoom.NumZSectors - 1, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                    v1.FaceColor.X = meanR;
                                    v1.FaceColor.Y = meanG;
                                    v1.FaceColor.Z = meanB;

                                    v2.FaceColor.X = meanR;
                                    v2.FaceColor.Y = meanG;
                                    v2.FaceColor.Z = meanB;

                                    currentRoom.VerticesGrid[x, 1, m] = v1;
                                    otherRoom.VerticesGrid[facingX, otherRoom.NumZSectors - 1, n] = v2;

                                    break;
                                }
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.East)
                {
                    for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                    {
                        int facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[currentRoom.NumXSectors - 1, z]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[currentRoom.NumXSectors - 1, z, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[1, facingZ]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[1, facingZ, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                    v1.FaceColor.X = meanR;
                                    v1.FaceColor.Y = meanG;
                                    v1.FaceColor.Z = meanB;

                                    v2.FaceColor.X = meanR;
                                    v2.FaceColor.Y = meanG;
                                    v2.FaceColor.Z = meanB;

                                    currentRoom.VerticesGrid[currentRoom.NumXSectors - 1, z, m] = v1;
                                    otherRoom.VerticesGrid[1, facingZ, n] = v2;

                                    break;
                                }
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.West)
                {
                    for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                    {
                        int facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[1, z]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[1, z, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[otherRoom.NumXSectors - 1, facingZ]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[otherRoom.NumXSectors - 1, facingZ, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                    v1.FaceColor.X = meanR;
                                    v1.FaceColor.Y = meanG;
                                    v1.FaceColor.Z = meanB;

                                    v2.FaceColor.X = meanR;
                                    v2.FaceColor.Y = meanG;
                                    v2.FaceColor.Z = meanB;

                                    currentRoom.VerticesGrid[1, z, m] = v1;
                                    otherRoom.VerticesGrid[otherRoom.NumXSectors - 1, facingZ, n] = v2;

                                    break;
                                }
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.Floor || currentPortal.Direction == PortalDirection.Ceiling)
                {
                    if (!(currentRoom.FlagWater ^ otherRoom.FlagWater))
                    {
                        for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                        {
                            for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                            {
                                int facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                                int facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                                for (int m = 0; m < currentRoom.NumVerticesInGrid[x, z]; m++)
                                {
                                    EditorVertex v1 = currentRoom.VerticesGrid[x, z, m];

                                    for (int n = 0; n < otherRoom.NumVerticesInGrid[facingX, facingZ]; n++)
                                    {
                                        EditorVertex v2 = otherRoom.VerticesGrid[facingX, facingZ, n];

                                        if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                        {
                                            int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                            int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                            int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                            v1.FaceColor.X = meanR;
                                            v1.FaceColor.Y = meanG;
                                            v1.FaceColor.Z = meanB;

                                            v2.FaceColor.X = meanR;
                                            v2.FaceColor.Y = meanG;
                                            v2.FaceColor.Z = meanB;

                                            currentRoom.VerticesGrid[x, z, m] = v1;
                                            otherRoom.VerticesGrid[facingX, facingZ, n] = v2;

                                            break;
                                        }
                                    }
                                }

                                _level.Rooms[currentPortal.Room] = currentRoom;
                                _level.Rooms[otherPortal.Room] = otherRoom;
                            }
                        }
                    }
                }

                _level.Portals[currentPortal.ID].LightAveraged = true;
                _level.Portals[otherPortal.ID].LightAveraged = true;
            }
        }

        private void BuildRooms()
        {
            ReportProgress(20, "Building rooms");

            // Map editor room indices to final level indices
            _roomsIdTable = new Dictionary<int, int>();
            int lastRoom = 0;

            for (int i = 0; i < _level.Rooms.Length - 1; i++)
            {
                Room room = _level.Rooms[i];
                if (room == null) continue;

                _roomsIdTable.Add(i, lastRoom);
                lastRoom++;
            }

            // Average lighting at the portals
            MatchPortalShades();

            Rooms = new tr_room[_roomsIdTable.Count];

            for (int i = 0; i < _level.Rooms.Length - 1; i++)
            {
                Room room = _level.Rooms[i];
                if (room == null) continue;

                tr_room newRoom = new tr_room();

                newRoom.OriginalRoomId = i;

                newRoom.TextureSounds = new TextureSounds[room.NumXSectors, room.NumZSectors];

                newRoom.Lights = new Compilers.LevelCompilerTR4.tr4_room_light[0];
                newRoom.StaticMeshes = new Compilers.LevelCompilerTR4.tr_room_staticmesh[0];
                newRoom.Portals = new Compilers.LevelCompilerTR4.tr_room_portal[0];

                newRoom.Info = new tr_room_info();
                newRoom.Info.X = (int)(room.Position.X * 1024.0f);
                newRoom.Info.Z = (int)(room.Position.Z * 1024.0f);
                newRoom.Info.YTop = (int)(-room.Position.Y * 256.0f - room.GetHighestCorner() * 256.0f);
                newRoom.Info.YBottom = (int)(-room.Position.Y * 256.0f);
                newRoom.NumXSectors = (ushort)(room.NumXSectors);
                newRoom.NumZSectors = (ushort)(room.NumZSectors);
                newRoom.AlternateRoom = (short)(room.Flipped && room.AlternateRoom != -1 ? _roomsIdTable[room.AlternateRoom] : -1);
                newRoom.AlternateGroup = (byte)(room.Flipped && room.AlternateRoom != -1 ? room.AlternateGroup : 0);
                newRoom.Flipped = room.Flipped;
                newRoom.FlippedRoom = (short)(room.AlternateRoom != -1 ? _roomsIdTable[room.AlternateRoom] : -1);
                newRoom.BaseRoom = (short)(room.BaseRoom != -1 ? _roomsIdTable[room.BaseRoom] : -1);

                newRoom.AmbientIntensity2 = (ushort)(0x0000 + room.AmbientLight.R);
                newRoom.AmbientIntensity1 = (ushort)((room.AmbientLight.G << 8) + room.AmbientLight.B);

                newRoom.ReverbInfo = (byte)room.Reverberation;

                // Room flags
                newRoom.Flags = 0x40;

                if (room.FlagWater) newRoom.Flags += 0x01;
                if (room.FlagOutside) newRoom.Flags += 0x20;
                if (room.FlagHorizon) newRoom.Flags += 0x08;
                if (room.FlagQuickSand) newRoom.Flags += 0x80;
                if (room.FlagMist) newRoom.Flags += 0x100;
                if (room.FlagReflection) newRoom.Flags += 0x200;
                if (room.FlagSnow) newRoom.Flags += 0x400;
                if (room.FlagRain) newRoom.Flags += 0x800;
                if (room.FlagDamage) newRoom.Flags += 0x1000;

                // Set the water scheme. I don't know how is calculated, but I have a table of all combinations of 
                // water and reflectivity. The water scheme must be set for the TOP room, in water room is 0x00.
                List<int> waterPortals = new List<int>();

                if (!room.FlagWater)
                {
                    short foundWaterRoom = -1;

                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            if (room.Blocks[x, z].FloorPortal != -1)
                            {
                                if (_level.Rooms[_level.Portals[room.Blocks[x, z].FloorPortal].AdjoiningRoom].FlagWater)
                                {
                                    if (!waterPortals.Contains(room.Blocks[x, z].FloorPortal)) waterPortals.Add(room.Blocks[x, z].FloorPortal);
                                }
                            }
                        }
                    }

                    if (waterPortals.Count != 0)
                    {
                        foundWaterRoom = _level.Portals[waterPortals[0]].AdjoiningRoom;

                        Room waterRoom = _level.Rooms[foundWaterRoom];

                        if (!room.FlagReflection && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x06;
                        if (!room.FlagReflection && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0a;
                        if (!room.FlagReflection && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0e;
                        if (!room.FlagReflection && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x12;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x05;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x06;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x07;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x08;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x09;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0a;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0b;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0c;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0d;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0e;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0f;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x10;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x11;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x12;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x13;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x14;
                    }
                }

                if (room.FlagMist) newRoom.WaterScheme += (byte)room.MistLevel;

                int lowest = -room.GetLowestCorner() * 256 + newRoom.Info.YBottom;

                // Prepare optimized vertices
                room.OptimizedVertices = new List<EditorVertex>();
                Dictionary<int, int> indicesDictionary = new Dictionary<int, int>();

                for (int x = 0; x < room.NumXSectors; x++)
                {
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        short base1 = (short)((x << 9) + (z << 4));

                        for (int n = 0; n < room.NumVerticesInGrid[x, z]; n++)
                        {
                            indicesDictionary.Add(base1 + n, room.OptimizedVertices.Count);
                            room.OptimizedVertices.Add(room.VerticesGrid[x, z, n]);
                        }
                    }
                }

                newRoom.Vertices = new tr_room_vertex[room.OptimizedVertices.Count];
                for (int j = 0; j < room.OptimizedVertices.Count; j++)
                {
                    tr_room_vertex rv = new tr_room_vertex();

                    tr_vertex v = new tr_vertex();
                    v.X = (short)room.OptimizedVertices[j].Position.X;
                    v.Y = (short)(-room.OptimizedVertices[j].Position.Y + newRoom.Info.YBottom);
                    v.Z = (short)room.OptimizedVertices[j].Position.Z;

                    if (i==0)
                    {
                        int gggh = 0;
                    }

                    rv.Vertex = v;
                    rv.Lighting1 = 0;
                    rv.Lighting2 = (short)Pack24BitColorTo16bit(room.OptimizedVertices[j].FaceColor);
                    rv.Attributes = 0;

                    // Water special effects
                    if (room.FlagWater)
                    {
                        rv.Attributes = 0x4000;
                    }
                    else
                    {
                        for (int ip = 0; ip < waterPortals.Count; ip++)
                        {
                            Portal portal = _level.Portals[waterPortals[ip]];

                            if (v.X > portal.X * 1024 && v.X < (portal.X + portal.NumXBlocks) * 1024 &&
                                v.Z > portal.Z * 1024 && v.Z < (portal.Z + portal.NumZBlocks) * 1024 &&
                                v.Y == lowest)
                            {
                                int xv = v.X / 1024;
                                int zv = v.Z / 1024;

                                if (!(room.Blocks[xv, zv].IsFloorSolid || room.Blocks[xv, zv].Type == BlockType.Wall || room.Blocks[xv, zv].Type == BlockType.BorderWall) &&
                                    !(room.Blocks[xv - 1, zv].IsFloorSolid || room.Blocks[xv - 1, zv].Type == BlockType.Wall || room.Blocks[xv - 1, zv].Type == BlockType.BorderWall) &&
                                    !(room.Blocks[xv, zv - 1].IsFloorSolid || room.Blocks[xv, zv - 1].Type == BlockType.Wall || room.Blocks[xv, zv - 1].Type == BlockType.BorderWall) &&
                                    !(room.Blocks[xv - 1, zv - 1].IsFloorSolid || room.Blocks[xv - 1, zv - 1].Type == BlockType.Wall || room.Blocks[xv - 1, zv - 1].Type == BlockType.BorderWall))
                                {
                                    rv.Attributes = 0x6000;
                                }
                            }
                            else
                            {
                                if (room.FlagReflection)
                                {
                                    if (v.X >= (portal.X - 1) * 1024 && v.X <= (portal.X + portal.NumXBlocks + 1) * 1024 &&
                                        v.Z >= (portal.Z - 1) * 1024 && v.Z <= (portal.Z + portal.NumZBlocks + 1) * 1024)
                                    {
                                        rv.Attributes = 0x4000;
                                    }
                                }
                            }
                        }
                    }

                    newRoom.Vertices[j] = rv;
                }

                List<tr_face4> tempRectangles = new List<tr_face4>();
                List<tr_face3> tempTriangles = new List<tr_face3>();

                for (int x = 0; x < room.NumXSectors; x++)
                {
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int f = 0; f < room.Blocks[x, z].Faces.Length; f++)
                        {
                            BlockFace face = room.Blocks[x, z].Faces[f];
                            if (face == null || !face.Defined) continue;

                            if ((f == 25 || f == 26) && (face.Invisible || face.Texture == -1))
                            {
                                newRoom.TextureSounds[x, z] = TextureSounds.Stone;
                            }

                            if (face.Invisible) continue;

                            // Assign texture sound
                            if ((f == (int)BlockFaces.Floor || f == (int)BlockFaces.FloorTriangle2))
                            {
                                newRoom.TextureSounds[x, z] = (face.Texture != -1 ? GetTextureSound(face.Texture) : TextureSounds.Stone);
                            }

                            if (face.Shape == BlockFaceShape.Rectangle)
                            {
                                tr_face4 rectangle = new tr_face4();

                                List<ushort> indices = new List<ushort>();

                                indices.Add((ushort)indicesDictionary[face.Indices[0]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[1]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[2]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[3]]);

                                byte rot = face.Rotation;
                                if (rot != 0)
                                {
                                    for (int n = 0; n < rot; n++)
                                    {
                                        ushort tmp = indices[0];
                                        indices.RemoveAt(0);
                                        indices.Insert(3, tmp);
                                    }
                                }

                                rectangle.Vertices = new ushort[4];
                                rectangle.Vertices[0] = indices[0];
                                rectangle.Vertices[1] = indices[1];
                                rectangle.Vertices[2] = indices[2];
                                rectangle.Vertices[3] = indices[3];

                                rectangle.Texture = (short)face.NewTexture;

                                tempRectangles.Add(rectangle);
                            }
                            else
                            {
                                tr_face3 triangle = new tr_face3();

                                List<ushort> indices = new List<ushort>();

                                indices.Add((ushort)indicesDictionary[face.Indices[0]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[1]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[2]]);

                                byte rot = face.Rotation;
                                if (rot != 0)
                                {
                                    for (int n = 0; n < rot; n++)
                                    {
                                        ushort tmp = indices[0];
                                        indices.RemoveAt(0);
                                        indices.Insert(2, tmp);
                                    }
                                }

                                triangle.Vertices = new ushort[4];
                                triangle.Vertices[0] = indices[0];
                                triangle.Vertices[1] = indices[1];
                                triangle.Vertices[2] = indices[2];

                                triangle.Texture = (short)face.NewTexture;

                                tempTriangles.Add(triangle);
                            }
                        }
                    }
                }

                newRoom.Rectangles = tempRectangles.ToArray();
                newRoom.Triangles = tempTriangles.ToArray();

                newRoom.NumRectangles = (ushort)tempRectangles.Count;
                newRoom.NumTriangles = (ushort)tempTriangles.Count;

                // Build portals
                List<tr_room_portal> tempPortals = new List<tr_room_portal>();
                List<int> tempIdPortals = new List<int>();
                Dictionary<int, int> portalToRooms = new Dictionary<int, int>();

                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        if (room.Blocks[x, z].WallPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].WallPortal))
                            tempIdPortals.Add(room.Blocks[x, z].WallPortal);

                        if (room.Blocks[x, z].FloorPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].FloorPortal))
                            tempIdPortals.Add(room.Blocks[x, z].FloorPortal);

                        if (room.Blocks[x, z].CeilingPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].CeilingPortal))
                            tempIdPortals.Add(room.Blocks[x, z].CeilingPortal);
                    }
                }

                if (i==110)
                {
                    int jjj = 0;
                }

                for (int j = 0; j < tempIdPortals.Count; j++)
                {
                    Portal portal = _editor.Level.Portals[tempIdPortals[j]];
                    tr_room_portal newPortal = new tr_room_portal();

                    Block startBlock;
                    Block endBlock;

                    int xMin = 0;
                    int xMax = 0;
                    int yMin1 = 0;
                    int yMax1 = 0;
                    int yMin2 = 0;
                    int yMax2 = 0;
                    int zMin = 0;
                    int zMax = 0;

                    newPortal.AdjoiningRoom = (ushort)_roomsIdTable[(int)portal.AdjoiningRoom];

                    newPortal.Vertices = new tr_vertex[4];

                    // Normal and vertices
                    if (portal.Direction == PortalDirection.North)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = -1;

                        startBlock = room.Blocks[portal.X, room.NumZSectors - 2];
                        endBlock = room.Blocks[portal.X + portal.NumXBlocks - 1, room.NumZSectors - 2];

                        xMin = portal.X;
                        xMax = portal.X + portal.NumXBlocks;
                        yMin1 = startBlock.QAFaces[0];
                        yMax1 = startBlock.WSFaces[0] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[1];
                        yMax2 = endBlock.WSFaces[1] + room.Ceiling;
                        zMin = room.NumZSectors - 1;
                        zMax = room.NumZSectors - 1;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int x = xMin; x < xMax; x++)
                        {
                            Block currentBlock = room.Blocks[x, room.NumZSectors - 2];
                            Block facingBlock = room.Blocks[x, room.NumZSectors - 1];

                            y1 = Math.Max(facingBlock.QAFaces[3], currentBlock.QAFaces[0]);
                            y2 = Math.Min(facingBlock.WSFaces[3], currentBlock.WSFaces[0]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[xMax - 1, room.NumZSectors - 2];
                        Block lastFacingBlock = room.Blocks[xMax - 1, room.NumZSectors - 1];

                        y1 = Math.Max(lastFacingBlock.QAFaces[2], lastBlock.QAFaces[1]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[2], lastBlock.WSFaces[1]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.East)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = -1;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[room.NumXSectors - 2, portal.Z + portal.NumZBlocks - 1];
                        endBlock = room.Blocks[room.NumXSectors - 2, portal.Z];

                        xMin = room.NumXSectors - 1;
                        xMax = room.NumXSectors - 1;
                        yMin1 = startBlock.QAFaces[1];
                        yMax1 = startBlock.WSFaces[1] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[2];
                        yMax2 = endBlock.WSFaces[2] + room.Ceiling;
                        zMin = portal.Z + portal.NumZBlocks;
                        zMax = portal.Z;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int z = zMax; z < zMin; z++)
                        {
                            Block currentBlock = room.Blocks[room.NumXSectors - 2, z];
                            Block facingBlock = room.Blocks[room.NumXSectors - 1, z];

                            y1 = Math.Max(facingBlock.QAFaces[0], currentBlock.QAFaces[1]);
                            y2 = Math.Min(facingBlock.WSFaces[0], currentBlock.WSFaces[1]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[room.NumXSectors - 2, zMin - 1];
                        Block lastFacingBlock = room.Blocks[room.NumXSectors - 1, zMin - 1];

                        y1 = Math.Max(lastFacingBlock.QAFaces[3], lastBlock.QAFaces[2]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[3], lastBlock.WSFaces[2]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024);
                        newPortal.Vertices[2].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024);
                        newPortal.Vertices[0].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.South)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = 1;

                        startBlock = room.Blocks[portal.X + portal.NumXBlocks - 1, 1];
                        endBlock = room.Blocks[portal.X, 1];

                        xMin = portal.X + portal.NumXBlocks;
                        xMax = portal.X;
                        yMin1 = startBlock.QAFaces[2];
                        yMax1 = startBlock.WSFaces[2] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[3];
                        yMax2 = endBlock.WSFaces[3] + room.Ceiling;
                        zMin = 1;
                        zMax = 1;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int x = xMax; x < xMin; x++)
                        {
                            Block currentBlock = room.Blocks[x, 1];
                            Block facingBlock = room.Blocks[x, 0];

                            y1 = Math.Max(facingBlock.QAFaces[1], currentBlock.QAFaces[2]);
                            y2 = Math.Min(facingBlock.WSFaces[1], currentBlock.WSFaces[2]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[xMin - 1, 1];
                        Block lastFacingBlock = room.Blocks[xMin - 1, 0];

                        y1 = Math.Max(lastFacingBlock.QAFaces[0], lastBlock.QAFaces[3]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[0], lastBlock.WSFaces[3]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f - 1.0f);

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f - 1.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f - 1.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f - 1.0f);
                    }
                    else if (portal.Direction == PortalDirection.West)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 1;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[1, portal.Z];
                        endBlock = room.Blocks[1, portal.Z + portal.NumZBlocks - 1];

                        xMin = 1;
                        xMax = 1;
                        yMin1 = startBlock.QAFaces[0];
                        yMax1 = startBlock.WSFaces[0] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[3];
                        yMax2 = endBlock.WSFaces[3] + room.Ceiling;
                        zMin = portal.Z;
                        zMax = portal.Z + portal.NumZBlocks;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int z = zMin; z < zMax; z++)
                        {
                            Block currentBlock = room.Blocks[1, z];
                            Block facingBlock = room.Blocks[0, z];

                            y1 = Math.Max(facingBlock.QAFaces[2], currentBlock.QAFaces[3]);
                            y2 = Math.Min(facingBlock.WSFaces[2], currentBlock.WSFaces[3]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[1, zMax - 1];
                        Block lastFacingBlock = room.Blocks[0, zMax - 1];

                        y1 = Math.Max(lastFacingBlock.QAFaces[1], lastBlock.QAFaces[0]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[1], lastBlock.WSFaces[0]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024.0f - 1.0f);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024.0f - 1.0f);
                        newPortal.Vertices[2].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024.0f - 1.0f);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024.0f - 1.0f);
                        newPortal.Vertices[0].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.Floor)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = -1;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[portal.X, portal.Z];
                        endBlock = room.Blocks[portal.X, portal.Z + portal.NumZBlocks - 1];

                        xMin = portal.X;
                        xMax = portal.X + portal.NumXBlocks;
                        int y = room.GetLowestCorner();
                        zMin = portal.Z;
                        zMax = portal.Z + portal.NumZBlocks;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.Ceiling)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = 1;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[portal.X, portal.Z];
                        endBlock = room.Blocks[portal.X, portal.Z + portal.NumZBlocks - 1];

                        xMin = portal.X;
                        xMax = portal.X + portal.NumXBlocks;
                        int y = room.GetHighestCorner();
                        zMin = portal.Z + portal.NumZBlocks;
                        zMax = portal.Z;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }

                    tempPortals.Add(newPortal);
                }

                newRoom.NumPortals = (ushort)tempPortals.Count;
                newRoom.Portals = tempPortals.ToArray();

                newRoom.Sectors = new tr_room_sector[room.NumXSectors * room.NumZSectors];
                newRoom.AuxSectors = new tr_sector_aux[room.NumXSectors, room.NumZSectors];

                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        tr_room_sector sector = new tr_room_sector();
                        tr_sector_aux aux = new Compilers.LevelCompilerTR4.tr_sector_aux();

                        byte sound = (byte)newRoom.TextureSounds[x, z];

                       /* if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 || room.Blocks[x, z].Type == BlockType.Wall ||
                            Math.Abs(room.Blocks[x, z].FloorSlopeX) > 2 || Math.Abs(room.Blocks[x, z].FloorSlopeZ) > 2)
                        {*/
                            sector.BoxIndex = 0x7ff6;
                      /*  }*
                        else
                        {
                            sector.BoxIndex = sound;
                        }*/

                        sector.FloorDataIndex = 0;

                        if (room.Blocks[x, z].FloorPortal >= 0)
                            sector.RoomBelow = (byte)_roomsIdTable[_editor.Level.Portals[room.Blocks[x, z].FloorPortal].AdjoiningRoom];
                        else
                            sector.RoomBelow = 0xff;

                        if (room.Blocks[x, z].CeilingPortal >= 0)
                            sector.RoomAbove = (byte)_roomsIdTable[_editor.Level.Portals[room.Blocks[x, z].CeilingPortal].AdjoiningRoom];
                        else
                            sector.RoomAbove = 0xff;

                        if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 ||
                            room.Blocks[x, z].Type == BlockType.BorderWall || room.Blocks[x, z].Type == BlockType.Wall)
                        {
                            sector.Floor = (sbyte)(-room.Position.Y - room.GetHighestFloorCorner(x, z));
                            sector.Ceiling = (sbyte)(-room.Position.Y - room.Ceiling - room.GetLowestCeilingCorner(x, z));
                        }
                        else
                        {
                            sector.Floor = (sbyte)(-room.Position.Y - room.GetHighestFloorCorner(x, z));
                            sector.Ceiling = (sbyte)(-room.Position.Y - room.Ceiling - room.GetLowestCeilingCorner(x, z));
                        }

                        //Setup some aux data for box generation
                        if (room.Blocks[x, z].Type == BlockType.BorderWall) aux.BorderWall = true;
                        if ((room.Blocks[x, z].Flags & BlockFlags.Monkey) != 0) aux.Monkey = true;
                        if ((room.Blocks[x, z].Flags & BlockFlags.Box) != 0) aux.Box = true;
                        if ((room.Blocks[x, z].Flags & BlockFlags.NotWalkableFloor) != 0) aux.NotWalkableFloor = true;
                        if (!room.FlagWater && (Math.Abs(room.Blocks[x, z].FloorSlopeX) == 1 || Math.Abs(room.Blocks[x, z].FloorSlopeX) == 2 ||
                            Math.Abs(room.Blocks[x, z].FloorSlopeZ) == 1 || Math.Abs(room.Blocks[x, z].FloorSlopeZ) == 2)) aux.SoftSlope = true;
                        if (!room.FlagWater && (Math.Abs(room.Blocks[x, z].FloorSlopeX) > 2 || Math.Abs(room.Blocks[x, z].FloorSlopeZ) > 2)) aux.HardSlope = true;
                        if (room.Blocks[x, z].Type == BlockType.Wall) aux.Wall = true;

                        // I must setup portal only if current sector is not solid and opacity if different from 1
                        if (room.Blocks[x, z].FloorPortal != -1)
                        {
                            if ((!room.Blocks[x, z].IsFloorSolid && room.Blocks[x, z].FloorOpacity != PortalOpacity.Opacity1) ||
                                (room.Blocks[x, z].IsFloorSolid && room.Blocks[x, z].NoCollisionFloor))
                            {
                                Portal portal = _editor.Level.Portals[room.Blocks[x, z].FloorPortal];
                                sector.RoomBelow = (byte)_roomsIdTable[portal.AdjoiningRoom];
                            }
                            else
                            {
                                sector.RoomBelow = 255;
                            }
                        }
                        else
                        {
                            sector.RoomBelow = 255;
                        }

                        if ((room.Blocks[x, z].FloorPortal != -1 && room.Blocks[x, z].FloorOpacity != PortalOpacity.Opacity1 && !room.Blocks[x, z].IsFloorSolid))
                        {
                            aux.Portal = true;
                            aux.FloorPortal = room.Blocks[x, z].FloorPortal;
                        }
                        else
                        {
                            aux.FloorPortal = -1;
                        }

                        aux.IsFloorSolid = room.Blocks[x, z].IsFloorSolid;

                        aux.MeanFloorHeight = (sbyte)(-room.Position.Y - room.GetMeanFloorHeight(x, z));

                        if ((room.Blocks[x, z].CeilingPortal != -1 && room.Blocks[x, z].CeilingOpacity != PortalOpacity.Opacity1))
                        {
                            aux.CeilingPortal = room.Blocks[x, z].CeilingPortal;
                        }
                        else
                        {
                            aux.CeilingPortal = -1;
                        }

                        if (room.Blocks[x, z].WallPortal != -1 && room.Blocks[x, z].WallOpacity != PortalOpacity.Opacity1)
                            aux.WallPortal = _roomsIdTable[(int)_editor.Level.Portals[room.Blocks[x, z].WallPortal].AdjoiningRoom];
                        else
                            aux.WallPortal = -1;

                        aux.LowestFloor = (sbyte)(-room.Position.Y - room.GetLowestFloorCorner(x, z));
                        short q0 = room.Blocks[x, z].QAFaces[0];
                        short q1 = room.Blocks[x, z].QAFaces[1];
                        short q2 = room.Blocks[x, z].QAFaces[2];
                        short q3 = room.Blocks[x, z].QAFaces[3];

                        if (!Room.IsQuad(x, z, q0, q1, q2, q3, true) && room.Blocks[x, z].FloorSlopeX == 0 && room.Blocks[x, z].FloorSlopeZ == 0)
                        {
                            if (room.Blocks[x, z].RealSplitFloor == 0)
                            {
                                aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(room.Blocks[x, z].QAFaces[0], room.Blocks[x, z].QAFaces[2]));
                            }
                            else
                            {
                                aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(room.Blocks[x, z].QAFaces[1], room.Blocks[x, z].QAFaces[3]));
                            }
                        }

                        newRoom.AuxSectors[x, z] = aux;
                        newRoom.Sectors[room.NumZSectors * x + z] = sector;
                    }
                }

                List<tr_room_staticmesh> tempStaticMeshes = new List<tr_room_staticmesh>();

                for (int j = 0; j < room.StaticMeshes.Count; j++)
                {
                    tr_room_staticmesh sm = new tr_room_staticmesh();
                    StaticMeshInstance instance = (StaticMeshInstance)_editor.Level.Objects[room.StaticMeshes[j]];

                    sm.X = (uint)(newRoom.Info.X + instance.Position.X);
                    sm.Y = (uint)(newRoom.Info.YBottom - instance.Position.Y);
                    sm.Z = (uint)(newRoom.Info.Z + instance.Position.Z);

                    sm.Rotation = (ushort)(instance.Rotation / 45 * 8192);
                    sm.ObjectID = (ushort)instance.Model.ObjectID;
                    sm.Intensity1 = Pack24BitColorTo16bit(instance.Color);
                    sm.Intensity2 = 0;

                    tempStaticMeshes.Add(sm);
                }

                newRoom.NumStaticMeshes = (ushort)tempStaticMeshes.Count;
                newRoom.StaticMeshes = tempStaticMeshes.ToArray();

                List<tr4_room_light> tempLights = new List<tr4_room_light>();
                for (int j = 0; j < room.Lights.Count; j++)
                {
                    tr4_room_light newLight = new tr4_room_light();
                    Light light = room.Lights[j];

                    if (light.Type == LightType.Effect) continue;

                    newLight.X = (int)(newRoom.Info.X + light.Position.X);
                    newLight.Y = (int)(-light.Position.Y + newRoom.Info.YBottom);
                    newLight.Z = (int)(newRoom.Info.Z + light.Position.Z);

                    newLight.Color = new tr_color();
                    newLight.Color.Red = light.Color.R;
                    newLight.Color.Green = light.Color.G;
                    newLight.Color.Blue = light.Color.B;

                    newLight.Intensity = (ushort)(((short)(Math.Abs(light.Intensity) * 31.0f) << 8) | 0x00ff);

                    if (light.Type == LightType.Light)
                    {
                        // Point light
                        newLight.LightType = 1;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
                    }
                    else if (light.Type == LightType.Shadow)
                    {
                        // Point shadow
                        newLight.LightType = 3;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
                    }
                    else if (light.Type == LightType.Spot)
                    {
                        // Spot light
                        newLight.LightType = 2;
                        newLight.In = (float)Math.Cos(MathUtil.DegreesToRadians(light.In));
                        newLight.Out = (float)Math.Cos(MathUtil.DegreesToRadians(light.Out));
                        newLight.Length = light.Len * 1024.0f;
                        newLight.CutOff = light.Cutoff * 1024.0f;
                        newLight.DirectionX = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                        newLight.DirectionY = (float)(Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                        newLight.DirectionZ = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));
                    }
                    else if (light.Type == LightType.Sun)
                    {
                        // Sun light
                        newLight.LightType = 0;
                        newLight.In = 0;
                        newLight.Out = 0;
                        newLight.Length = 0;
                        newLight.CutOff = 0;
                        newLight.DirectionX = -(float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                        newLight.DirectionY = -(float)(-Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                        newLight.DirectionZ = -(float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));
                    }
                    else if (light.Type == LightType.FogBulb)
                    {
                        // Fog bulb
                        newLight.LightType = 4;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
                    }

                    tempLights.Add(newLight);
                }

                newRoom.NumLights = (ushort)tempLights.Count;
                newRoom.Lights = tempLights.ToArray();

                Rooms[_roomsIdTable[i]] = newRoom;
            }

            ReportProgress(25, "    Number of rooms: " + Rooms.Length);
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
                tr_mesh newMesh = new Compilers.LevelCompilerTR4.tr_mesh();

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

                if (meshSize % 4 != 0) meshSize += 2;

                tempMeshesPointers.Add((uint)totalMeshSize);
                totalMeshSize += meshSize;

                tempMeshes.Add(newMesh);
            }

            NumMeshes = (uint)tempMeshes.Count;
            Meshes = tempMeshes.ToArray();
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
                    uint objectID = ((MoveableInstance)_editor.Level.Objects.ElementAt(i).Value).Model.ObjectID;

                    if (objectID >= 398 && objectID <= 406)
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

            List<tr_item> tempItems = new List<Compilers.LevelCompilerTR4.tr_item>();
            List<tr_ai_item> tempAIObjects = new List<Compilers.LevelCompilerTR4.tr_ai_item>();

            for (int i = 0; i < _moveablesTable.Count; i++)
            {
                //  if (i == 79) continue;
                tr_item item = new Compilers.LevelCompilerTR4.tr_item();
                MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[_moveablesTable.ElementAt(i).Key];
                tr_room newRoom = Rooms[_roomsIdTable[instance.Room]];

                item.X = (int)(Rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                item.Y = (int)(Rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                item.Z = (int)(Rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                item.ObjectID = (short)instance.Model.ObjectID;
                item.Room = (short)_roomsIdTable[instance.Room];
                item.Angle = (short)(instance.Rotation / 45 * 8192);
                item.Intensity1 = -1;
                item.Intensity2 = (short)instance.OCB;

                if (instance.ClearBody) item.Flags |= 0x80;
                if (instance.Invisible) item.Flags |= 0x100;

                ushort mask = 0;

                if (instance.Bits[0]) mask |= 0x01;
                if (instance.Bits[1]) mask |= 0x02;
                if (instance.Bits[2]) mask |= 0x04;
                if (instance.Bits[3]) mask |= 0x08;
                if (instance.Bits[4]) mask |= 0x10;

                item.Flags |= (ushort)(mask << 9);

                tempItems.Add(item);
            }

            NumItems = (uint)tempItems.Count;
            Items = tempItems.ToArray();

            for (int i = 0; i < _aiObjectsTable.Count; i++)
            {
                tr_ai_item item = new Compilers.LevelCompilerTR4.tr_ai_item();
                MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[_aiObjectsTable.ElementAt(i).Key];
                tr_room newRoom = Rooms[_roomsIdTable[instance.Room]];

                item.X = (int)(Rooms[_roomsIdTable[instance.Room]].Info.X + instance.Position.X);
                item.Y = (int)(Rooms[_roomsIdTable[instance.Room]].Info.YBottom - instance.Position.Y);
                item.Z = (int)(Rooms[_roomsIdTable[instance.Room]].Info.Z + instance.Position.Z);

                item.ObjectID = (ushort)instance.Model.ObjectID;
                item.Room = (ushort)_roomsIdTable[instance.Room];
                short angle = (short)(instance.Rotation);
                item.Angle = (short)(angle / 45 * 8192);
                item.OCB = (ushort)instance.OCB;

                ushort mask = 0;

                if (instance.Bits[0]) mask |= 0x01;
                if (instance.Bits[1]) mask |= 0x02;
                if (instance.Bits[2]) mask |= 0x04;
                if (instance.Bits[3]) mask |= 0x08;
                if (instance.Bits[4]) mask |= 0x10;

                item.Flags |= (ushort)(mask << 1);

                tempAIObjects.Add(item);
            }

            NumAiItems = (uint)tempAIObjects.Count;
            AiItems = tempAIObjects.ToArray();

            ReportProgress(30, "    Number of items: " + NumItems);
            ReportProgress(31, "    Number of AI objects: " + NumAiItems);
        }

        private void BuildFloorData()
        {
            ReportProgress(70, "Building floordata");

            // Initialize the floordata list and add the dummy entry for walls and sectors without particular things
            List<ushort> tempFloorData = new List<ushort>();
            tempFloorData.Add(0 | 0x8000);

            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                Room room = _editor.Level.Rooms[i];
                if (room == null) continue;
                int idNewRoom = _roomsIdTable[i];

                // Get all portals
                List<Portal> portals = new List<Portal>();
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        if (room.Blocks[x, z].CeilingPortal != -1 && room.Blocks[x, z].CeilingOpacity != PortalOpacity.Opacity1)
                        {
                            portals.Add(_level.Portals[room.Blocks[x, z].CeilingPortal]);
                        }
                    }
                }

                if (i==110)
                {
                    int ghghg = 0;
                }

                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        tr_room_sector sector = GetSector(idNewRoom, x, z);
                        Block block = room.Blocks[x, z];

                        if (i==0 && x == 3 && z ==12)
                        {
                            int ggg = 0;
                        }

                        ushort baseFloorData = (ushort)tempFloorData.Count;

                        // If a sector is a wall and this room is a water room, 
                        // I must check before if on the neighbour sector there's a ceiling portal 
                        // because eventually I must add a vertical portal
                        int isWallWithCeilingPortal = -1;
                        if (portals.Count != 0)
                        {
                            // Find a suitable portal
                            for (int j = 0; j < portals.Count; j++)
                            {
                                if (x >= portals[j].X - 1 && x <= portals[j].X + portals[j].NumXBlocks + 1 &&
                                    z >= portals[j].Z - 1 && z <= portals[j].Z + portals[j].NumZBlocks + 1)
                                {
                                    Room adjoining = _level.Rooms[portals[j].AdjoiningRoom];
                                    int x2 = (int)(room.Position.X + x - adjoining.Position.X);
                                    int z2 = (int)(room.Position.Z + z - adjoining.Position.Z);

                                    if (x2 < 0 || x2 > adjoining.NumXSectors - 1 || z2 < 0 || z2 > adjoining.NumZSectors - 1) continue;

                                    BlockType blockType = adjoining.Blocks[x2, z2].Type;
                                    DiagonalSplit adjoiningSplit = adjoining.Blocks[x2, z2].FloorDiagonalSplit;
                                    DiagonalSplit currentSplit = block.FloorDiagonalSplit;

                                    if ((x2 > 1 || z2 > 1 || x2 < adjoining.NumXSectors - 1 || z2 < adjoining.NumZSectors - 1) &&
                                        !((blockType == BlockType.Wall && adjoiningSplit == DiagonalSplit.None)
                                        || blockType == BlockType.BorderWall))
                                    {
                                        isWallWithCeilingPortal = _roomsIdTable[portals[j].AdjoiningRoom];
                                        break;
                                    }
                                }
                            }
                        }

                        // If sector is a wall with a ceiling portal on it or near it
                        if (block.WallPortal == -1 && isWallWithCeilingPortal != -1 && ((block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None) || room.Blocks[x, z].Type == BlockType.BorderWall))
                        {
                            ushort data1 = 0x8001;
                            ushort data2 = (ushort)isWallWithCeilingPortal;

                            tempFloorData.Add(data1);
                            tempFloorData.Add(data2);

                            // Update current sector
                            sector.FloorDataIndex = baseFloorData;

                            SaveSector(idNewRoom, x, z, sector);

                            continue;
                        }

                        // If sector is a border wall without portals or a normal wall
                        if ((block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None) || (block.Type == BlockType.BorderWall && block.WallPortal == -1))
                        {
                            sector.FloorDataIndex = 0;
                            sector.Floor = -127;
                            sector.Ceiling = -127;

                            SaveSector(idNewRoom, x, z, sector);
                            continue;
                        }

                        // If sector is a floor portal
                        if (block.FloorPortal >= 0)
                        {
                            // I must setup portal only if current sector is not solid and opacity if different from 1
                            if ((!block.IsFloorSolid && block.FloorOpacity != PortalOpacity.Opacity1) || (block.IsFloorSolid && block.NoCollisionFloor))
                            {
                                Portal portal = _editor.Level.Portals[block.FloorPortal];
                                sector.RoomBelow = (byte)_roomsIdTable[portal.AdjoiningRoom];
                            }
                            else
                            {
                                sector.RoomBelow = 255;
                            }
                        }

                        // If sector is a ceiling portal
                        if (block.CeilingPortal >= 0)
                        {
                            // I must setup portal only if current sector is not solid and opacity if different from 1
                            if ((!block.IsCeilingSolid && block.CeilingOpacity != PortalOpacity.Opacity1) || (block.IsCeilingSolid && block.NoCollisionCeiling))
                            {
                                Portal portal = _editor.Level.Portals[block.CeilingPortal];
                                sector.RoomAbove = (byte)_roomsIdTable[portal.AdjoiningRoom];
                            }
                            else
                            {
                                sector.RoomAbove = 255;
                            }
                        }

                        // If sector is a wall portal
                        if (block.WallPortal >= 0)
                        {
                            Portal portal = _editor.Level.Portals[block.WallPortal];

                            // Only if the portal is not a Toggle Opacity 1
                            if (block.WallOpacity != PortalOpacity.Opacity1)
                            {
                                ushort data1 = 0x8001;
                                ushort data2 = (ushort)_roomsIdTable[portal.AdjoiningRoom];

                                tempFloorData.Add(data1);
                                tempFloorData.Add(data2);

                                // Update current sector
                                sector.FloorDataIndex = baseFloorData;
                                SaveSector(idNewRoom, x, z, sector);
                            }
                            else
                            {
                                sector.Floor = -127;
                                sector.Ceiling = -127;
                                SaveSector(idNewRoom, x, z, sector);
                            }

                            continue;
                        }

                        // From this point, I will never bypass the loop and something must be there so I surely add at least one
                        // floordata value
                        sector.FloorDataIndex = baseFloorData;
                        List<ushort> tempCodes = new List<ushort>();
                        ushort lastFloorDataFunction = (ushort)tempCodes.Count;

                        // If sector is Death
                        if ((block.Flags & BlockFlags.Death) == BlockFlags.Death)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x05);
                        }

                        // If sector has a floor slope
                        if (block.FloorSlopeX != 0 || block.FloorSlopeZ != 0)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x02);

                            sector.Floor = (sbyte)(-room.Position.Y - room.GetHighestFloorCorner(x, z));

                            ushort slope = (ushort)(((block.FloorSlopeZ) << 8) | ((block.FloorSlopeX) & 0xff));

                            tempCodes.Add(slope);
                        }

                        // If sector has a ceiling slope
                        if (block.CeilingSlopeX != 0 || block.CeilingSlopeZ != 0)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x03);

                            ushort slope = (ushort)(((block.CeilingSlopeZ) << 8) | ((block.CeilingSlopeX) & 0xff));

                            tempCodes.Add(slope);
                        }

                        // Now begins the triangulation for floor and ceiling
                        // It's a very long and hard task
                        if (block.FloorDiagonalSplit != DiagonalSplit.None)
                        {
                            int q0 = block.QAFaces[0];
                            int q1 = block.QAFaces[1];
                            int q2 = block.QAFaces[2];
                            int q3 = block.QAFaces[3];

                            int w0 = block.WSFaces[0];
                            int w1 = block.WSFaces[1];
                            int w2 = block.WSFaces[2];
                            int w3 = block.WSFaces[3];

                            // The real floor split of the sector
                            int split = block.RealSplitFloor;
                            int function = 0;

                            int t1 = 0;
                            int t2 = 0;

                            int h00 = 0;
                            int h01 = 0;
                            int h10 = 0;
                            int h11 = 0;

                            // First, we fix the sector height
                            if (block.Type == BlockType.Wall)
                                sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - 0x0f);
                            else
                                sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - room.GetHighestFloorCorner(x, z));

                            if (block.FloorDiagonalSplit == DiagonalSplit.NE || block.FloorDiagonalSplit == DiagonalSplit.SW)
                            {
                                lastFloorDataFunction = (ushort)tempCodes.Count;

                                if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                {
                                    if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                                    {
                                        function = 0x0c;
                                    }
                                    else
                                    {
                                        function = 0x0b;
                                    }
                                }
                                else
                                {
                                    function = 0x07;
                                }

                                // Diagonal steps and walls are the simplest case. All corner heights are zero 
                                // except eventually the right angle on the top face. Corrections t1 and t2 
                                // are simple to calculate
                                if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                                {
                                    int lowCorner = q1;
                                    int highCorner = q3;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 0;
                                        t2 = 15 - block.QAFaces[1]; // Diagonal wall max height minus the height of the lower right angle

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = (q2 > q3 ? q3 - q2 : 0);
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = Math.Abs(q3 - q2);
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                }
                                else
                                {
                                    int lowCorner = q3;
                                    int highCorner = q1;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 15 - block.QAFaces[3];
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = q1 - q2;
                                    }
                                }
                            }
                            else
                            {
                                lastFloorDataFunction = (ushort)tempCodes.Count;

                                if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                {
                                    if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                                    {
                                        function = 0x0d;
                                    }
                                    else
                                    {
                                        function = 0x0e;
                                    }
                                }
                                else
                                {
                                    function = 0x08;
                                }

                                if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                                {
                                    int lowCorner = q0;
                                    int highCorner = q2;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 15 - block.QAFaces[0];
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = q2 - q1;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                }
                                else
                                {
                                    int lowCorner = q2;
                                    int highCorner = q0;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 0;
                                        t2 = 15 - block.QAFaces[2];

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = 0;
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = q0 - q1;
                                        h11 = 0;
                                    }
                                }
                            }

                            ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                            ushort code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                            tempCodes.Add(code1);
                            tempCodes.Add(code2);
                        }
                        else
                        {
                            if (block.FloorSlopeX == 0 && block.FloorSlopeZ == 0)
                            {
                                int q0 = block.QAFaces[0];
                                int q1 = block.QAFaces[1];
                                int q2 = block.QAFaces[2];
                                int q3 = block.QAFaces[3];

                                // We have not a slope, so if this is not a horizontal square then we have triangulation
                                if (!Room.IsQuad(x, z, q0, q1, q2, q3, true))
                                {
                                    // First, we fix the sector height
                                    sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - room.GetHighestFloorCorner(x, z));

                                    // Then we have to find the axis of the triangulation
                                    int min = room.GetLowestFloorCorner(x, z);
                                    int max = room.GetHighestFloorCorner(x, z);

                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    // Corner heights
                                    int h10 = q2 - min;
                                    int h00 = q3 - min;
                                    int h01 = q0 - min;
                                    int h11 = q1 - min;

                                    int maxCorner = Math.Max(h00, Math.Max(h01, Math.Max(h10, h11)));

                                    // Flip the Y axis
                                    min = -min;
                                    max = -max;

                                    int t1 = 0;
                                    int t2 = 0;

                                    // The real floor split of the sector
                                    int split = block.RealSplitFloor;
                                    int function = 0;

                                    if (split == 0)
                                    {
                                        if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                        {
                                            if (q0 == q1 && q1 == q2 && q2 == q0)
                                            {
                                                function = 0x0c;
                                            }
                                            else
                                            {
                                                function = 0x0b;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x07;
                                        }

                                        // Prepare four vectors that are vertices of a square from 0, 0 to 1024, 1024 and 
                                        // with variable corner heights. For calculating the right t1 and t2 values, we 
                                        // must know also the fourth point of the square that contains the triangle 
                                        // we are trying to correct. I simply intersect a very long ray with the plane 
                                        // passing through the triangle and I can obtain in this way the height of the fourth corner.
                                        // This height then is used in different ways
                                        Vector3 p00 = new Vector3(0, h00 * 256, 0);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 1024);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 0);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 1024);

                                        // In triangle collisions, everything is relative to the highest corner
                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h00 < Math.Min(h10, h01) && h11 < Math.Min(h10, h01))
                                        {
                                            // Case 1: both triangles have their right angles below the diagonal ( /D\ )
                                            Plane pl1 = new Plane(p01, p10, p00);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            //int maxTriangle1 = Math.Max(Math.Max(h01, h00), h10);

                                            // Correction is the max height of the sector minus the height of the fourth point
                                            t2 = (int)(maxHeight - distance1) & 0x1f;

                                            Plane pl2 = new Plane(p10, p01, p11);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            //int maxTriangle2 = Math.Max(Math.Max(h11, h10), h01);

                                            // Correction is the max height of the sector minus the height of the fourth point
                                            t1 = (int)(maxHeight - distance2) & 0x1f;
                                        }
                                        else if ((h01 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h11 < h00)
                                        {
                                            // Case 2: h00 is highest corner and h11 is lower than h00. Typical example, when you raise of one click 
                                            // one corner of a sector (simplest case)
                                            Plane p = new Plane(p01, p11, p10);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h10);

                                            // There are two cases (1.jpg and 2.jpg). The fourth point height can be lower than max height 
                                            // of the triangle or higher.
                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                        else if ((h01 == maxHeight || h11 == maxHeight || h10 == maxHeight) && h00 < h11)
                                        {
                                            // Case 3: similar to case 2, but the opposite 
                                            Plane p = new Plane(p01, p10, p00);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t2 = 0;
                                                t1 = (int)(maxHeight - maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t2 = 0;
                                                t1 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                        {
                                            if (q3 == q0 && q0 == q1 && q1 == q3)
                                            {
                                                function = 0x0d;
                                            }
                                            else
                                            {
                                                function = 0x0e;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x08;
                                        }

                                        Vector3 p00 = new Vector3(0, h00 * 256, 0);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 1024);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 0);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 1024);

                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h01 < Math.Min(h00, h11) && h10 < Math.Min(h00, h11))
                                        {
                                            Plane pl1 = new Plane(p11, p10, p00);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            int maxTriangle1 = Math.Max(Math.Max(h11, h00), h10);

                                            t2 = (int)(maxHeight - distance1) & 0x1f;

                                            Plane pl2 = new Plane(p00, p01, p11);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            int maxTriangle2 = Math.Max(Math.Max(h11, h00), h01);

                                            t1 = (int)(maxHeight - distance2) & 0x1f;
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h01 < h10)
                                        {
                                            Plane p = new Plane(p01, p11, p00);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h00);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = (int)(maxHeight - maxTriangle) & 0x1f;
                                                t2 = 0;
                                            }
                                            else
                                            {
                                                t1 = (int)(maxHeight - distance) & 0x1f;
                                                t2 = 0;
                                            }
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h01 == maxHeight) && h10 < h01)
                                        {
                                            Plane p = new Plane(p11, p10, p00);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h11, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                    }

                                    // Now build the floordata codes
                                    ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                    ushort code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                                    tempCodes.Add(code1);
                                    tempCodes.Add(code2);
                                }
                            }
                        }

                        if (block.CeilingDiagonalSplit != DiagonalSplit.None)
                        {
                            if (block.Type != BlockType.Wall)
                            {
                                int q0 = block.QAFaces[0];
                                int q1 = block.QAFaces[1];
                                int q2 = block.QAFaces[2];
                                int q3 = block.QAFaces[3];

                                int w0 = block.WSFaces[0];
                                int w1 = block.WSFaces[1];
                                int w2 = block.WSFaces[2];
                                int w3 = block.WSFaces[3];

                                // The real floor split of the sector
                                int split = block.RealSplitCeiling;
                                int function = 0;

                                int t1 = 0;
                                int t2 = 0;

                                int h00 = 0;
                                int h01 = 0;
                                int h10 = 0;
                                int h11 = 0;

                                // First, we fix the sector height
                                if (block.Type == BlockType.Wall)
                                    sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - 0x0f);
                                else
                                    sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - room.GetHighestFloorCorner(x, z));

                                if (block.CeilingDiagonalSplit == DiagonalSplit.NE || block.CeilingDiagonalSplit == DiagonalSplit.SW)
                                {
                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                    {
                                        if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                                        {
                                            function = 0x10;
                                        }
                                        else
                                        {
                                            function = 0x0f;
                                        }
                                    }
                                    else
                                    {
                                        function = 0x09;
                                    }

                                    if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                                    {
                                        int lowCorner = w1;
                                        int highCorner = w3;


                                        t1 = 0;
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = w3 - w2;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        int lowCorner = w3;
                                        int highCorner = w1;

                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = w1 - w2;
                                    }
                                }
                                else
                                {
                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                    {
                                        if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                                        {
                                            function = 0x11;
                                        }
                                        else
                                        {
                                            function = 0x12;
                                        }
                                    }
                                    else
                                    {
                                        function = 0x0a;
                                    }

                                    if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                                    {
                                        int lowCorner = w0;
                                        int highCorner = w2;


                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = w2 - w1;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        int lowCorner = w2;
                                        int highCorner = w0;

                                        t1 = 0;
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = w0 - w1;
                                        h11 = 0;
                                    }
                                }

                                ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                ushort code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                                tempCodes.Add(code1);
                                tempCodes.Add(code2);
                            }
                        }
                        else
                        {
                            if (block.CeilingSlopeX == 0 && block.CeilingSlopeZ == 0)
                            {
                                int w0 = block.WSFaces[0];
                                int w1 = block.WSFaces[1];
                                int w2 = block.WSFaces[2];
                                int w3 = block.WSFaces[3];

                                // We have not a slope, so if this is not a horizontal square then we have triangulation
                                if (!Room.IsQuad(x, z, w0, w1, w2, w3, true))
                                {
                                    // We have to find the axis of the triangulation
                                    int min = room.GetLowestCeilingCorner(x, z);
                                    int max = room.GetHighestCeilingCorner(x, z);

                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    // Corner heights
                                    int h10 = max - w2;
                                    int h00 = max - w3;
                                    int h01 = max - w0;
                                    int h11 = max - w1;

                                    int maxCorner = Math.Max(h00, Math.Max(h01, Math.Max(h10, h11)));

                                    // Flip the Y axis
                                    min = -min;
                                    max = -max;

                                    int t1 = 0;
                                    int t2 = 0;

                                    // The real ceiling split of the sector
                                    int split = block.RealSplitCeiling;
                                    int function = 0;

                                    // Now, for each of the two possible splits, apply the algorithm described by meta2tr and 
                                    // TRosettaStone 3. I've simply managed some cases by hand. The difficult task is to 
                                    // decide if apply the height correction to both triangles or just one of them.
                                    // Function must be decided looking at portals.

                                    if (split == 0)
                                    {
                                        if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                        {
                                            if (w0 == w1 && w1 == w2 && w2 == w0)
                                            {
                                                function = 0x10;
                                            }
                                            else
                                            {
                                                function = 0x0f;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x09;
                                        }

                                        Vector3 p00 = new Vector3(0, h00 * 256, 1024);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 0);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 1024);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 0);

                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h00 < Math.Min(h10, h01) && h11 < Math.Min(h10, h01))
                                        {
                                            Plane pl1 = new Plane(p01, p00, p10);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            int maxTriangle1 = Math.Max(Math.Max(h01, h00), h10);

                                            t2 = (int)(-maxHeight + distance1) & 0x1f;

                                            Plane pl2 = new Plane(p10, p11, p01);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            int maxTriangle2 = Math.Max(Math.Max(h11, h10), h01);

                                            t1 = (int)(-maxHeight + distance2) & 0x1f;
                                        }
                                        else if ((h01 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h11 < h00)
                                        {
                                            Plane p = new Plane(p01, p10, p11);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                        else if ((h01 == maxHeight || h11 == maxHeight || h10 == maxHeight) && h00 < h11)
                                        {
                                            Plane p = new Plane(p01, p00, p10);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t2 = 0;
                                                t1 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t2 = 0;
                                                t1 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                        {
                                            if (w3 == w0 && w0 == w1 && w1 == w3)
                                            {
                                                function = 0x11;
                                            }
                                            else
                                            {
                                                function = 0x12;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x0a;
                                        }

                                        Vector3 p00 = new Vector3(0, h00 * 256, 1024);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 0);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 1024);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 0);

                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h01 < Math.Min(h00, h11) && h10 < Math.Min(h00, h11))
                                        {
                                            Plane pl1 = new Plane(p11, p00, p10);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            int maxTriangle1 = Math.Max(Math.Max(h11, h00), h10);

                                            t2 = (int)(-maxHeight + distance1) & 0x1f;

                                            Plane pl2 = new Plane(p00, p11, p01);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            int maxTriangle2 = Math.Max(Math.Max(h11, h00), h01);

                                            t1 = (int)(-maxHeight + distance2) & 0x1f;
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h01 < h10)
                                        {
                                            Plane p = new Plane(p01, p00, p11);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h00);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                                t2 = 0;
                                            }
                                            else
                                            {
                                                t1 = (int)(-maxHeight + distance) & 0x1f;
                                                t2 = 0;
                                            }
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h01 == maxHeight) && h10 < h01)
                                        {
                                            Plane p = new Plane(p11, p00, p10);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h11, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                    }

                                    // Now build the floordata codes
                                    ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                    ushort code2 = (ushort)((h11) | (h01 << 4) | (h00 << 8) | (h10 << 12));

                                    tempCodes.Add(code1);
                                    tempCodes.Add(code2);
                                }
                            }
                        }

                        // If sector is Climbable
                        if (block.Climb[0] || block.Climb[1] || block.Climb[2] || block.Climb[3])
                        {
                            ushort climb = 0x06;

                            if (block.Climb[0]) climb |= (0x01 << 8);
                            if (block.Climb[1]) climb |= (0x02 << 8);
                            if (block.Climb[2]) climb |= (0x04 << 8);
                            if (block.Climb[3]) climb |= (0x08 << 8);

                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(climb);
                        }

                        // If sector is Death
                        if ((block.Flags & BlockFlags.Monkey) == BlockFlags.Monkey)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x13);
                        }

                        // If sector is Beetle
                        if ((block.Flags & BlockFlags.Beetle) == BlockFlags.Beetle)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x15);
                        }

                        // If sector is Trigger triggerer
                        if ((block.Flags & BlockFlags.TriggerTriggerer) == BlockFlags.TriggerTriggerer)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x14);
                        }

                        // Triggers
                        if (room.Blocks[x, z].Triggers.Count > 0)
                        {
                            int found = -1;
                            TriggerInstance trigger;

                            // First, I search a special trigger, if exists
                            for (int j = 0; j < room.Blocks[x, z].Triggers.Count; j++)
                            {
                                trigger = _editor.Level.Triggers[room.Blocks[x, z].Triggers[j]];

                                if (trigger.TriggerType == TriggerType.Trigger && found == -1)
                                {
                                    // Normal trigger can be used only if in the sector there aren't special triggers
                                    found = j;
                                    continue;
                                }

                                if (trigger.TriggerType != TriggerType.Trigger)
                                {
                                    // For now I use the first special trigger of the chain, ignoring the following triggers
                                    found = j;
                                    break;
                                }
                            }

                            List<int> tempTriggers = new List<int>();
                            tempTriggers.Add(room.Blocks[x, z].Triggers[found]);
                            for (int j = 0; j < room.Blocks[x, z].Triggers.Count; j++)
                            {
                                if (j != found) tempTriggers.Add(room.Blocks[x, z].Triggers[j]);
                            }

                            trigger = _editor.Level.Triggers[room.Blocks[x, z].Triggers[found]];

                            lastFloorDataFunction = (ushort)tempCodes.Count;

                            // Trigger type and setup are coming from the found trigger. Other triggers are needed onlt for action.
                            ushort trigger1 = 0x04;
                            if (trigger.TriggerType == TriggerType.Trigger) trigger1 |= (ushort)(0x00 << 8);
                            if (trigger.TriggerType == TriggerType.Pad) trigger1 |= (ushort)(0x01 << 8);
                            if (trigger.TriggerType == TriggerType.Switch) trigger1 |= (ushort)(0x02 << 8);
                            if (trigger.TriggerType == TriggerType.Key) trigger1 |= (ushort)(0x03 << 8);
                            if (trigger.TriggerType == TriggerType.Pickup) trigger1 |= (ushort)(0x04 << 8);
                            if (trigger.TriggerType == TriggerType.Heavy) trigger1 |= (ushort)(0x05 << 8);
                            if (trigger.TriggerType == TriggerType.Antipad) trigger1 |= (ushort)(0x06 << 8);
                            if (trigger.TriggerType == TriggerType.Combat) trigger1 |= (ushort)(0x07 << 8);
                            if (trigger.TriggerType == TriggerType.Dummy) trigger1 |= (ushort)(0x08 << 8);
                            if (trigger.TriggerType == TriggerType.Antitrigger) trigger1 |= (ushort)(0x09 << 8);
                            if (trigger.TriggerType == TriggerType.HeavySwitch) trigger1 |= (ushort)(0x0a << 8);
                            if (trigger.TriggerType == TriggerType.HeavyAntritrigger) trigger1 |= (ushort)(0x0b << 8);
                            if (trigger.TriggerType == TriggerType.Monkey) trigger1 |= (ushort)(0x0c << 8);

                            ushort triggerSetup = 0;
                            triggerSetup |= (ushort)(trigger.Timer & 0xff);
                            triggerSetup |= (ushort)(trigger.OneShot ? 0x100 : 0);
                            triggerSetup |= (ushort)(trigger.Bits[0] ? (0x01 << 13) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[1] ? (0x01 << 12) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[2] ? (0x01 << 11) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[3] ? (0x01 << 10) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[4] ? (0x01 << 9) : 0);

                            tempCodes.Add(trigger1);
                            tempCodes.Add(triggerSetup);

                            for (int j = 0; j < tempTriggers.Count; j++)
                            {
                                trigger = _editor.Level.Triggers[tempTriggers[j]];

                                ushort trigger2 = 0;
                                ushort trigger3 = 0;

                                if (trigger.TargetType == TriggerTargetType.Object)
                                {
                                    // Trigger for object
                                    int item = trigger.Target;
                                    if (_editor.Level.Objects[trigger.Target].Type == ObjectInstanceType.Moveable)
                                    {
                                        MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[trigger.Target];
                                        if (instance.Model.ObjectID >= 398 && instance.Model.ObjectID <= 406)
                                        {
                                            item = _aiObjectsTable[trigger.Target];
                                        }
                                        else
                                        {
                                            item = _moveablesTable[trigger.Target];
                                        }
                                    }

                                    trigger2 = (ushort)(item & 0x3ff | (0x00 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.Camera)
                                {
                                    // Trigger for camera
                                    trigger2 = (ushort)(_cameraTable[trigger.Target] & 0x3ff | (0x01 << 10));
                                    tempCodes.Add(trigger2);

                                    // Additional short
                                    trigger3 = 0;
                                    trigger3 |= (ushort)(trigger.Timer & 0xff);
                                    trigger3 |= (ushort)(trigger.OneShot ? 0x100 : 0);
                                    tempCodes.Add(trigger3);
                                }
                                else if (trigger.TargetType == TriggerTargetType.Sink)
                                {
                                    // Trigger for sink
                                    trigger2 = (ushort)(_sinkTable[trigger.Target] & 0x3ff | (0x02 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipMap)
                                {
                                    // Trigger for flip map
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x03 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipOn)
                                {
                                    // Trigger for flip map on
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x04 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipOff)
                                {
                                    // Trigger for flip map off
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x05 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.Target)
                                {
                                    // Trigger for look at item
                                    trigger2 = (ushort)(_moveablesTable[trigger.Target] & 0x3ff | (0x06 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FinishLevel)
                                {
                                    // Trigger for finish level
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x07 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.PlayAudio)
                                {
                                    // Trigger for play soundtrack
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x08 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipEffect)
                                {
                                    // Trigger for flip effect
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x09 << 10));
                                    tempCodes.Add(trigger2);

                                    /*  trigger2 = (ushort)(trigger.Timer);
                                      tempCodes.Add(trigger2);*/
                                }
                                else if (trigger.TargetType == TriggerTargetType.Secret)
                                {
                                    // Trigger for secret found
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x0a << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlyByCamera)
                                {
                                    // Trigger for fly by
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x0c << 10));
                                    tempCodes.Add(trigger2);

                                    trigger2 = (ushort)(trigger.OneShot ? 0x0100 : 0x00);
                                    tempCodes.Add(trigger2);
                                }
                            }

                            tempCodes[tempCodes.Count - 1] |= 0x8000; // End of the action list
                        }

                        if (tempCodes.Count == 0)
                        {
                            sector.FloorDataIndex = 0;
                        }
                        else
                        {
                            // Mark the end of the list
                            tempCodes[lastFloorDataFunction] |= 0x8000;
                            tempFloorData.AddRange(tempCodes);
                        }

                        // Update the sector
                        SaveSector(idNewRoom, x, z, sector);
                    }
                }
            }

            FloorData = tempFloorData.ToArray();

            ReportProgress(80, "    Floordata size: " + FloorData.Length * 2 + " bytes");
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

            if (txt < 0) txt = (short)-txt;

            tr_object_texture tile = new tr_object_texture();
            TR4Wad.wad_object_texture tex = wad.Textures[txt];

            // Texture page
            tile.Tile = (ushort)(tex.Page + _numRoomTexturePages);
            if (triangle) tile.Tile |= 0x8000;

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
            if ((attributes & 0x01) == 0x01) tile.Attributes = 2;

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

        // Decompiled code from winroomedit.exe ---------------------------------------------------------------------------------

        private bool IsXZInBorderOrOutsideRoom(int room, int x, int z)
        {
            return (x <= 0 || z <= 0 || x >= Rooms[room].NumXSectors - 1 || z >= Rooms[room].NumZSectors - 1);
        }

        private bool CanSectorBeReachedAndIsSolid(int room, int x, int z, out int destRoom)
        {
            tr_room currentRoom;
            Room editorRoom;
            int roomIndex = room;
            int xInRoom = 0;
            int zInRoom = 0;
            int xRoomPosition = 0;
            int zRoomPosition = 0;
            Portal portal;

            destRoom = room;

            currentRoom = Rooms[roomIndex];
            editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = x - xRoomPosition;
            zInRoom = z - zRoomPosition;

            bool isOutside = IsXZInBorderOrOutsideRoom(roomIndex, xInRoom, zInRoom);

            while (isOutside)
            {
                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - xRoomPosition;
                zInRoom = z - zRoomPosition;

                // Limit the X, Z to current room
                if (xInRoom >= 0)
                {
                    if (xInRoom >= currentRoom.NumXSectors)
                        xInRoom = currentRoom.NumXSectors - 1;
                }
                else
                {
                    xInRoom = 0;
                }
                if (zInRoom >= 0)
                {
                    if (zInRoom >= currentRoom.NumZSectors)
                        zInRoom = currentRoom.NumZSectors - 1;
                }
                else
                {
                    zInRoom = 0;
                }

                // If current X, Z is not a block wall then exit the loop
                if (editorRoom.Blocks[xInRoom, zInRoom].WallPortal == -1) return false;

                // Get the wall portal
                portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].WallPortal];
                roomIndex = _roomsIdTable[portal.AdjoiningRoom];
                destRoom = roomIndex;

                // If portal is a toggle opacity 1 then I can't go to original X, Z so quit the function
                if (editorRoom.Blocks[xInRoom, zInRoom].WallOpacity == PortalOpacity.Opacity1) return false;

                // Check if now I'm outside
                isOutside = IsXZInBorderOrOutsideRoom(roomIndex, x, z);
            }

            // If I am here, I've probed that I can reach the requested X, Z
            // Now I have to check if the floor under that sector is solid
            currentRoom = Rooms[roomIndex];
            editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = x - xRoomPosition;
            zInRoom = z - zRoomPosition;

            bool isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != -1);

            // Navigate all floor portals until I come to a solid surface or to a water surface
            while(isFloorPortal)
            {
                // Get the floor portal
                portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].FloorPortal];
                roomIndex = _roomsIdTable[portal.AdjoiningRoom];
                destRoom = roomIndex;

                // If floor portal is toggle opacity 1 and not one of the two rooms are water rooms
                if (editorRoom.Blocks[xInRoom, zInRoom].FloorOpacity == PortalOpacity.Opacity1 && 
                    !(editorRoom.FlagWater ^ _editor.Level.Rooms[Rooms[_roomsIdTable[portal.AdjoiningRoom]].OriginalRoomId].FlagWater))
                {
                    return true;
                }

                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - xRoomPosition;
                zInRoom = z - zRoomPosition;

                isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != -1);
            }

            return true;
        }

        private int GetBoxFloorHeight(int room, int x, int z)
        {
            return GetMostDownFloor(room, x, z);

            int roomIndex = room;

            tr_room currentRoom = Rooms[roomIndex];
            Room editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

            int positionX = (int)(currentRoom.Info.X / 1024.0f);
            int positionZ = (int)(currentRoom.Info.Z / 1024.0f);

            int xInRoom = x - positionX;
            int zInRoom = z - positionZ;

            int height0 = -1;
            int height1 = -1;
            int height2 = -1;
            int height3 = -1;

            byte slope1 = 0;
            byte slope2 = 0;
            byte slope3 = 0;
            byte slope4 = 0;

            if (xInRoom < 0 || xInRoom > currentRoom.NumXSectors - 1 || zInRoom < 0 || zInRoom > currentRoom.NumZSectors - 1)
            {
                return 0x7fff;
            }

            Block block = editorRoom.Blocks[xInRoom, zInRoom];

            // If block is a wall or is a vertical toggle opacity 1
            if ((block.Type == BlockType.BorderWall || block.Type == BlockType.Wall) && block.WallOpacity == PortalOpacity.Opacity1)
            {
                return 0x7fff;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if (!(block.WallPortal == -1 || block.WallOpacity == PortalOpacity.Opacity1))
            {
                // Is a wall portal if I'm here
                Portal portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].WallPortal];

                roomIndex = _roomsIdTable[portal.AdjoiningRoom];

                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                positionX = (int)(currentRoom.Info.X / 1024.0f);
                positionZ = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - positionX;
                zInRoom = z - positionZ;

                block = editorRoom.Blocks[xInRoom, zInRoom];
            }

            bool isFloorPortal = block.FloorPortal != -1;

            while (isFloorPortal)
            {
                Portal portal = _level.Portals[block.FloorPortal];

                // If the floor is toggle opacity 1 then exit loop
                if (block.FloorOpacity == PortalOpacity.Opacity1 &&
                    !(editorRoom.FlagWater ^ _editor.Level.Rooms[Rooms[portal.AdjoiningRoom].OriginalRoomId].FlagWater))
                {
                    break;
                }

                roomIndex = _roomsIdTable[portal.AdjoiningRoom];

                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                positionX = (int)(currentRoom.Info.X / 1024.0f);
                positionZ = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - positionX;
                zInRoom = z - positionZ;

                block = editorRoom.Blocks[xInRoom, zInRoom];

                isFloorPortal = block.FloorPortal != -1;
            }

            int sumHeights = block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3];
            int meanFloorCornerHeight = sumHeights >> 2;

            height0 = block.QAFaces[0];
            height1 = block.QAFaces[1];
            height2 = block.QAFaces[2];
            height3 = block.QAFaces[3];

            meanFloorCornerHeight = sumHeights >> 2;

            slope1 = (byte)(Math.Abs(height0 - height1) >= 3 ? 1 : 0);
            slope2 = (byte)(Math.Abs(height1 - height2) >= 3 ? 1 : 0);
            slope3 = (byte)(Math.Abs(height2 - height3) >= 3 ? 1 : 0);
            slope4 = (byte)(Math.Abs(height3 - height0) >= 3 ? 1 : 0);

            bool xa = false;
            bool za = false;

            if (height0 == height2)
            {
                za = false;
            }
            else
            {
                if (height1 != height3)
                {
                    if ((height0 < height1 && height0 < height3) ||
                        (height2 < height1 && height2 < height3) ||
                        (height0 > height1 && height0 > height3) ||
                        (height2 > height1 && height2 > height3))
                    {
                        za = true;
                    }
                }
                else
                {
                    za = true;
                }
            }

            int height = currentRoom.Info.YBottom + meanFloorCornerHeight * -256;

            if (slope1 + slope2 + slope4 + slope3 >= 3 || slope1 == 1 && slope3 == 1 || slope2 == 1 && slope4 == 1)
            {
                if ((block.Flags & BlockFlags.Box) != BlockFlags.Box)            
                {
                    return meanFloorCornerHeight;
                }
            }
            else
            {
                if (za)
                {
                    if ((slope1 == 0 || slope2 == 0) && (slope3 == 0 || slope4 == 0))
                    {
                        if ((block.Flags & BlockFlags.Box) != BlockFlags.Box)
                        {
                            return meanFloorCornerHeight;
                        }
                    }
                }
            }

            return 0x7fff;
        }

        private bool CheckIfCanJumpX(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            if (a.Xmax == b.Xmin || b.Xmax == a.Xmin) return false;

            if (a.Zmax < b.Zmin || a.Zmin > b.Zmax) return false;

            // Get min & max Z
            int zMin = a.Zmin;
            if (a.Zmin <= b.Zmin) zMin = b.Zmin;

            int zMax = a.Zmax;
            if (a.Zmax >= b.Zmax) zMax = b.Zmax;

            // Calculate mean Z
            int zMean = (zMin + zMax) >> 1;

            int roomIndex = 0;
            int destRoom = 0;
            int currentZ = zMean;

            int floor = 0;
            int xMax = 0;

            tr_room currentRoom;

            int xRoomPosition = 0;
            int zRoomPosition = 0;

            int xInRoom = 0;
            int zInRoom = 0;

            if (b.Xmax == a.Xmin - 1 || b.Xmax == a.Xmin - 2)
            {
                xMax = b.Xmax;
                roomIndex = b.Room;
            }

            if (a.Xmax == b.Xmin - 1 || a.Xmax == b.Xmin - 2)
            {
                xMax = a.Xmax;
                roomIndex = a.Room;
            }

            destRoom = roomIndex;

            // If the gap is of 1 sector
            if (b.Xmax == a.Xmin - 1 || a.Xmax == b.Xmin - 1)
            {
                // If X, Zmax - 1 can't be reached then quit the function
                if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax - 1, currentZ, out destRoom)) return false;

                if (CanSectorBeReachedAndIsSolid(roomIndex, xMax, currentZ, out destRoom))
                {
                    currentRoom = Rooms[destRoom];

                    xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                    zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                    xInRoom = xMax - xRoomPosition;
                    zInRoom = currentZ - zRoomPosition;

                    floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                    // Enemy can jump to final box if its height is lower than the starting box
                    if (-floor <= -b.TrueFloor - 512 && floor != 0x7fff) return true;

                    return false;
                }

                return false;
            }

            // If the gap is of 2 sectors
            if (b.Xmax == a.Xmin - 2 || a.Xmax == b.Xmin - 2)
            {
                if (CanSectorBeReachedAndIsSolid(roomIndex, xMax - 1, currentZ, out destRoom))
                {
                    if (CanSectorBeReachedAndIsSolid(roomIndex, xMax, currentZ, out destRoom))
                    {
                        currentRoom = Rooms[destRoom];

                        xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                        zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                        xInRoom = xMax - xRoomPosition;
                        zInRoom = currentZ - zRoomPosition;

                        floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);
                        
                        if (-floor <= -b.TrueFloor - 512 && floor != 0x7fff)
                        {
                            if (CanSectorBeReachedAndIsSolid(roomIndex, xMax + 1, currentZ, out destRoom))
                            {
                                currentRoom = Rooms[destRoom];

                                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                                xInRoom = xMax + 1 - xRoomPosition;
                                zInRoom = currentZ - zRoomPosition;

                                floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                                //floor = GetBoxFloorHeight(destRoom, xMax + 1, currentZ);
                                if (-floor <= -b.TrueFloor - 512 && floor != 0x7FFF) return true;
                            }
                        }
                    }
                }

                return false;
            }

            return false;
        }

        private bool CheckIfCanJumpZ(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            int floorHeight;

            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            if (a.Zmax == b.Zmin || b.Zmax == a.Zmin) return false;

            if (a.Xmax < b.Xmin || a.Xmin > b.Xmax) return false;

            // Get min & max X
            int xMin = a.Xmin;
            if (a.Xmin <= b.Xmin) xMin = b.Xmin;

            int xMax = a.Xmax;
            if (a.Xmax >= b.Xmax) xMax = b.Xmax;

            // Calculate mean X
            int xMean = (xMin + xMax) >> 1;

            int roomIndex = 0; ;

            int currentX = xMean;

            int floor = 0;
            int zMax = 0;

            if (b.Zmax == a.Zmin - 1 || b.Zmax == a.Zmin - 2)
            {
                zMax = b.Zmax;
                roomIndex = b.Room;
            }

            if (a.Zmax == b.Zmin - 1 || a.Zmax == b.Zmin - 2)
            {
                zMax = a.Zmax;
                roomIndex = a.Room;
            }

            int destRoom = 0;

            tr_room currentRoom;

            int xRoomPosition = 0;
            int zRoomPosition = 0;

            int xInRoom = 0;
            int zInRoom = 0;

            // If the gap is of 1 sector
            if (b.Zmax == a.Zmin - 1 || a.Zmax == b.Zmin - 1)
            {
                // If X, Zmax - 1 can't be reached then quit the function
                if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax - 1, out destRoom)) return false;

                if (CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax, out destRoom))
                {
                    currentRoom = Rooms[destRoom];

                    xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                    zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                    xInRoom = currentX - xRoomPosition;
                    zInRoom = zMax - zRoomPosition;

                    floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                  //  floor = GetBoxFloorHeight(destRoom, currentX, zMax);

                    // Enemy can jump to final box if its height is lower than the starting box
                    if (-floor <= -b.TrueFloor - 512 && floor != 0x7fff) return true;

                    return false;
                }

                return false;
            }

            // If the gap is of 2 sectors
            if (b.Zmax == a.Zmin - 2 || a.Zmax == b.Zmin - 2)
            {
                if (CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax - 1, out destRoom))
                {
                    if (CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax, out destRoom))
                    {
                        currentRoom = Rooms[destRoom];

                        xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                        zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                        xInRoom = currentX - xRoomPosition;
                        zInRoom = zMax - zRoomPosition;

                        floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                       // floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax);
                        if (-floor <= -b.TrueFloor - 512 && floor != 0x7fff)
                        {
                            if (CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax + 1, out destRoom))
                            {
                                currentRoom = Rooms[destRoom];

                                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                                xInRoom = currentX - xRoomPosition;
                                zInRoom = zMax + 1 - zRoomPosition;

                                floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                                //floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax + 1);
                                if (-floor <= -b.TrueFloor - 512 && floor != 0x7FFF) return true;
                            }
                        }
                    }
                }

                return false;
            }

            return false;
        }

       /* private bool OverlapXmax(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            int startZ = b.Zmin;
            if (a.Zmin > b.Zmin) startZ = a.Zmin;

            int endZ = b.Zmax;
            if (a.Zmax > b.Zmax) endZ = a.Zmax;

            if (startZ >=endZ)
            {
                return true;
            }
            else
            {
                while(true)
                {
                    if (!CanSectorBeReachedAndIsSolid(a.Room, a.Xmax - 1, startZ)) break;

                    if (b.TrueFloor != GetBoxFloorHeight(a.Room, a.Xmax, startZ)) break;

                    if (++startZ >= endZ) return true;
                }

                return false;
            }
        }

        private bool OverlapXmin(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            int startZ = b.Zmin;
            if (a.Zmin > b.Zmin) startZ = a.Zmin;

            int endZ = b.Zmax;
            if (a.Zmax > b.Zmax) endZ = a.Zmax;

            if (startZ >= endZ)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    if (!CanSectorBeReachedAndIsSolid(a.Room, a.Xmin, startZ)) break;

                    if (b.TrueFloor != GetBoxFloorHeight(a.Room, a.Xmin - 1, startZ)) break;

                    if (++startZ >= endZ) return true;
                }

                return false;
            }
        }

        private bool OverlapZmax(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            int startX = b.Xmin;
            if (a.Xmin > b.Xmin) startX = a.Xmin;

            int endX = b.Xmax;
            if (a.Xmax > b.Xmax) endX = a.Xmax;

            if (startX >= endX)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    if (!CanSectorBeReachedAndIsSolid(a.Room, startX, a.Zmax - 1)) break;

                    if (b.TrueFloor != GetBoxFloorHeight(a.Room, startX, a.Zmax)) break;

                    if (++startX >= endX) return true;
                }

                return false;
            }
        }

        private bool OverlapZmin(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            int startX = b.Xmin;
            if (a.Xmin > b.Xmin) startX = a.Xmin;

            int endX = b.Xmax;
            if (a.Xmax > b.Xmax) endX = a.Xmax;

            if (startX >= endX)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    if (!CanSectorBeReachedAndIsSolid(a.Room, startX, a.Zmin)) break;

                    if (b.TrueFloor != GetBoxFloorHeight(a.Room, startX, a.Zmin - 1)) break;

                    if (++startX >= endX) return true;
                }

                return false;
            }
        }

        private bool CheckIfCanMonkey(int box1, int box2)
        {
            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            return (a.Monkey && b.Monkey);
        }

        private bool BoxesOverlap2(int box1, int box2, out bool jump, out bool monkey)
        {
            if ((box1==286 && box2==317) || (box2 == 286 && box1 == 317))
            {
                int hjhjh = 0;
            }

            tr_box_aux a = tempBoxes[box1];
            tr_box_aux b = tempBoxes[box2];

            jump = false;
            monkey = false;

            if (b.TrueFloor > a.TrueFloor)
            {
                a = tempBoxes[box2];
                b = tempBoxes[box1];
            }

            if (a.Xmax <= b.Xmin || a.Xmin >= b.Xmax)
            {
                if (a.Zmax > b.Zmin && a.Zmin < b.Zmax && CheckIfCanJumpZ(box1, box2))
                {
                    jump = true;
                    return true;
                }
            
                if (a.Xmax < b.Xmin
                  || a.Xmin > b.Xmax
                  || a.Zmax <= b.Zmin
                  || a.Zmin >= b.Zmax
                  || a.Xmax == b.Xmin && !OverlapXmax(box1, box2)
                  || a.Xmin == b.Xmax && !OverlapXmin(box1, box2))
                {
                    return false;
                }

                if (CheckIfCanMonkey(box1, box2)) monkey = true;

                return true;
            }

            if (a.Zmax > b.Zmin && a.Zmin < b.Zmax)
            {
                if (a.TrueFloor != b.TrueFloor) return false;

                if (CheckIfCanMonkey(box1, box2)) monkey = true;

                return true;
            }

            if (CheckIfCanJumpX(box2, box1))
            {
                jump = true;
                return true;
            }

            if (a.Zmax < b.Zmin || a.Zmin > b.Zmax || a.Zmax == b.Zmin && !OverlapZmax(box1, box2)) return false;

            if (a.Zmin != b.Zmax)
            {
                if (CheckIfCanMonkey(box1, box2)) monkey = true;

                return true;
            }
               
            if (OverlapZmin((int)box1, (int)box2))
            {
                if (CheckIfCanMonkey(box1, box2)) monkey = true;

                return true;
            }

            return false;
        }*/
    }
}
