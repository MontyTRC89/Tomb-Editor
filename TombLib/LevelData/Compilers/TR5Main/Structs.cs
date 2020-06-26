using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

    public enum tr5main_split_type : int
    {
        None,
        Split1,
        Split2
    }

    public enum tr5main_nocollision_type : int
    {
        None,
        Triangle1,
        Triangle2
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_collision_info
    {
        public tr5main_split_type Split;
        public tr5main_nocollision_type NoCollision;
        public Vector3[] Planes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_room_sector
    {
        public int FloorDataIndex;
        public int BoxIndex;
        public int StepSound;
        public int Stopper;
        public int RoomBelow;
        public int Floor;
        public int RoomAbove;
        public int Ceiling;
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
        public tr5main_room_sector[] Sectors;
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
            writer.WriteBlock(Info);

            var offset = writer.BaseStream.Position;

            writer.Write(0);

            writer.Write((ushort)Vertices.Count);
            for (var k = 0; k < Vertices.Count; k++)
            {
                writer.Write(Vertices[k].Position.X);
                writer.Write(Vertices[k].Position.Y);
                writer.Write(Vertices[k].Position.Z);
                writer.Write(Vertices[k].Color);
                writer.Write(Vertices[k].Attributes);
            }

            writer.Write((ushort)Quads.Count);
            for (var k = 0; k < Quads.Count; k++)
                Quads[k].Write(writer);

            writer.Write((ushort)Triangles.Count);
            for (var k = 0; k < Triangles.Count; k++)
                Triangles[k].Write(writer);

            // Now save current offset and calculate the size of the geometry
            var offset2 = writer.BaseStream.Position;
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

            // Save the size of the geometry
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write(roomGeometrySize);
            writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

            // Write portals
            writer.WriteBlock((ushort)Portals.Count);
            if (Portals.Count != 0)
                writer.WriteBlockArray(Portals);

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            foreach (var s in Sectors)
            {
                writer.Write(s.FloorDataIndex);
                writer.Write(s.BoxIndex);
                writer.Write(s.StepSound);
                writer.Write(s.Stopper);
                writer.Write(s.RoomBelow);
                writer.Write(s.Floor);
                writer.Write(s.RoomAbove);
                writer.Write(s.Ceiling);
                writer.Write((int)s.FloorCollision.Split);
                writer.Write((int)s.FloorCollision.Split);
                writer.Write(s.FloorCollision.Planes[0]);
                writer.Write(s.FloorCollision.Planes[1]);
                writer.Write((int)s.CeilingCollision.Split);
                writer.Write((int)s.CeilingCollision.Split);
                writer.Write(s.CeilingCollision.Planes[0]);
                writer.Write(s.CeilingCollision.Planes[1]);
            }

            // Write room color
            writer.Write(AmbientIntensity);

            // Write lights
            writer.WriteBlock((ushort)Lights.Count);
            foreach (var light in Lights)
            {
                writer.Write((float)light.X);
                writer.Write((float)light.Y);
                writer.Write((float)light.Z);
                writer.Write(light.Color.Red / 255.0f);
                writer.Write(light.Color.Green / 255.0f);
                writer.Write(light.Color.Blue / 255.0f);

                writer.Write(light.In);
                writer.Write(light.Out);

                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.In) * 2.0f : 0));
                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.Out) * 2.0f : 0));
                writer.Write(light.CutOff);

                writer.Write(light.DirectionX);
                writer.Write(light.DirectionY);
                writer.Write(light.DirectionZ);

                writer.Write(light.LightType);
            }

            // Write static meshes
            writer.WriteBlock((ushort)StaticMeshes.Count);
            if (StaticMeshes.Count != 0)
                writer.WriteBlockArray(StaticMeshes);

            // Write final data
            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write(WaterScheme);
            writer.Write(ReverbInfo);
            writer.Write(AlternateGroup);
        }
    }
}
