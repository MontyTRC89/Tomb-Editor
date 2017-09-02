using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using TombEditor.Geometry;
using TombLib.IO;

namespace TombEditor.Compilers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrVertex
    {
        public short X;
        public short Y;
        public short Z;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static bool operator ==(TrVertex first, TrVertex second)
        {
            return (first.X == second.X) && (first.Y == second.Y) && (first.Z == second.Z);
        }

        public static bool operator !=(TrVertex first, TrVertex second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);
            return this == (TrVertex)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrFace4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public ushort[] Vertices;
        public ushort Texture;
        public ushort LightingEffect;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(Vertices[0]);
            writer.Write(Vertices[1]);
            writer.Write(Vertices[2]);
            writer.Write(Vertices[3]);
            writer.Write(Texture);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrFace3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public ushort[] Vertices;
        public ushort Texture;
        public ushort LightingEffect;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(Vertices[0]);
            writer.Write(Vertices[1]);
            writer.Write(Vertices[2]);
            writer.Write(Texture);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrRoomInfo
    {
        public int X;
        public int Z;
        public int YBottom;
        public int YTop;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrRoomPortal
    {
        public ushort AdjoiningRoom;
        public TrVertex Normal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public TrVertex[] Vertices;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrRoomSector
    {
        public ushort FloorDataIndex;
        public short BoxIndex;
        public byte RoomBelow;
        public sbyte Floor;
        public byte RoomAbove;
        public sbyte Ceiling;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tr4RoomLight
    {
        public int X;
        public int Y;
        public int Z;
        public TrColor Color;
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
    public struct TrRoomVertex : IEquatable<TrRoomVertex>
    {
        public TrVertex Position;
        public ushort Lighting1;
        public ushort Attributes;
        public ushort Lighting2;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static unsafe bool operator ==(TrRoomVertex first, TrRoomVertex second)
        {
            return (*(ulong*)&first == *(ulong*)&second) && (*(uint*)&first.Attributes == *(uint*)&second.Attributes);
        }

        public static bool operator !=(TrRoomVertex first, TrRoomVertex second)
        {
            return !(first == second);
        }

        public bool Equals(TrRoomVertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);
            return this == (TrRoomVertex)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrRoomStaticmesh
    {
        public int X;
        public int Y;
        public int Z;
        public ushort Rotation;
        public ushort Intensity1;
        public ushort Intensity2;
        public ushort ObjectID;
    }
    
    public class TrRoom
    {
        public TrRoomInfo Info;
        public List<TrRoomVertex> Vertices;
        public List<TrFace4> Quads;
        public List<TrFace3> Triangles;
        public TrRoomPortal[] Portals;
        public ushort NumZSectors;
        public ushort NumXSectors;
        public TrRoomSector[] Sectors;
        public uint AmbientIntensity;
        public Tr4RoomLight[] Lights;
        public TrRoomStaticmesh[] StaticMeshes;
        public short AlternateRoom;
        public short Flags;
        public byte WaterScheme;
        public byte ReverbInfo;
        public byte AlternateGroup;

        // Helper data
        public TrSectorAux[,] AuxSectors;
        
        public List<Room> ReachableRooms;
        public Room OriginalRoom;

        public void Write(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;
            
            writer.Write(0);
            
            writer.Write((ushort)Vertices.Count);
            writer.WriteBlockArray(Vertices);
            
            writer.Write((ushort)Quads.Count);
            for (var k = 0; k < Quads.Count; k++)
                Quads[k].Write(writer);
            
            writer.Write((ushort)Triangles.Count);
            for (var k = 0; k < Triangles.Count; k++)
                Triangles[k].Write(writer);

            // For sprites, not used
            writer.Write((ushort)0);

            // Now save current offset and calculate the size of the geometry
            var offset2 = writer.BaseStream.Position;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            ushort roomGeometrySize = (ushort) ((offset2 - offset - 4) / 2);

            // Save the size of the geometry
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(roomGeometrySize);
            writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

            // Write portals
            writer.WriteBlock((ushort)Portals.Length);
            if (Portals.Length != 0)
                writer.WriteBlockArray(Portals);

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            writer.WriteBlockArray(Sectors);

            // Write room color
            writer.Write(AmbientIntensity);

            // Write lights
            writer.WriteBlock((ushort)Lights.Length);
            if (Portals.Length != 0)
                writer.WriteBlockArray(Lights);

            // Write static meshes
            writer.WriteBlock((ushort)StaticMeshes.Length);
            if (StaticMeshes.Length != 0)
                writer.WriteBlockArray(StaticMeshes);

            // Write final data
            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write(WaterScheme);
            writer.Write(ReverbInfo);
            writer.Write(AlternateGroup);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrMesh
    {
        public TrVertex Center;
        public int Radius;
        public short NumVertices;
        public TrVertex[] Vertices;
        public short NumNormals;
        public TrVertex[] Normals;
        public short[] Lights;
        public short NumTexturedQuads;
        public TrFace4[] TexturedQuads;
        public short NumTexturedTriangles;
        public TrFace3[] TexturedTriangles;
        private readonly short NumColoredRectangles;
        private readonly TrFace4[] ColoredRectangles;
        private readonly short NumColoredTriangles;
        private readonly TrFace3[] ColoredTriangles;
        private readonly int MeshSize;
        private readonly int MeshPointer;

        public long WriteTr4(BinaryWriterEx writer)
        {
            long meshOffset1 = writer.BaseStream.Position;

            writer.WriteBlock(Center);
            writer.Write(Radius);

            writer.Write(NumVertices);
            writer.WriteBlockArray(Vertices);

            writer.Write(NumNormals);
            if (NumNormals > 0)
            {
                writer.WriteBlockArray(Normals);
            }
            else
            {
                writer.WriteBlockArray(Lights);
            }

            writer.Write(NumTexturedQuads);
            writer.WriteBlockArray(TexturedQuads);

            writer.Write(NumTexturedTriangles);
            writer.WriteBlockArray(TexturedTriangles);

            var meshOffset2 = writer.BaseStream.Position;
            var meshSize = (meshOffset2 - meshOffset1);
            if (meshSize % 4 == 0)
                return meshSize;
            
            const ushort tempFiller = 0;
            writer.Write(tempFiller);
            meshSize += 2;

            return meshSize;
        }

        public long WriteTr3(BinaryWriterEx writer)
        {
            var meshOffset1 = writer.BaseStream.Position;

            writer.WriteBlock(Center);
            writer.Write(Radius);

            writer.Write(NumVertices);
            writer.WriteBlockArray(Vertices);

            writer.Write(NumNormals);
            if (NumNormals > 0)
            {
                writer.WriteBlockArray(Normals);
            }
            else
            {
                writer.WriteBlockArray(Lights);
            }

            writer.Write(NumTexturedQuads);
            for (var k = 0; k < NumTexturedQuads; k++)
            {
                TexturedQuads[k].Write(writer);
            }
            // writer.WriteBlockArray(Meshes[i].TexturedRectangles);

            writer.Write(NumTexturedTriangles);
            for (var k = 0; k < NumTexturedTriangles; k++)
            {
                TexturedTriangles[k].Write(writer);
            }

            //  writer.WriteBlockArray(Meshes[i].TexturedTriangles);

            writer.Write(NumColoredRectangles);
            //writer.WriteBlockArray(Meshes[i].ColoredRectangles);

            writer.Write(NumColoredTriangles);
            //writer.WriteBlockArray(Meshes[i].ColoredTriangles);

            var meshOffset2 = writer.BaseStream.Position;
            var meshSize = meshOffset2 - meshOffset1;
            if (meshSize % 4 != 0)
            {
                const ushort tempFiller = 0;
                writer.Write(tempFiller);
                meshSize += 2;
            }

            return meshSize;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrStaticmesh
    {
        public uint ObjectID;
        public ushort Mesh;
        public TrBoundingBox VisibilityBox;
        public TrBoundingBox CollisionBox;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrAnimatedTexturesSet
    {
        public short[] Textures;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrBoundingBox
    {
        public short X1;
        public short X2;
        public short Y1;
        public short Y2;
        public short Z1;
        public short Z2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrMoveable
    {
        public uint ObjectID;
        public ushort NumMeshes;
        public ushort StartingMesh;
        public uint MeshTree;
        public uint FrameOffset;
        public ushort Animation;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrItem
    {
        public short ObjectID;
        public short Room;
        public int X;
        public int Y;
        public int Z;
        public short Angle;
        public short Intensity1;
        public short Ocb;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrSpriteTexture
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
    public struct TrSpriteSequence
    {
        public int ObjectID;
        public short NegativeLength;
        public short Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrAnimation
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

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(FrameOffset);
            writer.Write(FrameRate);
            writer.Write(FrameSize);
            writer.Write(StateID);
            writer.Write(Speed);
            writer.Write(Accel);
            writer.Write(FrameStart);
            writer.Write(FrameEnd);
            writer.Write(NextAnimation);
            writer.Write(NextFrame);
            writer.Write(NumStateChanges);
            writer.Write(StateChangeOffset);
            writer.Write(NumAnimCommands);
            writer.Write(AnimCommand);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrStateChange
    {
        public ushort StateID;
        public ushort NumAnimDispatches;
        public ushort AnimDispatch;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrAnimDispatch
    {
        public short Low;
        public short High;
        public short NextAnimation;
        public short NextFrame;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrBox
    {
        public byte Zmin;
        public byte Zmax;
        public byte Xmin;
        public byte Xmax;
        public short TrueFloor;
        public short OverlapIndex;
    }

    public struct TrBoxAux
    {
        public byte Zmin;
        public byte Zmax;
        public byte Xmin;
        public byte Xmax;
        public short TrueFloor;
        public short Clicks;
        public byte RoomBelow;
        public short OverlapIndex;
        public int ZoneId;
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
        public bool Water;
    }

    public struct TrOverlapAux
    {
        public int MainBox;
        public int Box;
        public bool Skeleton;
        public bool Monkey;
        public bool EndOfList;
        public bool IsEdge;
    }

    public class TrSectorAux
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
        public Room WallPortal;
        public Portal FloorPortal;
        public short LowestFloor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrZone
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
    public struct TrSoundSource
    {
        public int X;
        public int Y;
        public int Z;
        public ushort SoundID;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrSoundDetails
    {
        public short Sample;
        public short Volume;
        public short Unknown1;
        public short Unknown2;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrCamera
    {
        public int X;
        public int Y;
        public int Z;
        public short Room;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrAnimatedTextures
    {
        public short NumTextureID;
        public short[] TextureID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrAiItem
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
    public struct Tr4FlybyCamera
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
}
