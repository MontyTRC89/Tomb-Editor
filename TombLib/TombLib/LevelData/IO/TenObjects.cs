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
    }
}
