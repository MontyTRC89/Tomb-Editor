using System.IO;

namespace TombLib.LevelData.IO
{
    /// <summary>
    /// Intermediate, version-agnostic representation of the objects extracted from a compiled
    /// <c>.ten</c> geometry block. Filled incrementally: rooms are skipped (not imported), then
    /// meshes / mesh-trees / moveables / statics are reconstructed in later slices.
    /// </summary>
    public sealed class TenObjectData
    {
        public int MeshCount { get; internal set; }
    }

    /// <summary>
    /// Parses the (already LZ4-decompressed) geometry block of a compiled TombEngine level, mirroring
    /// in reverse the writer in <c>LevelCompilerTombEngine.WriteLevelTombEngine</c> (the geometry stream
    /// section). We only care about objects (moveables/statics), but they are written *after* the rooms
    /// and the floordata, and the format has no offset table, so those leading sections must be parsed
    /// (skipped) byte-for-byte to reach the object section.
    ///
    /// SLICE 2: parse the room and floordata sections to position the cursor exactly at the start of the
    /// object section, then read the mesh count. Mesh/mesh-tree/moveable/static reconstruction follows in
    /// later slices.
    /// </summary>
    public static class TenGeometryReader
    {
        public static TenObjectData ReadObjects(TenLevelFile file)
        {
            using var ms = new MemoryStream(file.GeometryData, writable: false);
            using var r = new BinaryReader(ms);

            SkipRooms(r);
            SkipFloorData(r);

            // Object section starts here: "// Write meshes" -> writer.Write(_meshes.Count)
            int meshCount = r.ReadInt32();

            return new TenObjectData { MeshCount = meshCount };
        }

        // ---- Rooms ------------------------------------------------------------------------------
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

            // Vertices: written as three separate per-vertex Vector3 passes (Position, Color, (Glow,Move,Locked)).
            int vertexCount = r.ReadInt32();
            Skip(r, vertexCount * (12 + 12 + 12));

            // Buckets
            int bucketCount = r.ReadInt32();
            for (int b = 0; b < bucketCount; b++)
            {
                // Material: Texture(int) + BlendMode(byte) + MaterialIndex(int) + Animated(bool)
                Skip(r, 4 + 1 + 4 + 1);

                int polyCount = r.ReadInt32();
                for (int p = 0; p < polyCount; p++)
                    SkipRoomOrMeshPolygon(r, hasShineStrength: false);
            }

            // Portals: writer.WriteBlock(Portals.Count) (int); then per portal, if any.
            int portalCount = r.ReadInt32();
            // AdjoiningRoom(ushort=2) + Normal(3 int=12) + 4 vertices*(3 int=12)=48 => 62 bytes
            Skip(r, portalCount * 62);

            // Sectors
            int numZSectors = r.ReadInt32();
            int numXSectors = r.ReadInt32();
            // Per sector = 100 bytes (see SkipRoom comment block / WriteStaticData):
            //   Trigger/Box/Step/Stopper (4*4=16) + Floor(36) + Ceiling(36) + WallPortal(4) + 8 bools(8)
            Skip(r, numZSectors * numXSectors * 100);

            // Lights: writer.WriteBlock(Lights.Count) (int); then per light.
            int lightCount = r.ReadInt32();
            // Position(3 int=12) + Direction(12) + Color(12) + 5 floats(20) + LightType(byte) + CastShadows(bool)
            Skip(r, lightCount * 58);
        }

        // Polygon layout shared between room buckets and mesh buckets. The only difference is that mesh
        // polygons additionally write a ShineStrength float (rooms do not).
        // Layout: Shape(int) [+ ShineStrength(float)] AnimatedSequence(int) AnimatedFrame(int) Normal(Vec3)
        //         then n indices(int), n uv(Vec2), n normals(Vec3), n tangents(Vec3), n binormals(Vec3)
        // where n = 4 for Quad (Shape==0) or 3 for Triangle (Shape==1).
        private static void SkipRoomOrMeshPolygon(BinaryReader r, bool hasShineStrength)
        {
            int shape = r.ReadInt32();
            int n = shape == 0 ? 4 : 3; // TombEnginePolygonShape: Quad=0, Triangle=1

            int fixedBytes = 4 + 4 + 12; // AnimatedSequence + AnimatedFrame + Normal
            if (hasShineStrength)
                fixedBytes += 4;

            // indices(n*4) + uv(n*8) + normals(n*12) + tangents(n*12) + binormals(n*12)
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

        private static void Skip(BinaryReader r, long bytes) => r.BaseStream.Seek(bytes, SeekOrigin.Current);
    }
}
