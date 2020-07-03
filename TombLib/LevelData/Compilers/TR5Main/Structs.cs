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

    public enum tr5main_polygon_shape : int
    {
        Quad,
        Triangle
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

    public class tr5main_polygon
    {
        public tr5main_polygon_shape Shape;
        public List<int> Indices = new List<int>();
        public int TextureId;
        public byte BlendMode;
        public bool Animated;
        public int BaseIndex;
    }

    public class tr5main_vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoords;
        public Vector4 Color;
        public int Bone;
        public int Effects;
        public int Index;

        public bool IsOnPortal;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        /*public static bool operator ==(tr5main_vertex first, tr5main_vertex second)
        {
            return first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        }

        public static bool operator !=(tr5main_vertex first, tr5main_vertex second)
        {
            return !(first == second);
        }

        public bool Equals(tr5main_vertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is tr5main_vertex))
                return false;
            return this == (tr5main_vertex)obj;
        }

        public override int GetHashCode()
        {
            return unchecked(X + Y * 695504311 + Z * 550048883);
        }*/
    }

    public class tr5main_material
    {
        public class Tr5MainMaterialComparer : IEqualityComparer<tr5main_material>
        {
            public bool Equals(tr5main_material x, tr5main_material y)
            {
                return (x.Texture == y.Texture && x.BlendMode == y.BlendMode && x.Animated == y.Animated);
            }

            public int GetHashCode(tr5main_material obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + obj.Texture.GetHashCode();
                    hash = hash * 23 + obj.BlendMode.GetHashCode();
                    hash = hash * 23 + obj.Animated.GetHashCode();
                    return hash;
                }
            }
        }

        public int Texture;
        public byte BlendMode;
        public bool Animated;
    }

    public class tr5main_bucket
    {
        public tr5main_material Material;
        public List<int> Indices;
        public List<tr5main_polygon> Polygons;

        public tr5main_bucket()
        {
            Indices = new List<int>();
            Polygons = new List<tr5main_polygon>();
        }
    }

    public class tr5main_room
    {
        public tr_room_info Info;
        public int NumDataWords;
        public List<tr5main_vertex> Vertices;
        public Dictionary<tr5main_material, tr5main_bucket> Buckets;
        public List<tr_room_portal> Portals;
        public int NumZSectors;
        public int NumXSectors;
        public tr5main_room_sector[] Sectors;
        public Vector3 AmbientLight;
        public List<tr4_room_light> Lights;
        public List<tr_room_staticmesh> StaticMeshes;
        public int AlternateRoom;
        public int Flags;
        public int WaterScheme;
        public int ReverbInfo;
        public int AlternateGroup;

        // Helper data
        public List<tr5main_polygon> Polygons;
        public TrSectorAux[,] AuxSectors;
        public AlternateKind AlternateKind;
        public List<Room> ReachableRooms;
        public bool Visited;
        public bool Flipped;
        public Room FlippedRoom;
        public Room BaseRoom;
        public Room OriginalRoom;

        public void Write(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            writer.Write(Vertices.Count);
            for (var k = 0; k < Vertices.Count; k++)
            {
                writer.Write(Vertices[k].Position.X);
                writer.Write(Vertices[k].Position.Y);
                writer.Write(Vertices[k].Position.Z);
                writer.Write(Vertices[k].Normal.X);
                writer.Write(Vertices[k].Normal.Y);
                writer.Write(Vertices[k].Normal.Z);
                writer.Write(Vertices[k].TextureCoords.X);
                writer.Write(Vertices[k].TextureCoords.Y);
                writer.Write(Vertices[k].Color.X);
                writer.Write(Vertices[k].Color.Y);
                writer.Write(Vertices[k].Color.Z);
                writer.Write(Vertices[k].Effects);
                writer.Write(Vertices[k].Index);
            }

            writer.Write(Buckets.Count);
            foreach (var bucket in Buckets.Values)
            {
                writer.Write(bucket.Material.Texture);
                writer.Write(bucket.Material.BlendMode);
                writer.Write(bucket.Material.Animated);
                writer.Write(bucket.Indices.Count);
                foreach (var index in bucket.Indices)
                    writer.Write(index);
            }            
    
            // Write portals
            writer.WriteBlock(Portals.Count);
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
                writer.Write((int)s.FloorCollision.NoCollision);
                writer.Write(s.FloorCollision.Planes[0]);
                writer.Write(s.FloorCollision.Planes[1]);
                writer.Write((int)s.CeilingCollision.Split);
                writer.Write((int)s.CeilingCollision.NoCollision);
                writer.Write(s.CeilingCollision.Planes[0]);
                writer.Write(s.CeilingCollision.Planes[1]);
            }

            // Write room color
            writer.Write(AmbientLight.X);
            writer.Write(AmbientLight.Y);
            writer.Write(AmbientLight.Z);

            // Write lights
            writer.WriteBlock(Lights.Count);
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
            writer.WriteBlock(StaticMeshes.Count);
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

    public class tr5main_mesh
    {
        public BoundingSphere Sphere;
        public List<tr5main_vertex> Vertices;
        public List<tr5main_polygon> Polygons;
        public Dictionary<tr5main_material, tr5main_bucket> Buckets;
    }
}
