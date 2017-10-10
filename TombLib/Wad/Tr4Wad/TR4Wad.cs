using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombLib.IO;
using System.IO;
using System.Runtime.InteropServices;
using TombLib.Wad;
using SharpDX;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.Wad.Tr4Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_object_texture
    {
        public byte X;
        public byte Y;
        public ushort Page;
        public sbyte FlipX;
        public byte Width;
        public sbyte FlipY;
        public byte Height;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_vertex
    {
        public short X;
        public short Y;
        public short Z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_polygon
    {
        public ushort Shape;
        public ushort V1;
        public ushort V2;
        public ushort V3;
        public ushort V4;
        public ushort Texture;
        public byte Attributes;
        public byte Unknown;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_mesh
    {
        public short SphereX;
        public short SphereY;
        public short SphereZ;
        public ushort Radius;
        public ushort Unknown;
        public ushort NumVertices;
        public List<wad_vertex> Vertices;
        public short NumNormals;
        public List<wad_vertex> Normals;
        public List<short> Shades;
        public ushort NumPolygons;
        public List<wad_polygon> Polygons;

        public Vector3 Minimum;
        public Vector3 Maximum;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_animation
    {
        public uint KeyFrameOffset;
        public byte FrameDuration;
        public byte KeyFrameSize;
        public ushort StateId;
        public int Speed;
        public int Accel;
        public int SpeedLateral;
        public int AccelLateral;
        public ushort FrameStart;
        public ushort FrameEnd;
        public ushort NextAnimation;
        public ushort NextFrame;
        public ushort NumStateChanges;
        public ushort ChangesIndex;
        public ushort NumCommands;
        public ushort CommandOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_state_change
    {
        public ushort StateId;
        public ushort NumDispatches;
        public ushort DispatchesIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_anim_dispatch
    {
        public short Low;
        public short High;
        public short NextAnimation;
        public short NextFrame;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_moveable
    {
        public uint ObjectID;
        public ushort NumPointers;
        public ushort PointerIndex;
        public uint LinksIndex;
        public uint KeyFrameOffset;
        public short AnimationIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_static_mesh
    {
        public uint ObjectId;
        public ushort PointersIndex;
        public short VisibilityX1;
        public short VisibilityX2;
        public short VisibilityY1;
        public short VisibilityY2;
        public short VisibilityZ1;
        public short VisibilityZ2;
        public short CollisionX1;
        public short CollisionX2;
        public short CollisionY1;
        public short CollisionY2;
        public short CollisionZ1;
        public short CollisionZ2;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_sound_info
    {
        public ushort Sample;
        public byte Volume;
        public byte Range;
        public byte Chance;
        public byte Pitch;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct wad_sprite_texture
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
    public struct wad_sprite_sequence
    {
        public int ObjectID;
        public short NegativeLength;
        public short Offset;
    }

    public class Tr4Wad
    {
        public struct texture_piece
        {
            public byte Width;
            public byte Height;
            public byte[] Data;
        }

        public List<wad_object_texture> Textures = new List<wad_object_texture>();
        public byte[,] TexturePages;
        public int NumTexturePages;
        public List<uint> Pointers = new List<uint>();
        public List<uint> RealPointers = new List<uint>();
        public List<uint> HelperPointers = new List<uint>();
        public List<wad_mesh> Meshes = new List<wad_mesh>();
        public List<wad_animation> Animations = new List<wad_animation>();
        public List<wad_state_change> Changes = new List<wad_state_change>();
        public List<wad_anim_dispatch> Dispatches = new List<wad_anim_dispatch>();
        public List<short> Commands = new List<short>();
        public List<int> Links = new List<int>();
        public List<short> KeyFrames = new List<short>();
        public List<wad_moveable> Moveables = new List<wad_moveable>();
        public List<wad_static_mesh> StaticMeshes = new List<wad_static_mesh>();
        public short[] SoundMap = new short[370];
        public List<wad_sound_info> SoundInfo = new List<wad_sound_info>();
        public List<wad_sprite_sequence> SpriteSequences = new List<wad_sprite_sequence>();
        public List<wad_sprite_texture> SpriteTextures = new List<wad_sprite_texture>();
        public byte[] SpriteData;

        public List<string> Sounds { get; set; } = new List<string>();
        public string BaseName { get; set; }
        public string BasePath { get; set; }

        public void LoadWad(string fileName)
        {
            BaseName = Path.GetFileNameWithoutExtension(fileName);
            BasePath = Path.GetDirectoryName(fileName);

            // inizializzo lo stream
            using (BinaryReaderEx reader = new BinaryReaderEx(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // leggo la versione
                int version = reader.ReadInt32();
                if (version != 129)
                    throw new InvalidDataException();

                // leggo le texture
                uint numTextures = reader.ReadUInt32();
                for (int i = 0; i < numTextures; i++)
                {
                    wad_object_texture text;
                    reader.ReadBlock<wad_object_texture>(out text);
                    Textures.Add(text);
                }

                uint numTextureBytes = reader.ReadUInt32();
                TexturePages = new byte[numTextureBytes / 768, 768];
                for (int y = 0; y < numTextureBytes / 768; y++)
                    for (int x = 0; x < 768; x++)
                        TexturePages[y, x] = reader.ReadByte();

                NumTexturePages = (int)(numTextureBytes / 196608);

                // leggo le mesh
                uint numMeshPointers = reader.ReadUInt32();
                for (int i = 0; i < numMeshPointers; i++)
                {
                    Pointers.Add(reader.ReadUInt32());
                    RealPointers.Add(0);
                    HelperPointers.Add(0);
                }

                uint numMeshWords = reader.ReadUInt32();
                uint bytesRead = 0;

                while (bytesRead < (numMeshWords * 2))
                {
                    uint startOfMesh = (uint)reader.BaseStream.Position;

                    wad_mesh mesh = new wad_mesh();
                    mesh.Polygons = new List<wad_polygon>();
                    mesh.Vertices = new List<wad_vertex>();
                    mesh.Normals = new List<wad_vertex>();
                    mesh.Shades = new List<short>();

                    mesh.SphereX = reader.ReadInt16();
                    mesh.SphereY = reader.ReadInt16();
                    mesh.SphereZ = reader.ReadInt16();
                    mesh.Radius = reader.ReadUInt16();
                    mesh.Unknown = reader.ReadUInt16();

                    ushort numVertices = reader.ReadUInt16();
                    mesh.NumVertices = numVertices;

                    int xMin = Int32.MaxValue;
                    int yMin = Int32.MaxValue;
                    int zMin = Int32.MaxValue;
                    int xMax = Int32.MinValue;
                    int yMax = Int32.MinValue;
                    int zMax = Int32.MinValue;

                    for (int i = 0; i < numVertices; i++)
                    {
                        wad_vertex v;
                        reader.ReadBlock<wad_vertex>(out v);

                        if (v.X < xMin)
                            xMin = v.X;
                        if (-v.Y < yMin)
                            yMin = -v.Y;
                        if (v.Z < zMin)
                            zMin = v.Z;

                        if (v.X > xMax)
                            xMax = v.X;
                        if (-v.Y > yMax)
                            yMax = -v.Y;
                        if (v.Z > zMax)
                            zMax = v.Z;

                        mesh.Vertices.Add(v);
                    }

                    mesh.Minimum = new Vector3(xMin, yMin, zMin);
                    mesh.Maximum = new Vector3(xMax, yMax, zMax);

                    short numNormals = reader.ReadInt16();
                    mesh.NumNormals = numNormals;
                    if (numNormals > 0)
                    {
                        for (int i = 0; i < numNormals; i++)
                        {
                            wad_vertex v;
                            reader.ReadBlock<wad_vertex>(out v);
                            mesh.Normals.Add(v);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < -numNormals; i++)
                        {
                            mesh.Shades.Add(reader.ReadInt16());
                        }
                    }

                    ushort numPolygons = reader.ReadUInt16();
                    mesh.NumPolygons = numPolygons;
                    ushort numQuads = 0;
                    for (int i = 0; i < numPolygons; i++)
                    {
                        wad_polygon poly = new wad_polygon();
                        poly.Shape = reader.ReadUInt16();
                        poly.V1 = reader.ReadUInt16();
                        poly.V2 = reader.ReadUInt16();
                        poly.V3 = reader.ReadUInt16();
                        if (poly.Shape == 9)
                            poly.V4 = reader.ReadUInt16();
                        poly.Texture = reader.ReadUInt16();
                        poly.Attributes = reader.ReadByte();
                        poly.Unknown = reader.ReadByte();

                        if (poly.Shape == 9)
                            numQuads++;
                        mesh.Polygons.Add(poly);
                    }

                    if (numQuads % 2 != 0)
                        reader.ReadInt16();

                    uint endPosition = (uint)reader.BaseStream.Position;
                    bytesRead += endPosition - startOfMesh;
                    Meshes.Add(mesh);

                    // Update the real pointers
                    for (int k = 0; k < Pointers.Count; k++)
                    {
                        if (Pointers[k] == bytesRead)
                        {
                            RealPointers[k] = (uint)Meshes.Count;
                            HelperPointers[k] = (uint)Meshes.Count;
                        }

                    }
                }

                uint numAnimations = reader.ReadUInt32();
                for (int i = 0; i < numAnimations; i++)
                {
                    wad_animation anim;
                    reader.ReadBlock<wad_animation>(out anim);
                    Animations.Add(anim);
                }

                uint numChanges = reader.ReadUInt32();
                for (int i = 0; i < numChanges; i++)
                {
                    wad_state_change change;
                    reader.ReadBlock<wad_state_change>(out change);
                    Changes.Add(change);
                }

                uint numDispatches = reader.ReadUInt32();
                for (int i = 0; i < numDispatches; i++)
                {
                    wad_anim_dispatch anim;
                    reader.ReadBlock<wad_anim_dispatch>(out anim);
                    Dispatches.Add(anim);
                }

                uint numCommands = reader.ReadUInt32();
                for (int i = 0; i < numCommands; i++)
                {
                    short anim;
                    reader.ReadBlock<short>(out anim);
                    Commands.Add(anim);
                }

                uint numLinks = reader.ReadUInt32();
                for (int i = 0; i < numLinks; i++)
                {
                    int link;
                    reader.ReadBlock<int>(out link);
                    Links.Add(link);
                }

                uint numFrames = reader.ReadUInt32();
                for (int i = 0; i < numFrames; i++)
                {
                    short frame;
                    reader.ReadBlock<short>(out frame);
                    KeyFrames.Add(frame);
                }

                uint numMoveables = reader.ReadUInt32();
                for (int i = 0; i < numMoveables; i++)
                {
                    long pos = reader.BaseStream.Position;
                    wad_moveable moveable;
                    reader.ReadBlock<wad_moveable>(out moveable);
                    Moveables.Add(moveable);
                }

                uint numStaticMeshes = reader.ReadUInt32();
                for (int i = 0; i < numStaticMeshes; i++)
                {
                    wad_static_mesh staticMesh;
                    reader.ReadBlock<wad_static_mesh>(out staticMesh);
                    StaticMeshes.Add(staticMesh);
                }

                reader.Close();

                // Read sounds
                using (var readerSounds = new StreamReader(new FileStream(BasePath + "\\" + BaseName + ".sam", FileMode.Open, FileAccess.Read, FileShare.Read)))
                    while (!readerSounds.EndOfStream)
                        Sounds.Add(readerSounds.ReadLine());

                using (var readerSfx = new BinaryReaderEx(new FileStream(BasePath + "\\" + BaseName + ".sfx", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    for (int i = 0; i < 370; i++)
                    {
                        SoundMap[i] = readerSfx.ReadInt16();
                    }

                    uint numSounds = readerSfx.ReadUInt32();
                    for (int i = 0; i < numSounds; i++)
                    {
                        wad_sound_info info;
                        readerSfx.ReadBlock<wad_sound_info>(out info);

                        SoundInfo.Add(info);
                    }
                }

                // Read sprites
                using (var readerSprites = new BinaryReaderEx(new FileStream(
                                            BasePath + Path.DirectorySeparatorChar + BaseName + ".swd",
                FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    // Version
                    readerSprites.ReadUInt32();

                    uint numSpritesTextures = readerSprites.ReadUInt32();

                    //Sprite texture array
                    for (int i = 0; i < numSpritesTextures; i++)
                    {
                        byte[] buffer;
                        readerSprites.ReadBlockArray(out buffer, 16);

                        var spriteTexture = new wad_sprite_texture
                        {
                            Tile = buffer[2],
                            X = buffer[0],
                            Y = buffer[1],
                            Width = (ushort)(buffer[5]),
                            Height = (ushort)(buffer[7]),
                            LeftSide = buffer[0],
                            TopSide = buffer[1],
                            RightSide = (short)(buffer[0] + buffer[5] + 1),
                            BottomSide = (short)(buffer[1] + buffer[7] + 1)
                        };

                        SpriteTextures.Add(spriteTexture);
                    }

                    // Sprites size
                    int spriteDataSize = readerSprites.ReadInt32();
                    SpriteData = readerSprites.ReadBytes(spriteDataSize);

                    uint numSequences = readerSprites.ReadUInt32();

                    // Sprite sequences
                    for (int i = 0; i < numSequences; i++)
                    {
                        wad_sprite_sequence sequence;
                        readerSprites.ReadBlock(out sequence);
                        SpriteSequences.Add(sequence);
                    }
                }
            }
        }

        public int GetNextMoveableWithAnimations(int current)
        {
            for (int i = current + 1; i < Moveables.Count; i++)
                if (Moveables[i].AnimationIndex != -1)
                    return i;
            return -1;
        }

        public wad_mesh GetMeshFromPointer(int pointer)
        {
            return Meshes[(int)RealPointers[pointer]];
        }
    }
}
