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
            ReadMoveables(r, data);
            ReadStatics(r, data);

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

        // ---- Moveables --------------------------------------------------------------------------
        // writer.Write(_moveables.Count); foreach moveable: TombEngineMoveable.Write(writer)

        private static void ReadMoveables(BinaryReader r, TenObjectData data)
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                data.Moveables.Add(ReadMoveable(r));
        }

        private static TenMoveable ReadMoveable(BinaryReader r)
        {
            var mov = new TenMoveable
            {
                ObjectID = r.ReadInt32(),
                Skin = r.ReadInt32(),
                NumMeshes = r.ReadInt32(),
                StartingMesh = r.ReadInt32(),
                MeshTree = r.ReadInt32()
            };

            int numAnimations = r.ReadInt32();
            for (int a = 0; a < numAnimations; a++)
                mov.Animations.Add(ReadAnimation(r, mov.NumMeshes));

            return mov;
        }

        private static TenAnimation ReadAnimation(BinaryReader r, int numMeshes)
        {
            var anim = new TenAnimation
            {
                StateID = r.ReadInt32(),
                FrameEnd = r.ReadInt32(),
                NextAnimation = r.ReadInt32(),
                NextFrame = r.ReadInt32(),
                BlendFrameCount = r.ReadInt32()
            };

            // Blend curve (4 Vec2) - not needed for object import, skipped.
            SkipBezierCurve(r);

            // Velocity is encoded as three fixed motion bezier curves (X/Y/Z). For each, Start = (0, velStart.c)
            // and End = (1, velEnd.c), so the velocity components are recoverable from the curves' Y values.
            ReadVelocityFromCurve(r, out float startX, out float endX);
            ReadVelocityFromCurve(r, out float startY, out float endY);
            ReadVelocityFromCurve(r, out float startZ, out float endZ);
            anim.VelocityStart = new Vector3(startX, startY, startZ);
            anim.VelocityEnd = new Vector3(endX, endY, endZ);

            // Pre-baked interpolated frames
            int frameCount = r.ReadInt32();
            for (int f = 0; f < frameCount; f++)
                anim.InterpolatedFrames.Add(ReadKeyFrame(r));

            // State changes
            int stateChangeCount = r.ReadInt32();
            for (int s = 0; s < stateChangeCount; s++)
                anim.StateChanges.Add(ReadStateChange(r));

            // Anim commands: count of commands, then each command is self-describing (type + fixed params).
            int numAnimCommands = r.ReadInt32();
            for (int c = 0; c < numAnimCommands; c++)
                anim.Commands.Add(ReadAnimCommand(r));

            anim.RootMotionFlags = r.ReadInt32();
            return anim;
        }

        private static TenKeyFrame ReadKeyFrame(BinaryReader r)
        {
            var frame = new TenKeyFrame
            {
                BoundingBoxCenter = ReadVector3(r),
                BoundingBoxExtents = ReadVector3(r),
                RootOffset = ReadVector3(r)
            };

            int boneCount = r.ReadInt32();
            for (int i = 0; i < boneCount; i++)
                frame.BoneOrientations.Add(ReadQuaternion(r));

            return frame;
        }

        private static TenStateChange ReadStateChange(BinaryReader r)
        {
            var sc = new TenStateChange
            {
                StateID = r.ReadInt32(),
                FrameLow = r.ReadInt32(),
                FrameHigh = r.ReadInt32(),
                NextAnimation = r.ReadInt32(),
                NextLowFrame = r.ReadInt32(),
                NextHighFrame = r.ReadInt32(),
                BlendFrames = r.ReadInt32()
            };
            SkipBezierCurve(r); // BlendCurve (4 Vec2)
            return sc;
        }

        private static TenAnimCommand ReadAnimCommand(BinaryReader r)
        {
            var cmd = new TenAnimCommand { Type = r.ReadInt32() };
            switch (cmd.Type)
            {
                case 1: // SetPosition
                case 2: // SetJumpDistance
                    cmd.Vector = ReadVector3(r);
                    break;
                case 3: // EmptyHands
                case 4: // KillEntity
                    break;
                case 5: // PlaySound: SoundID, Frame, Environment
                    cmd.Ints = new[] { r.ReadInt32(), r.ReadInt32(), r.ReadInt32() };
                    break;
                case 6: // FlipEffect: two ints
                    cmd.Ints = new[] { r.ReadInt32(), r.ReadInt32() };
                    break;
                case 7: // DisableInterpolation: one int
                    cmd.Ints = new[] { r.ReadInt32() };
                    break;
            }
            return cmd;
        }

        // ---- Static meshes ----------------------------------------------------------------------
        // writer.Write(_staticMeshes.Count); writer.WriteBlockArray(_staticMeshes)  // TombEngineStaticMesh

        private static void ReadStatics(BinaryReader r, TenObjectData data)
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var s = new TenStatic
                {
                    ObjectID = r.ReadInt32(),
                    Mesh = r.ReadInt32()
                };

                // TombEngineBoundingBox: short X1, X2, Y1, Y2, Z1, Z2 (min = *1, max = *2)
                short vx1 = r.ReadInt16(), vx2 = r.ReadInt16(), vy1 = r.ReadInt16(), vy2 = r.ReadInt16(), vz1 = r.ReadInt16(), vz2 = r.ReadInt16();
                short cx1 = r.ReadInt16(), cx2 = r.ReadInt16(), cy1 = r.ReadInt16(), cy2 = r.ReadInt16(), cz1 = r.ReadInt16(), cz2 = r.ReadInt16();
                s.VisibilityBoxMin = new Vector3(vx1, vy1, vz1);
                s.VisibilityBoxMax = new Vector3(vx2, vy2, vz2);
                s.CollisionBoxMin = new Vector3(cx1, cy1, cz1);
                s.CollisionBoxMax = new Vector3(cx2, cy2, cz2);

                s.Flags = r.ReadUInt16();
                s.ShatterType = r.ReadInt16();
                s.ShatterSound = r.ReadInt16();

                data.Statics.Add(s);
            }
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

        // A BezierCurve2 is serialized as Start, End, StartHandle, EndHandle (4 Vec2 = 32 bytes).
        private static void SkipBezierCurve(BinaryReader r) => Skip(r, 32);

        // Velocity component encoded as a fixed motion curve: Start = (0, velStart), End = (1, velEnd),
        // handles duplicate the endpoints. We only need the Y of Start and End.
        private static void ReadVelocityFromCurve(BinaryReader r, out float velStart, out float velEnd)
        {
            var start = ReadVector2(r);
            var end = ReadVector2(r);
            Skip(r, 16); // StartHandle + EndHandle
            velStart = start.Y;
            velEnd = end.Y;
        }

        private static Vector2 ReadVector2(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle());
        private static Vector3 ReadVector3(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        private static Quaternion ReadQuaternion(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        private static void Skip(BinaryReader r, long bytes) => r.BaseStream.Seek(bytes, SeekOrigin.Current);
    }
}
