using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace TombLib.LevelData.IO
{
    /// <summary>
    /// Parses the (already LZ4-decompressed) geometry block of a compiled TombEngine level, mirroring
    /// in reverse the writer in <c>LevelCompilerTombEngine.WriteLevelTombEngine</c> (the geometry stream
    /// section). We only care about objects (moveables/statics), but they are written *after* the rooms
    /// and the floordata, and the format has no offset table, so those leading sections must be parsed
    /// (skipped) byte-for-byte to reach the object section.
    ///
    /// Object section order in the stream: meshes, mesh-trees, moveables, static meshes, (then sprites,
    /// pathfinding, etc. which we ignore).
    ///
    /// SLICE 2: skip rooms + floordata to reach the object section.
    /// SLICE 3: parse meshes and mesh-trees into the intermediate model.
    /// </summary>
    public static class TenGeometryReader
    {
        public static TenObjectData ReadObjects(TenLevelFile file)
        {
            using var ms = new MemoryStream(file.GeometryData, writable: false);
            using var r = new BinaryReader(ms);

            SkipRooms(r);
            SkipFloorData(r);

            var data = new TenObjectData();
            ReadMeshes(r, data);
            ReadMeshTrees(r, data);

            return data;
        }

        // ---- Meshes -----------------------------------------------------------------------------
        // writer.Write(_meshes.Count); foreach mesh: header, vertices (split passes), buckets+polygons.

        private static void ReadMeshes(BinaryReader r, TenObjectData data)
        {
            int meshCount = r.ReadInt32();
            for (int i = 0; i < meshCount; i++)
                data.Meshes.Add(ReadMesh(r));
        }

        private static TenMesh ReadMesh(BinaryReader r)
        {
            var mesh = new TenMesh
            {
                Hidden = r.ReadBoolean(),
                LightingType = r.ReadByte(),
                SphereCenter = ReadVector3(r),
                SphereRadius = r.ReadSingle()
            };

            int vertexCount = r.ReadInt32();
            for (int i = 0; i < vertexCount; i++)
                mesh.Vertices.Add(new TenMeshVertex());

            // Vertex data is written as separate per-attribute passes over all vertices.
            for (int i = 0; i < vertexCount; i++)
                mesh.Vertices[i].Position = ReadVector3(r);
            for (int i = 0; i < vertexCount; i++)
                mesh.Vertices[i].Color = ReadVector3(r);
            for (int i = 0; i < vertexCount; i++)
            {
                // new Vector3(Glow, Move, Locked ? 0 : 1)
                var gml = ReadVector3(r);
                mesh.Vertices[i].Glow = gml.X;
                mesh.Vertices[i].Move = gml.Y;
                mesh.Vertices[i].Locked = gml.Z == 0f;
            }
            for (int i = 0; i < vertexCount; i++)
                for (int w = 0; w < 4; w++)
                    mesh.Vertices[i].BoneIndex[w] = r.ReadByte();
            for (int i = 0; i < vertexCount; i++)
                for (int w = 0; w < 4; w++)
                    mesh.Vertices[i].BoneWeight[w] = r.ReadByte() / 255.0f;

            int bucketCount = r.ReadInt32();
            for (int b = 0; b < bucketCount; b++)
            {
                int texture = r.ReadInt32();
                byte blendMode = r.ReadByte();
                int materialIndex = r.ReadInt32();
                bool animated = r.ReadBoolean();

                int polyCount = r.ReadInt32();
                for (int p = 0; p < polyCount; p++)
                {
                    var poly = ReadPolygon(r, hasShineStrength: true);
                    poly.TextureAtlas = texture;
                    poly.BlendMode = blendMode;
                    poly.MaterialIndex = materialIndex;
                    poly.Animated = animated;
                    mesh.Polygons.Add(poly);
                }
            }

            return mesh;
        }

        // Polygon layout shared between room buckets and mesh buckets. Mesh polygons additionally write a
        // ShineStrength float (rooms do not).
        // Layout: Shape(int) AnimatedSequence(int) AnimatedFrame(int) [ShineStrength(float)] Normal(Vec3)
        //         then n indices(int), n uv(Vec2), n normals(Vec3), n tangents(Vec3), n binormals(Vec3)
        // where n = 4 for Quad (Shape==0) or 3 for Triangle (Shape==1).
        // NOTE on field order: the writer emits ShineStrength *after* AnimatedFrame and *before* Normal for
        // meshes; rooms omit it entirely.
        private static TenPolygon ReadPolygon(BinaryReader r, bool hasShineStrength)
        {
            var poly = new TenPolygon
            {
                Shape = r.ReadInt32(),
                AnimatedSequence = r.ReadInt32(),
                AnimatedFrame = r.ReadInt32()
            };

            if (hasShineStrength)
                poly.ShineStrength = r.ReadSingle();

            poly.Normal = ReadVector3(r);

            int n = poly.VertexCount;
            poly.Indices = new int[n];
            for (int i = 0; i < n; i++)
                poly.Indices[i] = r.ReadInt32();
            poly.TexCoords = new Vector2[n];
            for (int i = 0; i < n; i++)
                poly.TexCoords[i] = ReadVector2(r);
            poly.Normals = new Vector3[n];
            for (int i = 0; i < n; i++)
                poly.Normals[i] = ReadVector3(r);
            poly.Tangents = new Vector3[n];
            for (int i = 0; i < n; i++)
                poly.Tangents[i] = ReadVector3(r);
            poly.Binormals = new Vector3[n];
            for (int i = 0; i < n; i++)
                poly.Binormals[i] = ReadVector3(r);

            return poly;
        }

        // ---- Mesh trees -------------------------------------------------------------------------
        // writer.Write(_meshTrees.Count); writer.WriteBlockArray(_meshTrees)  // List<int>

        private static void ReadMeshTrees(BinaryReader r, TenObjectData data)
        {
            int count = r.ReadInt32();
            var trees = new int[count];
            for (int i = 0; i < count; i++)
                trees[i] = r.ReadInt32();
            data.MeshTrees = trees;
        }

        // ---- Rooms (skipped) --------------------------------------------------------------------
        // writer.Write(_level.ExistingRooms.Count); foreach room: TombEngineRoom.WriteStaticData(writer)

        private static void SkipRooms(BinaryReader r)
        {
            int roomCount = r.ReadInt32();
            for (int i = 0; i < roomCount; i++)
                SkipRoom(r);
        }

        private static void SkipRoom(BinaryReader r)
        {
            // writer.WriteBlock(Info) -> tr_room_info { int X, Z, YBottom, YTop } = 16 bytes
            Skip(r, 16);

            // Vertices: three separate per-vertex Vector3 passes (Position, Color, (Glow,Move,Locked)).
            int vertexCount = r.ReadInt32();
            Skip(r, vertexCount * (12L + 12 + 12));

            // Buckets
            int bucketCount = r.ReadInt32();
            for (int b = 0; b < bucketCount; b++)
            {
                // Material: Texture(int) + BlendMode(byte) + MaterialIndex(int) + Animated(bool)
                Skip(r, 4 + 1 + 4 + 1);

                int polyCount = r.ReadInt32();
                for (int p = 0; p < polyCount; p++)
                    SkipPolygon(r, hasShineStrength: false);
            }

            // Portals: writer.WriteBlock(Portals.Count) (int); then per portal.
            int portalCount = r.ReadInt32();
            // AdjoiningRoom(ushort=2) + Normal(3 int=12) + 4 vertices*(3 int=12)=48 => 62 bytes
            Skip(r, portalCount * 62L);

            // Sectors
            int numZSectors = r.ReadInt32();
            int numXSectors = r.ReadInt32();
            // Per sector = 100 bytes: Trigger/Box/Step/Stopper(16) + Floor(36) + Ceiling(36) + WallPortal(4) + 8 bools(8)
            Skip(r, (long)numZSectors * numXSectors * 100);

            // Lights: writer.WriteBlock(Lights.Count) (int); then per light.
            int lightCount = r.ReadInt32();
            // Position(3 int=12) + Direction(12) + Color(12) + 5 floats(20) + LightType(byte) + CastShadows(bool)
            Skip(r, lightCount * 58L);
        }

        private static void SkipPolygon(BinaryReader r, bool hasShineStrength)
        {
            int shape = r.ReadInt32();
            int n = shape == 0 ? 4 : 3;

            int fixedBytes = 4 + 4 + 12; // AnimatedSequence + AnimatedFrame + Normal
            if (hasShineStrength)
                fixedBytes += 4;

            int perVertexBytes = n * (4 + 8 + 12 + 12 + 12);
            Skip(r, fixedBytes + perVertexBytes);
        }

        // ---- Floordata --------------------------------------------------------------------------
        // writer.Write((uint)_floorData.Count); writer.WriteBlockArray(_floorData)  // List<ushort>

        private static void SkipFloorData(BinaryReader r)
        {
            uint count = r.ReadUInt32();
            Skip(r, (long)count * 2);
        }

        private static Vector2 ReadVector2(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle());
        private static Vector3 ReadVector3(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        private static void Skip(BinaryReader r, long bytes) => r.BaseStream.Seek(bytes, SeekOrigin.Current);
    }
}
