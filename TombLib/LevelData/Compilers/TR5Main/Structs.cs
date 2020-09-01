using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

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
    public class tr5main_atlas
    {
        public ImageC ColorMap;
        public ImageC NormalMap;
        public bool HasNormalMap;
        public bool CustomNormalMap;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_polygon
    {
        public tr5main_polygon_shape Shape;
        public List<int> Indices = new List<int>();
        public List<Vector2> TextureCoordinates = new List<Vector2>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Tangents = new List<Vector3>();
        public List<Vector3> Bitangents = new List<Vector3>();
        public int TextureId;
        public byte BlendMode;
        public bool Animated;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_room_staticmesh
    {
        public int X;
        public int Y;
        public int Z;
        public ushort Rotation;
        public ushort Intensity1;
        public ushort Intensity2;
        public ushort ObjectID;
        public short HitPoints;
    }

    public class NormalHelper
    {
        public tr5main_polygon Polygon;
        public bool Smooth;

        public NormalHelper(tr5main_polygon poly)
        {
            Polygon = poly;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoords;
        public Vector3 Color;
        public Vector3 Tangent;
        public Vector3 Bitangent;
        public int Bone;
        public int Effects;
        public int IndexInPoly;
        public int OriginalIndex;

        public List<NormalHelper> Polygons = new List<NormalHelper>();
        public bool IsOnPortal;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static bool operator ==(tr5main_vertex first, tr5main_vertex second)
        {
            return first.Position.X == second.Position.X && first.Position.Y == second.Position.Y && first.Position.Z == second.Position.Z;
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
            return unchecked((int)Position.X + (int)Position.Y * 695504311 + (int)Position.Z * 550048883);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_material
    {
        public class Tr5MainMaterialComparer : IEqualityComparer<tr5main_material>
        {
            public bool Equals(tr5main_material x, tr5main_material y)
            {
                return (x.Texture == y.Texture && x.BlendMode == y.BlendMode && x.Animated == y.Animated && x.NormalMapping == y.NormalMapping);
            }

            public int GetHashCode(tr5main_material obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + obj.Texture.GetHashCode();
                    hash = hash * 23 + obj.BlendMode.GetHashCode();
                    hash = hash * 23 + obj.Animated.GetHashCode();
                    hash = hash * 23 + obj.NormalMapping.GetHashCode();
                    return hash;
                }
            }
        }

        public int Texture;
        public byte BlendMode;
        public bool Animated;
        public bool NormalMapping;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_bucket
    {
        public tr5main_material Material;
        public List<tr5main_polygon> Polygons;

        public tr5main_bucket()
        {
            Polygons = new List<tr5main_polygon>();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_room
    {
        public tr_room_info Info;
        public int NumDataWords;
        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Tangents = new List<Vector3>();
        public List<Vector3> Bitangents = new List<Vector3>();
        public List<Vector3> Colors = new List<Vector3>();
        public List<tr5main_vertex> Vertices = new List<tr5main_vertex>();
        public Dictionary<tr5main_material, tr5main_bucket> Buckets;
        public List<tr_room_portal> Portals;
        public int NumZSectors;
        public int NumXSectors;
        public tr5main_room_sector[] Sectors;
        public Vector3 AmbientLight;
        public List<tr5main_room_light> Lights;
        public List<tr5main_room_staticmesh> StaticMeshes;
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

            writer.Write(Positions.Count);
            foreach (var p in Positions)
                writer.Write(p);
            foreach (var n in Normals)
                writer.Write(n);
            foreach (var c in Colors)
                writer.Write(c);

            writer.Write(Buckets.Count);
            foreach (var bucket in Buckets.Values)
            {
                writer.Write(bucket.Material.Texture);
                writer.Write(bucket.Material.BlendMode);
                writer.Write(bucket.Material.Animated);
                writer.Write(bucket.Polygons.Count);
                foreach (var poly in bucket.Polygons)
                {
                    writer.Write((int)poly.Shape);
                    foreach (int index in poly.Indices)
                        writer.Write(index);
                    foreach (var uv in poly.TextureCoordinates)
                        writer.Write(uv);
                    foreach (var n in poly.Normals)
                        writer.Write(n);
                    foreach (var t in poly.Tangents)
                        writer.Write(t);
                    foreach (var bt in poly.Bitangents)
                        writer.Write(bt);
                }
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
                writer.Write((int)light.Position.X);
                writer.Write((int)light.Position.Y);
                writer.Write((int)light.Position.Z);
                writer.Write(light.Direction.X);
                writer.Write(light.Direction.Y);
                writer.Write(light.Direction.Z);
                writer.Write(light.Color.X);
                writer.Write(light.Color.Y);
                writer.Write(light.Color.Z);
                writer.Write(light.Intensity);
                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.In) * 2.0f : light.In));
                writer.Write((float)(light.LightType == 2 ? Math.Acos(light.Out) * 2.0f : light.Out));
                writer.Write(light.Length);
                writer.Write(light.CutOff);
                writer.Write(light.LightType);
                writer.Write((byte)(light.CastShadows ? 1 : 0));
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_room_light
    {
        public VectorInt3 Position;
        public Vector3 Direction;
        public Vector3 Color;
        public float Intensity;
        public float In;
        public float Out;
        public float Length;
        public float CutOff;
        public byte LightType;
        public bool CastShadows;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_mesh
    {
        public BoundingSphere Sphere;
        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Colors = new List<Vector3>();
        public List<int> Bones = new List<int>();
        public List<tr5main_polygon> Polygons = new List<tr5main_polygon>();
        public Dictionary<tr5main_material, tr5main_bucket> Buckets = new Dictionary<tr5main_material, tr5main_bucket>();
        public List<tr5main_vertex> Vertices = new List<tr5main_vertex>();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_box
    {
        public int Zmin;
        public int Zmax;
        public int Xmin;
        public int Xmax;
        public int TrueFloor;
        public int OverlapIndex;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_overlap
    {
        public int Box;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class tr5main_zone
    {
        public int GroundZone1_Normal;
        public int GroundZone2_Normal;
        public int GroundZone3_Normal;
        public int GroundZone4_Normal;
        public int GroundZone5_Normal;
        public int FlyZone_Normal;
        public int GroundZone1_Alternate;
        public int GroundZone2_Alternate;
        public int GroundZone3_Alternate;
        public int GroundZone4_Alternate;
        public int GroundZone5_Alternate;
        public int FlyZone_Alternate;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_camera
    {
        public int X;
        public int Y;
        public int Z;
        public int Room;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_sound_source
    {
        public int X;
        public int Y;
        public int Z;
        public int SoundID;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_staticmesh
    {
        public int ObjectID;
        public int Mesh;
        public tr5main_bounding_box VisibilityBox;
        public tr5main_bounding_box CollisionBox;
        public ushort Flags;
        public short ShatterType;
        public short ShatterDamage;
        public short ShatterSound;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_moveable
    {
        public int ObjectID;
        public short NumMeshes;
        public short StartingMesh;
        public int MeshTree;
        public int FrameOffset;
        public short Animation;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_animation
    {
        public int FrameOffset;
        public short FrameRate;
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
    public class tr5main_keyframe
    {
        public tr5main_bounding_box BoundingBox;
        public Vector3 Offset;
        public List<Quaternion> Angles;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_bounding_box
    {
        public short X1;
        public short X2;
        public short Y1;
        public short Y2;
        public short Z1;
        public short Z2;
    }
}
