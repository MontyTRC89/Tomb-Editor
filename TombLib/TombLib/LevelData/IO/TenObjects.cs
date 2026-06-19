using System.Collections.Generic;
using System.Numerics;

namespace TombLib.LevelData.IO
{
    // Intermediate, engine-faithful model of the object section of a compiled .ten level. It mirrors the
    // compiler's in-memory structures (TombEngineMesh / TombEngineMoveable / ...) closely so the parsing
    // stays a 1:1 reverse of the writer; conversion into Wad2 objects (WadMesh/WadMoveable/WadStatic) and
    // texture extraction from the media atlas happen in a separate, later step.

    public sealed class TenMeshVertex
    {
        public Vector3 Position;   // engine space (Y already negated by the compiler)
        public Vector3 Color;
        public float Glow;
        public float Move;
        public bool Locked;
        public int[] BoneIndex = new int[4];
        public float[] BoneWeight = new float[4];
    }

    public sealed class TenPolygon
    {
        public int Shape;          // 0 = Quad (4 verts), 1 = Triangle (3 verts)
        public int AnimatedSequence;
        public int AnimatedFrame;
        public float ShineStrength;
        public Vector3 Normal;
        public int[] Indices;
        public Vector2[] TexCoords;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public Vector3[] Binormals;

        // Carried over from the owning bucket's material.
        public int TextureAtlas;   // atlas page index into the media block
        public byte BlendMode;
        public int MaterialIndex;
        public bool Animated;

        public bool IsTriangle => Shape == 1;
        public int VertexCount => Shape == 0 ? 4 : 3;
    }

    public sealed class TenMesh
    {
        public bool Hidden;
        public byte LightingType;
        public Vector3 SphereCenter;
        public float SphereRadius;
        public List<TenMeshVertex> Vertices = new();
        public List<TenPolygon> Polygons = new();
    }

    public sealed class TenKeyFrame
    {
        public Vector3 BoundingBoxCenter;
        public Vector3 BoundingBoxExtents;
        public Vector3 RootOffset;
        public List<Quaternion> BoneOrientations = new();
    }

    public sealed class TenStateChange
    {
        public int StateID;
        public int FrameLow;
        public int FrameHigh;
        public int NextAnimation;
        public int NextLowFrame;
        public int NextHighFrame;
        public int BlendFrames;
    }

    public sealed class TenAnimCommand
    {
        public int Type;          // 1 SetPosition, 2 SetJumpDistance, 3 EmptyHands, 4 KillEntity, 5 PlaySound, 6 FlipEffect, 7 DisableInterpolation
        public Vector3 Vector;    // for types 1/2
        public int[] Ints;        // for types 5 (3), 6 (2), 7 (1)
    }

    public sealed class TenAnimation
    {
        public int StateID;
        public int FrameEnd;
        public int NextAnimation;
        public int NextFrame;
        public int BlendFrameCount;
        public Vector3 VelocityStart;
        public Vector3 VelocityEnd;

        // Pre-baked, per-frame interpolated frames (the .ten format does not retain the original keyframes).
        public List<TenKeyFrame> InterpolatedFrames = new();
        public List<TenStateChange> StateChanges = new();
        public List<TenAnimCommand> Commands = new();
        public int RootMotionFlags;
    }

    public sealed class TenMoveable
    {
        public int ObjectID;
        public int Skin;
        public int NumMeshes;
        public int StartingMesh;   // index of the first mesh in TenObjectData.Meshes
        public int MeshTree;       // offset (in ints) into TenObjectData.MeshTrees
        public List<TenAnimation> Animations = new();
    }

    public sealed class TenStatic
    {
        public int ObjectID;
        public int Mesh;           // index into TenObjectData.Meshes
        public Vector3 VisibilityBoxMin;
        public Vector3 VisibilityBoxMax;
        public Vector3 CollisionBoxMin;
        public Vector3 CollisionBoxMax;
        public ushort Flags;
        public short ShatterType;
        public short ShatterSound;
    }

    /// <summary>
    /// Intermediate, version-agnostic representation of the objects extracted from a compiled
    /// <c>.ten</c> geometry block. Filled incrementally across the reader's slices.
    /// </summary>
    public sealed class TenObjectData
    {
        public List<TenMesh> Meshes = new();

        /// <summary>
        /// Flat mesh-tree blob (4 ints per non-root bone: link flags + X/Y/Z offset), referenced by
        /// each moveable through its MeshTree offset. Kept raw; decoded when moveables are rebuilt.
        /// </summary>
        public int[] MeshTrees = System.Array.Empty<int>();

        public List<TenMoveable> Moveables = new();
        public List<TenStatic> Statics = new();
    }
}
