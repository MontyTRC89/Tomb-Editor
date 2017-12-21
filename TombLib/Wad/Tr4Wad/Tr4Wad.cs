using NLog;
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
using TombLib.Wad.Catalog;

namespace TombLib.Wad.Tr4Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct wad_object_texture
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
    internal struct wad_vertex
    {
        public short X;
        public short Y;
        public short Z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct wad_polygon
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
    internal struct wad_mesh
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
    internal struct wad_animation
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
    internal struct wad_state_change
    {
        public ushort StateId;
        public ushort NumDispatches;
        public ushort DispatchesIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct wad_anim_dispatch
    {
        public short Low;
        public short High;
        public short NextAnimation;
        public short NextFrame;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct wad_moveable
    {
        public uint ObjectID;
        public ushort NumPointers;
        public ushort PointerIndex;
        public uint LinksIndex;
        public uint KeyFrameOffset;
        public short AnimationIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct wad_static_mesh
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
    internal struct wad_sound_info
    {
        public ushort Sample;
        public byte Volume;
        public byte Range;
        public byte Chance;
        public byte Pitch;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct wad_sprite_texture
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
    internal struct wad_sprite_sequence
    {
        public int ObjectID;
        public short NegativeLength;
        public short Offset;
    }

    public class Tr4Wad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal int Version;
        internal List<wad_object_texture> Textures = new List<wad_object_texture>();
        internal byte[] TexturePages;
        internal int NumTexturePages;
        internal List<uint> Pointers = new List<uint>();
        internal List<uint> RealPointers = new List<uint>();
        internal List<wad_mesh> Meshes = new List<wad_mesh>();
        internal List<wad_animation> Animations = new List<wad_animation>();
        internal List<wad_state_change> Changes = new List<wad_state_change>();
        internal List<wad_anim_dispatch> Dispatches = new List<wad_anim_dispatch>();
        internal List<short> Commands = new List<short>();
        internal List<int> Links = new List<int>();
        internal List<short> KeyFrames = new List<short>();
        internal List<wad_moveable> Moveables = new List<wad_moveable>();
        internal List<wad_static_mesh> StaticMeshes = new List<wad_static_mesh>();
        internal short[] SoundMap = new short[370];
        internal List<wad_sound_info> SoundInfo = new List<wad_sound_info>();
        internal List<wad_sprite_sequence> SpriteSequences = new List<wad_sprite_sequence>();
        internal List<wad_sprite_texture> SpriteTextures = new List<wad_sprite_texture>();
        internal byte[] SpriteData;

        internal List<string> LegacyNames { get; set; } = new List<string>();

        internal List<string> Sounds { get; set; } = new List<string>();
        internal string BaseName { get; set; }
        internal string BasePath { get; set; }

        public void LoadWad(string fileName)
        {
            BaseName = Path.GetFileNameWithoutExtension(fileName);
            BasePath = Path.GetDirectoryName(fileName);

            logger.Info("Reading wad file: " + fileName);

            // Initialize stream
            using (BinaryReaderEx reader = new BinaryReaderEx(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Read wad version
                Version = reader.ReadInt32();
                if (Version != 129 && Version != 130)
                {
                    logger.Error("Wad version  " + Version + " is not supported!");
                    throw new InvalidDataException();
                }

                // Read textures
                uint numTextures = reader.ReadUInt32();
                logger.Info("Wad object textures: " + numTextures);
                for (int i = 0; i < numTextures; i++)
                {
                    wad_object_texture text;
                    reader.ReadBlock<wad_object_texture>(out text);
                    Textures.Add(text);
                }

                uint numTextureBytes = reader.ReadUInt32();
                logger.Info("Wad texture data size: " + numTextureBytes);
                TexturePages = reader.ReadBytes((int)numTextureBytes);

                NumTexturePages = (int)(numTextureBytes / 196608);

                // Read meshes
                uint numMeshPointers = reader.ReadUInt32();
                logger.Info("Wad mesh pointers: " + numMeshPointers);
                for (int i = 0; i < numMeshPointers; i++)
                {
                    Pointers.Add(reader.ReadUInt32());
                    RealPointers.Add(0);
                }

                uint numMeshWords = reader.ReadUInt32();
                uint bytesToRead = numMeshWords * 2;
                uint bytesRead = 0;

                logger.Info("Wad mesh data size: " + bytesToRead);
                while (bytesRead < bytesToRead)
                {
                    var startOfMesh = (uint)reader.BaseStream.Position;

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

                    var numVertices = reader.ReadUInt16();
                    mesh.NumVertices = numVertices;

                    var xMin = Int32.MaxValue;
                    var yMin = Int32.MaxValue;
                    var zMin = Int32.MaxValue;
                    var xMax = Int32.MinValue;
                    var yMax = Int32.MinValue;
                    var zMax = Int32.MinValue;

                    for (var i = 0; i < numVertices; i++)
                    {
                        var v = new wad_vertex();
                        v.X = reader.ReadInt16();
                        v.Y = reader.ReadInt16();
                        v.Z = reader.ReadInt16();

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
                        for (var i = 0; i < numNormals; i++)
                        {
                            var n = new wad_vertex();
                            n.X = reader.ReadInt16();
                            n.Y = reader.ReadInt16();
                            n.Z = reader.ReadInt16();
                            mesh.Normals.Add(n);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < -numNormals; i++)
                        {
                            mesh.Shades.Add(reader.ReadInt16());
                        }
                    }

                    ushort numPolygons = reader.ReadUInt16();
                    mesh.NumPolygons = numPolygons;
                    ushort numQuads = 0;
                    for (var i = 0; i < numPolygons; i++)
                    {
                        var poly = new wad_polygon();
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

                    var endPosition = (uint)reader.BaseStream.Position;
                    bytesRead += endPosition - startOfMesh;
                    Meshes.Add(mesh);

                    // Update the real pointers
                    for (int k = 0; k < Pointers.Count; k++)
                    {
                        if (Pointers[k] == bytesRead)
                            RealPointers[k] = (uint)Meshes.Count;
                    }
                }

                var numAnimations = reader.ReadUInt32();
                logger.Info("Wad animations: " + numAnimations);
                for (var i = 0; i < numAnimations; i++)
                {
                    var anim = new wad_animation();
                    anim.KeyFrameOffset = reader.ReadUInt32();
                    anim.FrameDuration = reader.ReadByte();
                    anim.KeyFrameSize = reader.ReadByte();
                    anim.StateId = reader.ReadUInt16();
                    anim.Speed = reader.ReadInt32();
                    anim.Accel = reader.ReadInt32();
                    anim.SpeedLateral = reader.ReadInt32();
                    anim.AccelLateral = reader.ReadInt32();
                    anim.FrameStart = reader.ReadUInt16();
                    anim.FrameEnd = reader.ReadUInt16();
                    anim.NextAnimation = reader.ReadUInt16();
                    anim.NextFrame = reader.ReadUInt16();
                    anim.NumStateChanges = reader.ReadUInt16();
                    anim.ChangesIndex = reader.ReadUInt16();
                    anim.NumCommands = reader.ReadUInt16();
                    anim.CommandOffset = reader.ReadUInt16();

                    Animations.Add(anim);
                }

                var numChanges = reader.ReadUInt32();
                logger.Info("Wad animation state changes: " + numChanges);
                for (var i = 0; i < numChanges; i++)
                {
                    var change = new wad_state_change();
                    change.StateId = reader.ReadUInt16();
                    change.NumDispatches = reader.ReadUInt16();
                    change.DispatchesIndex = reader.ReadUInt16();
                    Changes.Add(change);
                }

                var numDispatches = reader.ReadUInt32();
                logger.Info("Wad animation dispatches: " + numDispatches);
                for (var i = 0; i < numDispatches; i++)
                {
                    var anim = new wad_anim_dispatch();
                    anim.Low = reader.ReadInt16();
                    anim.High = reader.ReadInt16();
                    anim.NextAnimation = reader.ReadInt16();
                    anim.NextFrame = reader.ReadInt16();
                    Dispatches.Add(anim);
                }

                var numCommands = reader.ReadUInt32();
                logger.Info("Wad animation commands: " + numCommands);
                for (var i = 0; i < numCommands; i++)
                {
                    Commands.Add(reader.ReadInt16());
                }

                var numLinks = reader.ReadUInt32();
                logger.Info("Wad animation links: " + numLinks);
                for (var i = 0; i < numLinks; i++)
                {
                    Links.Add(reader.ReadInt32());
                }

                var numFrames = reader.ReadUInt32();
                logger.Info("Wad animation frames: " + numFrames);
                for (int i = 0; i < numFrames; i++)
                {
                    KeyFrames.Add(reader.ReadInt16());
                }

                var numMoveables = reader.ReadUInt32();
                logger.Info("Wad objects (moveables): " + numMoveables);
                for (var i = 0; i < numMoveables; i++)
                {
                    var moveable = new wad_moveable();
                    moveable.ObjectID = reader.ReadUInt32();
                    moveable.NumPointers = reader.ReadUInt16();
                    moveable.PointerIndex = reader.ReadUInt16();
                    moveable.LinksIndex = reader.ReadUInt32();
                    moveable.KeyFrameOffset = reader.ReadUInt32();
                    moveable.AnimationIndex = reader.ReadInt16();
                    Moveables.Add(moveable);
                }

                var numStaticMeshes = reader.ReadUInt32();
                logger.Info("Wad static meshes: " + numStaticMeshes);
                for (var i = 0; i < numStaticMeshes; i++)
                {
                    var staticMesh = new wad_static_mesh();
                    staticMesh.ObjectId = reader.ReadUInt32();
                    staticMesh.PointersIndex = reader.ReadUInt16();
                    staticMesh.VisibilityX1 = reader.ReadInt16();
                    staticMesh.VisibilityX2 = reader.ReadInt16();
                    staticMesh.VisibilityY1 = reader.ReadInt16();
                    staticMesh.VisibilityY2 = reader.ReadInt16();
                    staticMesh.VisibilityZ1 = reader.ReadInt16();
                    staticMesh.VisibilityZ2 = reader.ReadInt16();
                    staticMesh.CollisionX1 = reader.ReadInt16();
                    staticMesh.CollisionX2 = reader.ReadInt16();
                    staticMesh.CollisionY1 = reader.ReadInt16();
                    staticMesh.CollisionY2 = reader.ReadInt16();
                    staticMesh.CollisionZ1 = reader.ReadInt16();
                    staticMesh.CollisionZ2 = reader.ReadInt16();
                    staticMesh.Flags = reader.ReadUInt16();
                    StaticMeshes.Add(staticMesh);
                }

                reader.Close();
                logger.Info("Wad loaded successfully.");
            }

            // Read sounds
            logger.Info("Reading sound (sfx/sam) files associated with wad.");
            var soundMapSize = TrCatalog.GetSoundMapSize(WadTombRaiderVersion.TR4, Version == 130);
            using (var readerSounds = new StreamReader(new FileStream(BasePath + "\\" + BaseName + ".sam", FileMode.Open, FileAccess.Read, FileShare.Read)))
                    while (!readerSounds.EndOfStream)
                        Sounds.Add(readerSounds.ReadLine());

            using (var readerSfx = new BinaryReaderEx(new FileStream(BasePath + "\\" + BaseName + ".sfx", FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                SoundMap = new short[soundMapSize];
                for (var i = 0; i < soundMapSize; i++)
                {
                    SoundMap[i] = readerSfx.ReadInt16();
                }

                var numSounds = readerSfx.ReadUInt32();
                logger.Info("Sounds: " + numSounds);
                for (var i = 0; i < numSounds; i++)
                {
                    var info = new wad_sound_info();
                    info.Sample = readerSfx.ReadUInt16();
                    info.Volume = readerSfx.ReadByte();
                    info.Range = readerSfx.ReadByte();
                    info.Chance = readerSfx.ReadByte();
                    info.Pitch = readerSfx.ReadByte();
                    info.Characteristics = readerSfx.ReadUInt16();
                    SoundInfo.Add(info);
                }
            }

            // Read sprites
            logger.Info("Reading sprites (swd file) associated with wad.");
            using (var readerSprites = new BinaryReaderEx(new FileStream(BasePath + Path.DirectorySeparatorChar + BaseName + ".swd",
                                                                         FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Version
                readerSprites.ReadUInt32();

                var numSpritesTextures = readerSprites.ReadUInt32();
                logger.Info("Sprites: " + numSpritesTextures);

                //Sprite texture array
                for (var i = 0; i < numSpritesTextures; i++)
                {
                    var buffer = readerSprites.ReadBytes(16);

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
                var spriteDataSize = readerSprites.ReadInt32();
                SpriteData = readerSprites.ReadBytes(spriteDataSize);

                var numSequences = readerSprites.ReadUInt32();
                logger.Info("Sprite sequences: " + numSequences);

                // Sprite sequences
                for (var i = 0; i < numSequences; i++)
                {
                    var sequence = new wad_sprite_sequence();
                    sequence.ObjectID = readerSprites.ReadInt32();
                    sequence.NegativeLength = readerSprites.ReadInt16();
                    sequence.Offset = readerSprites.ReadInt16();
                    SpriteSequences.Add(sequence);
                }
            }

            // Read WAS
            using (var reader = new StreamReader(File.OpenRead(BasePath + Path.DirectorySeparatorChar + BaseName + ".was")))
            {
                while (!reader.EndOfStream)
                    LegacyNames.Add(reader.ReadLine().Split(':')[0].Replace(" ", ""));
            }
        }

        internal int GetNextMoveableWithAnimations(int current)
        {
            for (int i = current + 1; i < Moveables.Count; i++)
                if (Moveables[i].AnimationIndex != -1)
                    return i;
            return -1;
        }

        internal wad_mesh GetMeshFromPointer(int pointer)
        {
            return Meshes[(int)RealPointers[pointer]];
        }
    }
}
