using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TombEditor.Geometry;
using TombLib.IO;

namespace TombEditor.Compilers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_color
    {
        public byte Red;
        public byte Green;
        public byte Blue;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_vertex
    {
        public short X;
        public short Y;
        public short Z;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static unsafe bool operator ==(tr_vertex first, tr_vertex second)
        {
            return (first.X == second.X) && (first.Y == second.Y) && (first.Z == second.Z);
        }

        public static bool operator !=(tr_vertex first, tr_vertex second)
        {
            return !(first == second);
        }

        public bool Equals(tr_vertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (tr_vertex)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_face4
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
    public struct tr_face3
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public tr_vertex[] Vertices;
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
    public struct tr_room_vertex : IEquatable<tr_room_vertex>
    {
        public tr_vertex Position;
        public ushort Lighting1;
        public ushort Attributes;
        public ushort Lighting2;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static unsafe bool operator ==(tr_room_vertex first, tr_room_vertex second)
        {
            return (*(ulong*)&first == *(ulong*)&second) && (*(uint*)&first.Attributes == *(uint*)&second.Attributes);
        }

        public static bool operator !=(tr_room_vertex first, tr_room_vertex second)
        {
            return !(first == second);
        }

        public bool Equals(tr_room_vertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (tr_room_vertex)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_room_staticmesh
    {
        public int X;
        public int Y;
        public int Z;
        public ushort Rotation;
        public ushort Intensity1;
        public ushort Intensity2;
        public ushort ObjectID;
    }
    
    public class tr_room
    {
        public tr_room_info Info;
        public uint NumDataWords;
        public List<tr_room_vertex> Vertices;
        public List<tr_face4> Quads;
        public List<tr_face3> Triangles;
        public ushort NumSprites;
        public ushort NumPortals;
        public tr_room_portal[] Portals;
        public ushort NumZSectors;
        public ushort NumXSectors;
        public tr_room_sector[] Sectors;
        public uint AmbientIntensity;
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
        public TrSectorAux[,] AuxSectors;
        
        public List<Room> ReachableRooms;
        public bool Visited;
        public bool Flipped;
        public Room FlippedRoom;
        public Room BaseRoom;
        public Room OriginalRoom;

        public void Write(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;
            
            writer.Write((int)0);
            
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
            if (Lights.Length != 0)
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
    public struct tr_mesh
    {
        public tr_vertex Center;
        public int Radius;
        public short NumVertices;
        public tr_vertex[] Vertices;
        public short NumNormals;
        public tr_vertex[] Normals;
        public short[] Lights;
        public short NumTexturedQuads;
        public tr_face4[] TexturedQuads;
        public short NumTexturedTriangles;
        public tr_face3[] TexturedTriangles;
        public short NumColoredRectangles;
        public tr_face4[] ColoredRectangles;
        public short NumColoredTriangles;
        public tr_face3[] ColoredTriangles;
        public int MeshSize;
        public int MeshPointer;

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
            if (meshSize % 4 != 0)
            {
                const ushort tempFiller = 0;
                writer.Write(tempFiller);
                meshSize += 2;
            }

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
    public struct tr_staticmesh
    {
        public uint ObjectID;
        public ushort Mesh;
        public tr_bounding_box VisibilityBox;
        public tr_bounding_box CollisionBox;
        public ushort Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_animatedTextures_set
    {
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
        public short Ocb;
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
    public struct tr_state_change
    {
        public ushort StateID;
        public ushort NumAnimDispatches;
        public ushort AnimDispatch;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr_anim_dispatch
    {
        public ushort Low;
        public ushort High;
        public ushort NextAnimation;
        public ushort NextFrame;
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
        public short OverlapIndex;
        public bool IsolatedBox;
        public bool NotWalkableBox;
        public bool Monkey;
        public bool Jump;
        public short Room;
        public bool Portal;
        public bool Flag0x02;
        public bool Flag0x04;
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

    public class TrSectorAux
    {
        public bool Wall;
        public bool SoftSlope;
        public bool HardSlope;
        public bool Monkey;
        public bool Box;
        public bool BorderWall;
        public bool Portal;
        public bool NotWalkableFloor;
        public Room WallPortal;
        public Portal FloorPortal;
        public short LowestFloor;
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
        public byte Volume;
        public byte Range;
        public byte Chance;
        public byte Pitch;
        public ushort Characteristics;
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
}
