using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;

namespace TombLib.LevelData.Compilers.TR5Main
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_sprite_texture
    {
        public int Tile;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public float X3;
        public float Y3;
        public float X4;
        public float Y4;
    }

    public enum tr5main_collision_type : short
    {
        Flat,
        Slope,
        TrianglesXminZmin,
        TrianglesXmaxZmin,
        TriangleNoCollisionXminZmin,
        TriangleNoCollisionXminZmax,
        TriangleNoCollisionXmaxZmin,
        TriangleNoCollisionXmaxZmax,
        DiagonalStepXminZmin,
        DiagonalStepXmaxZmin,
        DiagonalStepNoCollisionXminZmin,
        DiagonalStepNoCollisionXminZmax,
        DiagonalStepNoCollisionXmaxZmin,
        DiagonalStepNoCollisionXmaxZmax
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_collision_info
    {
        public tr5main_collision_type Type;
        public short Adjust1;
        public short Adjust2;
        public short CornerXminZmin;
        public short CornerXminZmax;
        public short CornerXmaxZmin;
        public short CornerXmaxZmax;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_room_sector
    {
        public int FloorDataIndex;
        public int BoxIndex;
        public short StepSound;
        public short Stopper;
        public short RoomBelow;
        public int Floor;
        public short RoomAbove;
        public int Ceiling;
        public bool IsDeath;
        public bool IsMonkey;
        public byte IsClimbable;
        public bool HasTriggers;
        public tr5main_collision_info FloorCollision;
        public tr5main_collision_info CeilingCollision;
    }

    public class tr5main_room
    {
        public tr_room_info Info;
        public uint NumDataWords;
        public List<tr_room_vertex> Vertices;
        public List<tr_face4> Quads;
        public List<tr_face3> Triangles;
        public ushort NumSprites;
        public List<tr_room_portal> Portals;
        public ushort NumZSectors;
        public ushort NumXSectors;
        public tr_room_sector[] Sectors;
        public uint AmbientIntensity;
        public short LightMode;
        public List<tr4_room_light> Lights;
        public List<tr_room_staticmesh> StaticMeshes;
        public short AlternateRoom;
        public short Flags;
        public byte WaterScheme;
        public byte ReverbInfo;
        public byte AlternateGroup;

        // Helper data
        public TrSectorAux[,] AuxSectors;
        public AlternateKind AlternateKind;
        public List<Room> ReachableRooms;
        public bool Visited;
        public bool Flipped;
        public Room FlippedRoom;
        public Room BaseRoom;
        public Room OriginalRoom;

        public void WriteTr5(BinaryWriterEx writer)
        {
            var roomStartOffset = writer.BaseStream.Position;

            var xela = System.Text.Encoding.ASCII.GetBytes("XELA");
            writer.Write(xela);

            var startOfRoomPosition = writer.BaseStream.Position;
            var roomDataSize = 0;
            writer.Write((uint)roomDataSize);

            var separator = 0xcdcdcdcd;
            writer.Write(separator);

            var EndSDOffsetPosition = writer.BaseStream.Position;
            var EndSDOffset = (uint)0;
            writer.Write(EndSDOffset);

            var StartOfSDOffsetPosition = writer.BaseStream.Position;
            var StartOfSDOffset = (uint)0;
            writer.Write(StartOfSDOffset);

            writer.Write(separator);

            var EndPortalOffsetPosition = writer.BaseStream.Position;
            var EndPortalOffset = (uint)0;
            writer.Write(EndPortalOffset);

            // tr5_room_info
            writer.Write(Info.X);
            writer.Write(0);
            writer.Write(Info.Z);
            writer.Write(Info.YBottom);
            writer.Write(Info.YTop);

            writer.Write(NumZSectors);
            writer.Write(NumXSectors);

            writer.Write(AmbientIntensity);

            writer.Write((ushort)Lights.Count);
            writer.Write((ushort)StaticMeshes.Count);

            writer.Write(ReverbInfo);
            writer.Write(AlternateGroup);
            writer.Write(WaterScheme);
            writer.Write((byte)0);

            writer.Write((uint)0x00007fff);
            writer.Write((uint)0x00007fff);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xffffffff);

            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write((ushort)0xffff);

            writer.Write((ushort)0);
            writer.Write((uint)0);
            writer.Write((uint)0);

            writer.Write(0xcdcdcdcd);
            writer.Write((uint)0);

            writer.Write((float)Info.X);
            writer.Write((float)Info.YBottom);
            writer.Write((float)Info.Z);

            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write((uint)0);
            writer.Write(0xcdcdcdcd);

            writer.Write((uint)Triangles.Count);
            writer.Write((uint)Quads.Count);

            writer.Write((uint)0);

            writer.Write((uint)(Lights.Count * 88));
            writer.Write((uint)Lights.Count);

            writer.Write((uint)0);

            writer.Write(Info.YTop);
            writer.Write(Info.YBottom);

            writer.Write((uint)1);

            var LayerOffsetPosition = writer.BaseStream.Position;
            var LayerOffset = 0;
            writer.Write(LayerOffset);

            var VerticesOffsetPosition = writer.BaseStream.Position;
            var VerticesOffset = 0;
            writer.Write(VerticesOffset);

            var PolyOffsetPosition = writer.BaseStream.Position;
            var PolyOffset = 0;
            writer.Write(PolyOffset);
            writer.Write(PolyOffset);

            writer.Write((uint)(Vertices.Count * 28));

            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);
            writer.Write(0xcdcdcdcd);

            // Start of room data (after 216 bytes from XELA)
            foreach (var light in Lights)
            {
                writer.Write((float)light.X);
                writer.Write((float)light.Y);
                writer.Write((float)light.Z);
                writer.Write(light.Color.Red / 255.0f);
                writer.Write(light.Color.Green / 255.0f);
                writer.Write(light.Color.Blue / 255.0f);

                writer.Write(0xcdcdcdcd);

                writer.Write(light.In);
                writer.Write(light.Out);

                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.In) * 2.0f : 0));
                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.Out) * 2.0f : 0));
                writer.Write(light.CutOff);

                writer.Write(light.DirectionX);
                writer.Write(light.DirectionY);
                writer.Write(light.DirectionZ);

                writer.Write(light.X);
                writer.Write(light.Y);
                writer.Write(light.Z);

                writer.Write((int)light.DirectionX);
                writer.Write((int)light.DirectionY);
                writer.Write((int)light.DirectionZ);

                writer.Write(light.LightType);

                writer.Write((byte)0xcd);
                writer.Write((byte)0xcd);
                writer.Write((byte)0xcd);
            }

            StartOfSDOffset = (uint)(writer.BaseStream.Position - roomStartOffset - 216);
            writer.WriteBlockArray(Sectors);
            EndSDOffset = (uint)(StartOfSDOffset + NumXSectors * NumZSectors * 8);

            writer.Write((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            writer.Write((ushort)0xcdcd);
            EndPortalOffset = (uint)(writer.BaseStream.Position - roomStartOffset - 216);

            if (StaticMeshes.Count != 0)
                writer.WriteBlockArray(StaticMeshes);

            LayerOffset = (int)(writer.BaseStream.Position - roomStartOffset - 216);

            // Write just one layer
            writer.Write((uint)Vertices.Count);
            writer.Write((ushort)0);
            writer.Write((ushort)Quads.Count);
            writer.Write((ushort)Triangles.Count);
            writer.Write((ushort)0);

            writer.Write((ushort)0);
            writer.Write((ushort)0);

            writer.Write(1024.0f);
            writer.Write((float)Info.YBottom);
            writer.Write(1024.0f);
            writer.Write((NumXSectors - 1) * 1024.0f);
            writer.Write((float)Info.YTop);
            writer.Write((NumZSectors - 1) * 1024.0f);

            writer.Write((uint)0);
            var LayerVerticesOffset = writer.BaseStream.Position;
            writer.Write((uint)0);
            var LayerPolygonsOffset = writer.BaseStream.Position;
            writer.Write((uint)0);
            writer.Write((uint)0);

            PolyOffset = LayerOffset + 56;

            for (var k = 0; k < Quads.Count; k++)
            {
                Quads[k].Write(writer);
                writer.Write((ushort)0);
            }
            for (var k = 0; k < Triangles.Count; k++)
            {
                Triangles[k].Write(writer);
                writer.Write((ushort)0);
            }

            if (Triangles.Count % 2 != 0)
                writer.Write((ushort)0xcdcd);

            VerticesOffset = (int)(writer.BaseStream.Position - roomStartOffset - 216);

            foreach (var vertex in Vertices)
            {
                writer.Write((float)vertex.Position.X);
                writer.Write((float)vertex.Position.Y);
                writer.Write((float)vertex.Position.Z);
                writer.Write((float)vertex.Normal.X);
                writer.Write((float)vertex.Normal.Y);
                writer.Write((float)vertex.Normal.Z);
                writer.Write(vertex.Color);
            }

            var endOfRoomOffset = writer.BaseStream.Position;
            roomDataSize = (int)(endOfRoomOffset - roomStartOffset - 8);

            writer.Seek((int)startOfRoomPosition, SeekOrigin.Begin);
            writer.Write(roomDataSize);

            writer.Seek((int)EndSDOffsetPosition, SeekOrigin.Begin);
            writer.Write((int)EndSDOffset);
            writer.Write((int)StartOfSDOffset);

            writer.Seek((int)EndPortalOffsetPosition, SeekOrigin.Begin);
            writer.Write((int)EndPortalOffset);

            writer.Seek((int)LayerOffsetPosition, SeekOrigin.Begin);
            writer.Write(LayerOffset);
            writer.Write(VerticesOffset);
            writer.Write(PolyOffset);
            writer.Write(PolyOffset);

            writer.Seek((int)LayerVerticesOffset, SeekOrigin.Begin);
            writer.Write(VerticesOffset);
            writer.Write(PolyOffset);
            writer.Write(PolyOffset);

            writer.Seek((int)endOfRoomOffset, SeekOrigin.Begin);
        }

    }

}
