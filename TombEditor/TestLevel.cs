using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;
using NLog;
using TombLib.IO;

namespace TombEngine
{   
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
        public ushort Texture;
        public ushort LightingEffect;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_face3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] Vertices;
        public ushort Texture;
        public ushort LightingEffect;
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
        public ushort BoxIndex;
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
        public short AmbientIntensity1;
        public short AmbientIntensity2;
        public short LightMode;
        public ushort NumLights;
        public tr4_room_light[] Lights;
        public ushort NumStaticMeshes;
        public tr_room_staticmesh[] StaticMeshes;
        public short AlternateRoom;
        public short Flags;
        public byte Param1;
        public byte Unknown1;
        public byte Unknown2;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public tr_vertex[] BoundingBox;
        public ushort Flags;
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
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
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
        public byte Xcoordinate;
        public byte Xpixel;
        public byte Ycoordinate;
        public byte Ypixel;
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
        public ushort Unknown1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_animatedTextures
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

    class TombRaider4Level
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        public short[] FloorData;

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
        public ushort[] Frames;
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
        public byte[] FlyByCameras;
        public uint NumSoundSources;
        public tr_sound_source[] SoundSources;
        public uint NumBoxes;
        public tr_box[] Boxes;
        public uint NumOverlaps;
        public ushort[] Overlaps;
        public short[] Zones;
        public uint NumAnimatedTextures;
        //public ushort[] AnimatedTextures;

        public byte[] TEX;

        public uint NumObjectTextures;
        public tr_object_texture[] ObjectTextures;
        public uint NumItems;
        public tr_item[] Items;
        public uint NumAiItems;
        public tr_ai_item[] AiItems;

        string fileName;
        
        public TombRaider4Level(string fileName)
        {
            this.fileName = fileName;
        }

        public void Load(string ind)
        {
            FileStream fileStream = File.OpenRead(fileName);
            BinaryReaderEx reader = new BinaryReaderEx(fileStream);
            byte[] buffer;
            MemoryStream stream = new MemoryStream();
            Inflater inflater = new Inflater(false);
            InflaterInputStream input;

            reader.ReadBlock(out Version);
            reader.ReadBlock(out NumRoomTextureTiles);
            reader.ReadBlock(out NumObjectTextureTiles);
            reader.ReadBlock(out NumBumpTextureTiles);

            reader.ReadBlock(out Texture32UncompressedSize);
            reader.ReadBlock(out Texture32CompressedSize);
            Texture32 = new byte[Texture32CompressedSize];
            reader.ReadBlockArray(out Texture32, Texture32CompressedSize);
            stream.Write(Texture32, 0, (int)Texture32CompressedSize);
            Texture32 = new byte[Texture32UncompressedSize];
            stream.Seek(0, SeekOrigin.Begin);
            input = new InflaterInputStream(stream, new Inflater(false));
            input.Read(Texture32, 0, (int)Texture32UncompressedSize);

            BinaryWriterEx wrttext = new BinaryWriterEx(File.OpenWrite("textures.raw"));
            wrttext.WriteBlockArray(Texture32);
            wrttext.Flush();
            wrttext.Close();

            stream = new MemoryStream();
            reader.ReadBlock(out Texture16UncompressedSize);
            reader.ReadBlock(out Texture16CompressedSize);
            Texture16 = new byte[Texture16CompressedSize];
            reader.ReadBlockArray(out Texture16, Texture16CompressedSize);
            stream.Write(Texture16, 0, (int)Texture16CompressedSize);
            Texture16 = new byte[Texture16UncompressedSize];
            stream.Seek(0, SeekOrigin.Begin);
            input = new InflaterInputStream(stream, new Inflater(false));
            input.Read(Texture16, 0, (int)Texture16UncompressedSize);


            stream = new MemoryStream();
            reader.ReadBlock(out MiscTextureUncompressedSize);
            reader.ReadBlock(out MiscTextureCompressedSize);
            MiscTexture = new byte[MiscTextureCompressedSize];
            reader.ReadBlockArray(out MiscTexture, MiscTextureCompressedSize);
            stream.Write(MiscTexture, 0, (int)MiscTextureCompressedSize);
            MiscTexture = new byte[MiscTextureUncompressedSize];
            stream.Seek(0, SeekOrigin.Begin);
            input = new InflaterInputStream(stream, new Inflater(false));
            input.Read(MiscTexture, 0, (int)MiscTextureUncompressedSize);


            stream = new MemoryStream();
            reader.ReadBlock(out LevelUncompressedSize);
            reader.ReadBlock(out LevelCompressedSize);
            buffer = new byte[LevelCompressedSize];
            reader.ReadBlockArray(out buffer, LevelCompressedSize);
            stream.Write(buffer, 0, (int)LevelCompressedSize);
            buffer = new byte[LevelUncompressedSize];
            stream.Seek(0, SeekOrigin.Begin);
            input = new InflaterInputStream(stream, new Inflater(false));
            input.Read(buffer, 0, (int)LevelUncompressedSize);

            stream = new MemoryStream();
            stream.Write(buffer, 0, (int)LevelUncompressedSize);
            stream.Seek(0, SeekOrigin.Begin);

            BinaryWriterEx wrt = new BinaryWriterEx(File.OpenWrite("coastal.bin"));
            wrt.Write(buffer, 0, (int)LevelUncompressedSize);
            wrt.Flush();
            wrt.Close();

            BinaryWriterEx wrs = new BinaryWriterEx(File.OpenWrite("samples." + ind + ".bin"));
            byte[] samples = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            wrs.Write(samples);
            wrs.Flush();
            wrs.Close();

            reader.Close();

            reader = new BinaryReaderEx(stream);
            reader.ReadBlock(out Unused);
            reader.ReadBlock(out NumRooms);

            int max = 0;

            StreamWriter wp = new StreamWriter(File.OpenWrite("portals" + ind + ".txt"));

            Rooms = new tr_room[NumRooms];
            for (int i = 0; i < NumRooms; i++)
            {
                wp.WriteLine("=====================================================================");
                wp.WriteLine("ROOM #" + i);
                wp.WriteLine("=====================================================================");

                reader.ReadBlock(out Rooms[i].Info);
                reader.ReadBlock(out Rooms[i].NumDataWords);
                reader.ReadBlock(out Rooms[i].NumVertices);
                //  Rooms[i].Vertices = new tr_room_vertex[Rooms[i].NumVertices];
                reader.ReadBlockArray(out Rooms[i].Vertices, Rooms[i].NumVertices);
                if (Rooms[i].NumVertices > max)
                    max = Rooms[i].NumVertices;
                reader.ReadBlock(out Rooms[i].NumRectangles);
                Rooms[i].Rectangles = new tr_face4[Rooms[i].NumRectangles];
                for (int j = 0; j < Rooms[i].NumRectangles; j++)
                {
                    // Rooms[i].Rectangles[j].Vertices = new ushort[4];
                    reader.ReadBlockArray(out Rooms[i].Rectangles[j].Vertices, 4);
                    reader.ReadBlock(out Rooms[i].Rectangles[j].Texture);
                }

                reader.ReadBlock(out Rooms[i].NumTriangles);
                Rooms[i].Triangles = new tr_face3[Rooms[i].NumTriangles];
                for (int j = 0; j < Rooms[i].NumTriangles; j++)
                {
                    // Rooms[i].Triangles[j].Vertices = new ushort[3];
                    reader.ReadBlockArray(out Rooms[i].Triangles[j].Vertices, 3);
                    reader.ReadBlock(out Rooms[i].Triangles[j].Texture);
                }

                reader.ReadBlock(out Rooms[i].NumSprites);

                reader.ReadBlock(out Rooms[i].NumPortals);
                //Rooms[i].Portals = new tr_room_portal[Rooms[i].NumPortals];
                reader.ReadBlockArray(out Rooms[i].Portals, Rooms[i].NumPortals);

                for (int nn = 0; nn < Rooms[i].Portals.Length; nn++)
                {
                    tr_room_portal pt = Rooms[i].Portals[nn];
                    wp.WriteLine(nn + ": ");
                    wp.WriteLine("Room: " + pt.AdjoiningRoom);
                    for (int vv = 0; vv < 4; vv++)
                    {
                        wp.Write("V" + vv + " = " + pt.Vertices[vv].X + ", " + pt.Vertices[vv].Y + ", " + pt.Vertices[vv].Z);
                        wp.WriteLine("");
                    }
                    wp.WriteLine("");
                }

                reader.ReadBlock(out Rooms[i].NumZSectors);
                reader.ReadBlock(out Rooms[i].NumXSectors);
                //Rooms[i].Sectors = new tr_room_sector[Rooms[i].NumZSectors * Rooms[i].NumXSectors];
                reader.ReadBlockArray(out Rooms[i].Sectors, (uint)Rooms[i].NumZSectors * Rooms[i].NumXSectors);

                reader.ReadBlock(out Rooms[i].AmbientIntensity1);
                reader.ReadBlock(out Rooms[i].AmbientIntensity2);

                reader.ReadBlock(out Rooms[i].NumLights);
                reader.ReadBlockArray(out Rooms[i].Lights, Rooms[i].NumLights);

                reader.ReadBlock(out Rooms[i].NumStaticMeshes);
                reader.ReadBlockArray(out Rooms[i].StaticMeshes, Rooms[i].NumStaticMeshes);

                reader.ReadBlock(out Rooms[i].AlternateRoom);
                reader.ReadBlock(out Rooms[i].Flags);
                reader.ReadBlock(out Rooms[i].Param1);
                reader.ReadBlock(out Rooms[i].Unknown1);
                reader.ReadBlock(out Rooms[i].Unknown2);
            }

            wp.Flush();
            wp.Close();
            //return;

            reader.ReadBlock(out NumFloorData);
            reader.ReadBlockArray(out FloorData, NumFloorData);

            reader.ReadBlock(out NumMeshData);

            int numBytes = 0;
            int totalBytes = 0;
            int l = 0;
            short temp = 0;

            Meshes = new tr_mesh[2048];
            while (totalBytes < (NumMeshData * 2))
            {
                long offset1 = reader.BaseStream.Position;

                reader.ReadBlock(out Meshes[l].Centre);
                reader.ReadBlock(out Meshes[l].Radius);
                numBytes += 10;

                reader.ReadBlock(out Meshes[l].NumVertices);
                reader.ReadBlockArray(out Meshes[l].Vertices, Meshes[l].NumVertices);
                numBytes += 2 + 6 * Meshes[l].NumVertices;

                reader.ReadBlock(out Meshes[l].NumNormals);
                if (Meshes[l].NumNormals > 0)
                {
                    reader.ReadBlockArray(out Meshes[l].Normals, Meshes[l].NumNormals);
                    numBytes += 2 + 6 * Meshes[l].NumNormals;
                }
                else
                {
                    reader.ReadBlockArray(out Meshes[l].Lights, -Meshes[l].NumNormals);
                    numBytes += 2 - 2 * Meshes[l].NumNormals;
                }

                reader.ReadBlock(out Meshes[l].NumTexturedRectangles);
                reader.ReadBlockArray(out Meshes[l].TexturedRectangles, Meshes[l].NumTexturedRectangles);
                numBytes += 2 + 12 * Meshes[l].NumTexturedRectangles;

                reader.ReadBlock(out Meshes[l].NumTexturedTriangles);
                reader.ReadBlockArray(out Meshes[l].TexturedTriangles, Meshes[l].NumTexturedTriangles);
                numBytes += 2 + 10 * Meshes[l].NumTexturedTriangles;

                long offset2 = reader.BaseStream.Position;
                int diff = (int)(offset2 - offset1);
                if (diff % 4 != 0)
                { reader.ReadBlock(out temp); diff += 2; }
                Meshes[l].MeshSize = numBytes;
                Meshes[l].MeshPointer = totalBytes;

                if (l == 209)
                {
                    BinaryWriterEx tmpwriter = new BinaryWriterEx(File.OpenWrite("cleopal.msh"));
                    tmpwriter.WriteBlock(Meshes[l].Centre);
                    tmpwriter.WriteBlock(Meshes[l].Radius);
                    tmpwriter.WriteBlock(Meshes[l].NumVertices);
                    tmpwriter.WriteBlockArray(Meshes[l].Vertices);
                    tmpwriter.WriteBlock(Meshes[l].NumNormals);
                    if (Meshes[l].NumNormals > 0)
                        tmpwriter.WriteBlockArray(Meshes[l].Normals);
                    else
                        tmpwriter.WriteBlockArray(Meshes[l].Lights);
                    tmpwriter.WriteBlock(Meshes[l].NumTexturedRectangles);
                    tmpwriter.WriteBlockArray(Meshes[l].TexturedRectangles);
                    tmpwriter.WriteBlock(Meshes[l].NumTexturedTriangles);
                    tmpwriter.WriteBlockArray(Meshes[l].TexturedTriangles);

                    tmpwriter.Flush();
                    tmpwriter.Close();
                }

                totalBytes += diff;// numBytes;
                numBytes = 0;
                l++;
            }

            Array.Resize(ref Meshes, l);

            NumMeshes = (uint)Meshes.Length;

            reader.ReadBlock(out NumMeshPointers);
            reader.ReadBlockArray(out MeshPointers, NumMeshPointers);

            reader.ReadBlock(out NumAnimations);
            reader.ReadBlockArray(out Animations, NumAnimations);

            reader.ReadBlock(out NumStateChanges);
            reader.ReadBlockArray(out StateChanges, NumStateChanges);

            reader.ReadBlock(out NumAnimDispatches);
            reader.ReadBlockArray(out AnimDispatches, NumAnimDispatches);

            reader.ReadBlock(out NumAnimCommands);
            reader.ReadBlockArray(out AnimCommands, NumAnimCommands);

            reader.ReadBlock(out NumMeshTrees);
            reader.ReadBlockArray(out MeshTrees, NumMeshTrees);

            logger.Debug(reader.BaseStream.Position.ToString());
            reader.ReadBlock(out NumFrames);
            reader.ReadBlockArray(out Frames, NumFrames);

            reader.ReadBlock(out NumMoveables);
            reader.ReadBlockArray(out Moveables, NumMoveables);

            reader.ReadBlock(out NumStaticMeshes);
            reader.ReadBlockArray(out StaticMeshes, NumStaticMeshes);

            reader.ReadBlockArray(out SPR, 3);

            reader.ReadBlock(out NumSpriteTextures);
            reader.ReadBlockArray(out SpriteTextures, NumSpriteTextures);

            reader.ReadBlock(out NumSpriteSequences);
            reader.ReadBlockArray(out SpriteSequences, NumSpriteSequences);

            reader.ReadBlock(out NumCameras);
            reader.ReadBlockArray(out Cameras, NumCameras);

            reader.ReadBlock(out NumFlyByCameras);
            reader.ReadBlockArray(out FlyByCameras, NumFlyByCameras * 40);

            reader.ReadBlock(out NumSoundSources);
            reader.ReadBlockArray(out SoundSources, NumSoundSources);

            reader.ReadBlock(out NumBoxes);
            reader.ReadBlockArray(out Boxes, NumBoxes);

            reader.ReadBlock(out NumOverlaps);
            reader.ReadBlockArray(out Overlaps, NumOverlaps);

            // reader.ReadBlockArray(out Zones, NumBoxes * 10);
            Zones = new short[NumBoxes * 10];
            for (int n = 0; n < NumBoxes * 10; n++)
            {
                Zones[n] = reader.ReadInt16();
            }

            reader.ReadBlock(out NumAnimatedTextures);
            short[] animTextures;
            reader.ReadBlockArray(out animTextures, NumAnimatedTextures);

            string fn = Path.GetFileNameWithoutExtension(fileName);
            if (File.Exists("pathfinding." + fn + "." + ind + ".txt"))
                File.Delete("pathfinding." + fn + "." + ind + ".txt");
            StreamWriter writer = new StreamWriter(File.OpenWrite("pathfinding." + fn + "." + ind + ".txt"));

            writer.WriteLine("BOXES");

            for (int n = 0; n < Boxes.Length; n++)
            {
                writer.WriteLine("[" + n + "] " + "Xmin: " + Boxes[n].Xmin + ", " + "Xmax: " + Boxes[n].Xmax + ", " +
                                 "Zmin: " + Boxes[n].Zmin + ", " + "Zmax: " + Boxes[n].Zmax + ", " + 
                                 "Floor: " + Boxes[n].TrueFloor + ", Overlap Index: " + Boxes[n].OverlapIndex);
            }

            writer.WriteLine(" ");
            writer.WriteLine("OVERLAPS");

            for (int n = 0; n < Overlaps.Length; n++)
            {
                writer.WriteLine("[" + n + "] " + (Overlaps[n] & 0x7fff).ToString());
                if ((Overlaps[n] & 0x8000) != 0)
                    writer.WriteLine("--- END OF LIST ---");
            }

            writer.WriteLine(" ");
            writer.WriteLine("ZONES");

            for (int n = 0; n < Boxes.Length; n++)
            {
                /*writer.WriteLine("[" + n + "] " + "Ground1: " + Zones[n * 10 + 0] + ", " + "Ground2: " + Zones[n * 10 + 1] + ", " +
                                 "Ground3: " + Zones[n * 10 + 2] + ", " + "Ground4: " + Zones[n * 10 + 3] + ", " +
                                 "Fly: " + Zones[n * 10 + 4] + ", A_Ground1: " + Zones[n * 10 + 5] + ", " + "A_Ground2: " + Zones[n * 10 + 6] + ", " +
                                 "A_Ground3: " + Zones[n * 10 + 7] + ", " + "A_Ground4: " + Zones[n * 10 + 8] + ", " +
                                 "A_Fly: " + Zones[n * 10 + 9]);*/
                writer.WriteLine("[" + n + "] " + "Ground1: " + Zones[n] + ", " + "Ground2: " + Zones[1 * NumBoxes + n] + ", " +
                                "Ground3: " + Zones[2 * NumBoxes + n] + ", " + "Ground4: " + Zones[3 * NumBoxes + n] + ", " +
                                "Fly: " + Zones[4 * NumBoxes + n] + ", A_Ground1: " + Zones[5 * NumBoxes + n] + ", " + "A_Ground2: " + Zones[6 * NumBoxes + n] + ", " +
                                "A_Ground3: " + Zones[7 * NumBoxes + n] + ", " + "A_Ground4: " + Zones[8 * NumBoxes + n] + ", " +
                                "A_Fly: " + Zones[9 * NumBoxes + n]);
            }

            writer.Flush();
            writer.Close();

            reader.ReadBlockArray(out TEX, 4);

            reader.ReadBlock(out NumObjectTextures);
            reader.ReadBlockArray(out ObjectTextures, NumObjectTextures);

            for (int ii = 0; ii < NumObjectTextures; ii++)
            {
                /* BinaryWriterEx tmpwriter2 = new BinaryWriterEx(File.OpenWrite("test\\cleopal_" + ii + ".text"));
                 tmpwriter2.WriteBlock(ObjectTextures[ii]);
                 tmpwriter2.Flush();
                 tmpwriter2.Close();*/
            }

            reader.ReadBlock(out NumItems);
            reader.ReadBlockArray(out Items, NumItems);

            reader.ReadBlock(out NumAiItems);
            reader.ReadBlockArray(out AiItems, NumAiItems);

            StreamWriter aiw = new StreamWriter(File.OpenWrite("AI" + ind + ".txt"));

            for (int n = 0; n < NumAiItems; n++)
            {
                aiw.WriteLine("[" + n + "]");
                aiw.WriteLine("    ObjectID: " + AiItems[n].ObjectID);
                aiw.WriteLine("    X: " + AiItems[n].X);
                aiw.WriteLine("    Y: " + AiItems[n].Y);
                aiw.WriteLine("    Z: " + AiItems[n].Z);
                aiw.WriteLine("    Room: " + AiItems[n].Room);
                aiw.WriteLine("    Angle: " + AiItems[n].Angle);
            }

            aiw.Flush();
            aiw.Close();

            BinaryWriterEx bwex = new BinaryWriterEx(File.OpenWrite("sounds" + ind + ".sfx"));

            reader.ReadInt16();
            byte[] soundmap = reader.ReadBytes(370 * 2);
            int numSoundDetails = reader.ReadInt32();
            byte[] details = reader.ReadBytes(8 * numSoundDetails);
            int numIndices = reader.ReadInt32();
            byte[] indices = reader.ReadBytes(4 * numIndices);

            bwex.Write(soundmap);
            bwex.Write(numSoundDetails);
            bwex.Write(details);
            bwex.Write(numIndices);
            bwex.Write(indices);

            bwex.Flush();
            bwex.Close();

            List<byte> bytes = new List<byte>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
                bytes.Add(reader.ReadByte());
        }
    }
}
