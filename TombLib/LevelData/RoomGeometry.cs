using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public class RoomGeometry
    {
        // EditorUV Map:
        //                      | +Y
        //   ################## | #################
        //   ###   Triangle   # | #               #
        //   #  ###    Split  # | #               #
        //   #     ###        # | #      Quad     #
        //   #        ###     # | #      Face     #
        //   #           ###  # | #               #
        //   #              ### | #               #
        //   ################## | #################    +x
        // ---------------------0---------------------------
        //   ################## | ##################
        //   ###   Triangle   # | ###   Triangle   #
        //   #  ###    Split  # | #  ###    Split  #
        //   #     ###        # | #     ###        #
        //   #        ###     # | #        ###     #
        //   #           ###  # | #           ###  #
        //   #              ### | #              ###
        //   ################## | ##################
        //                      |

        // Quads are reformed by 2 triangles.
        public int DoubleSidedTriangleCount { get; private set; } = 0;
        public List<Vector3> VertexPositions { get; } = new List<Vector3>(); // one for each vertex
        public List<Vector2> VertexEditorUVs { get; } = new List<Vector2>(); // one for each vertex (ushort to save (GPU) memory and bandwidth)
        public List<Vector3> VertexColors { get; } = new List<Vector3>(); // one for each vertex

        public List<TextureArea> TriangleTextureAreas { get; } = new List<TextureArea>(); // one for each triangle
        public List<SectorInfo> TriangleSectorInfo { get; } = new List<SectorInfo>(); // one for each triangle

        public Dictionary<Vector3, List<int>> SharedVertices { get; } = new Dictionary<Vector3, List<int>>();
        public SortedList<SectorInfo, VertexRange> VertexRangeLookup { get; } = new SortedList<SectorInfo, VertexRange>();

        public bool HasVertices { get { return (VertexPositions.Count != 0 && (VertexPositions.Count + DoubleSidedTriangleCount * 3 >= 0)); } }

        public RoomGeometry(Room room)
        {
            int xMin = 0;
            int zMin = 0;
            int xMax = room.NumXSectors - 1;
            int zMax = room.NumZSectors - 1;
            Block[,] Blocks = room.Blocks;

            // Build face polygons
            for (int x = xMin; x <= xMax; x++) // This is in order to VertexRangeKey sorting.
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    // If x, z is one of the four corner then nothing has to be done
                    if (x == 0 && z == 0 || x == 0 && z == room.NumZSectors - 1 ||
                        x == room.NumXSectors - 1 && z == room.NumZSectors - 1 || x == room.NumXSectors - 1 && z == 0)
                        continue;

                    // Vertical polygons

                    // +Z direction
                    if (x > 0 && x < room.NumXSectors - 1 && z > 0 && z < room.NumZSectors - 2 &&
                        !(Blocks[x, z + 1].Type == BlockType.Wall &&
                         (Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z + 1].FloorDiagonalSplit == DiagonalSplit.XnZn)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZn))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, false);
                    }


                    // -Z direction
                    if (x > 0 && x < room.NumXSectors - 1 && z > 1 && z < room.NumZSectors - 1 &&
                        !(Blocks[x, z - 1].Type == BlockType.Wall &&
                         (Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z - 1].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall ||
                            (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, false);
                    }

                    // +X direction
                    if (z > 0 && z < room.NumZSectors - 1 && x > 0 && x < room.NumXSectors - 2 &&
                        !(Blocks[x + 1, z].Type == BlockType.Wall &&
                        (Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.XnZn || Blocks[x + 1, z].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZn || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, false);
                    }

                    // -X direction
                    if (z > 0 && z < room.NumZSectors - 1 && x > 1 && x < room.NumXSectors - 1 &&
                        !(Blocks[x - 1, z].Type == BlockType.Wall &&
                        (Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x - 1, z].FloorDiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.XpZp))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, false);
                    }

                    // Diagonal faces
                    if (Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type == BlockType.Wall)
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalFloor, true, true, true);
                        }
                        else
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalFloor, true, false, false);
                        }
                    }

                    if (Blocks[x, z].CeilingDiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type != BlockType.Wall)
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalCeiling, false, true, false);
                        }
                    }

                    // +Z directed border wall
                    if (z == 0 && x != 0 && x != room.NumXSectors - 1 &&
                        !(Blocks[x, 1].Type == BlockType.Wall &&
                         (Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[x, 1].FloorDiagonalSplit == DiagonalSplit.XnZn)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Flipped && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingX = x + (room.Position.X - adjoiningRoom.Position.X);
                            var block = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZp ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZp))
                            {
                                addMiddle = true;
                            }
                        }


                        if (addMiddle || Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, false);
                    }

                    // -Z directed border wall
                    if (z == room.NumZSectors - 1 && x != 0 && x != room.NumXSectors - 1 &&
                        !(Blocks[x, room.NumZSectors - 2].Type == BlockType.Wall &&
                         (Blocks[x, room.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.None || Blocks[x, room.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.XpZp || Blocks[x, room.NumZSectors - 2].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Flipped && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingX = x + (room.Position.X - adjoiningRoom.Position.X);
                            var block = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZn))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, false);
                    }

                    // -X directed border wall
                    if (x == 0 && z != 0 && z != room.NumZSectors - 1 &&
                        !(Blocks[1, z].Type == BlockType.Wall &&
                         (Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.XnZn || Blocks[1, z].FloorDiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Flipped && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);
                            var block = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZn ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XpZp))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, false);
                    }

                    // +X directed border wall
                    if (x == room.NumXSectors - 1 && z != 0 && z != room.NumZSectors - 1 &&
                        !(Blocks[room.NumXSectors - 2, z].Type == BlockType.Wall &&
                         (Blocks[room.NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.None || Blocks[room.NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.XpZn || Blocks[room.NumXSectors - 2, z].FloorDiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Flipped && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);
                            var block = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.FloorDiagonalSplit == DiagonalSplit.None ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                 block.FloorDiagonalSplit == DiagonalSplit.XnZp))
                            {
                                addMiddle = true;
                            }
                        }

                        if (addMiddle || Blocks[x, z].Type == BlockType.BorderWall && Blocks[x, z].WallPortal == null || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, false);
                    }

                    // Floor polygons
                    Room.RoomConnectionInfo floorPortalInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z));
                    BuildFloorOrCeilingFace(room, x, z, Blocks[x, z].QA[0], Blocks[x, z].QA[1], Blocks[x, z].QA[2], Blocks[x, z].QA[3], Blocks[x, z].FloorDiagonalSplit, Blocks[x, z].FloorSplitDirectionIsXEqualsZ,
                        BlockFace.Floor, BlockFace.FloorTriangle2, floorPortalInfo.VisualType, false);

                    // Ceiling polygons
                    int ceilingStartVertex = VertexPositions.Count;

                    Room.RoomConnectionInfo ceilingPortalInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z));
                    BuildFloorOrCeilingFace(room, x, z, Blocks[x, z].WS[0], Blocks[x, z].WS[1], Blocks[x, z].WS[2], Blocks[x, z].WS[3], Blocks[x, z].CeilingDiagonalSplit, Blocks[x, z].CeilingSplitDirectionIsXEqualsZ,
                        BlockFace.Ceiling, BlockFace.CeilingTriangle2, ceilingPortalInfo.VisualType, true);

                    // Change vertices order for ceiling polygons
                    for (int i = ceilingStartVertex; i < VertexPositions.Count; i += 3)
                    {
                        var tempPosition = VertexPositions[i + 2];
                        VertexPositions[i + 2] = VertexPositions[i];
                        VertexPositions[i] = tempPosition;
                        var tempEditorUV = VertexEditorUVs[i + 2];
                        VertexEditorUVs[i + 2] = VertexEditorUVs[i];
                        VertexEditorUVs[i] = tempEditorUV;
                    }
                }
            }

            // Group shared vertices
            for (int i = 0; i < VertexPositions.Count; ++i)
            {
                Vector3 position = VertexPositions[i];
                List<int> list;
                if (!SharedVertices.TryGetValue(position, out list))
                    SharedVertices.Add(position, list = new List<int>());
                list.Add(i);
            }

            // Build color array
            VertexColors.Resize(VertexPositions.Count, room.AmbientLight);

            // Lighting
            Relight(room);
        }

        private enum FaceDirection
        {
            PositiveZ, NegativeZ, PositiveX, NegativeX, DiagonalFloor, DiagonalCeiling, DiagonalWall
        }

        private void BuildFloorOrCeilingFace(Room room, int x, int z, int h0, int h1, int h2, int h3, DiagonalSplit splitType, bool diagonalSplitXEqualsY,
                                             BlockFace face1, BlockFace face2, Room.RoomConnectionType portalMode,
                                             bool isForCeiling)
        {
            BlockType blockType = room.Blocks[x, z].Type;

            // Exit function if the sector is a complete wall or portal
            if (portalMode == Room.RoomConnectionType.FullPortal)
                return;

            switch (blockType)
            {
                case BlockType.BorderWall:
                    return;
                case BlockType.Wall:
                    if (splitType == DiagonalSplit.None)
                        return;
                    break;
            }

            // Process relevant sectors for portals
            switch (portalMode)
            {
                case Room.RoomConnectionType.FullPortal:
                    return;
                case Room.RoomConnectionType.TriangularPortalXnZp:
                case Room.RoomConnectionType.TriangularPortalXpZn:
                    if (Block.IsQuad(h0, h1, h2, h3))
                        diagonalSplitXEqualsY = true;
                    break;
                case Room.RoomConnectionType.TriangularPortalXpZp:
                case Room.RoomConnectionType.TriangularPortalXnZn:
                    if (Block.IsQuad(h0, h1, h2, h3))
                        diagonalSplitXEqualsY = false;
                    break;
            }

            //
            // 1----2    Split 0: 231 413
            // | \  |    Split 1: 124 342
            // |  \ |
            // 4----3
            //

            //
            // 1----2    Split 0: 231 413
            // |  / |    Split 1: 124 342
            // | /  |
            // 4----3
            //

            // Build sector
            if (splitType != DiagonalSplit.None)
            {
                switch (splitType)
                {
                    case DiagonalSplit.XpZn:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h0 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn && blockType != BlockType.Wall)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                        break;
                    case DiagonalSplit.XnZn:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, z * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn && blockType != BlockType.Wall)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                        break;
                    case DiagonalSplit.XnZp:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp && blockType != BlockType.Wall)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);

                        break;
                    case DiagonalSplit.XpZp:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                                new Vector3(x * 1024.0f, h3 * 256.0f, (z + 1) * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp && blockType != BlockType.Wall)
                            AddTriangle(x, z, face1,
                                new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                                new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                        break;
                    default:
                        throw new NotSupportedException("Unknown FloorDiagonalSplit");
                }
            }
            else if (Block.IsQuad(h0, h1, h2, h3) && portalMode == Room.RoomConnectionType.NoPortal)
            {
                AddQuad(x, z, face1,
                    new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                    new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                    room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                    isForCeiling);
            }
            else if (diagonalSplitXEqualsY || portalMode == Room.RoomConnectionType.TriangularPortalXnZp || portalMode == Room.RoomConnectionType.TriangularPortalXpZn)
            {
                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                    AddTriangle(x, z, face2,
                        new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                        room.Blocks[x, z].GetFaceTexture(face2), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);

                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                    AddTriangle(x, z, face1,
                        new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                        room.Blocks[x, z].GetFaceTexture(face1), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
            }
            else
            {
                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                    AddTriangle(x, z, face1,
                        new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                        new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                        room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);

                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                    AddTriangle(x, z, face2,
                        new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                        new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                        room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
            }
        }

        private void AddVerticalFaces(Room room, int x, int z, FaceDirection direction, bool floor, bool ceiling, bool middle)
        {
            int xA, xB, zA, zB, yA, yB;
            int qA, qB, eA, eB, rA, rB, wA, wB, fA, fB, cA, cB;

            Block b = room.Blocks[x, z];
            Block ob;
            TextureArea face;

            BlockFace qaFace, edFace, wsFace, rfFace, middleFace;

            switch (direction)
            {
                case FaceDirection.PositiveZ:
                    xA = x + 1;
                    xB = x;
                    zA = z + 1;
                    zB = z + 1;
                    ob = room.Blocks[x, z + 1];
                    qA = b.QA[1];
                    qB = b.QA[0];
                    eA = b.ED[1];
                    eB = b.ED[0];
                    rA = b.RF[1];
                    rB = b.RF[0];
                    wA = b.WS[1];
                    wB = b.WS[0];
                    fA = ob.QA[2];
                    fB = ob.QA[3];
                    cA = ob.WS[2];
                    cB = ob.WS[3];
                    qaFace = BlockFace.PositiveZ_QA;
                    edFace = BlockFace.PositiveZ_ED;
                    middleFace = BlockFace.PositiveZ_Middle;
                    rfFace = BlockFace.PositiveZ_RF;
                    wsFace = BlockFace.PositiveZ_WS;

                    if (b.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = b.WallPortal;
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (room.Flipped && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[x, 1];

                        var qaNearA = nearBlock.QA[2];
                        var qaNearB = nearBlock.QA[3];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[2];
                        var wsNearB = nearBlock.WS[3];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                            wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[1];
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[0];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZn)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[1];
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[0];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                            wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[1];
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[0];
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[1];
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[0];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = b.QA[0];
                        qB = b.QA[0];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = b.QA[1];
                        qB = b.QA[1];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = ob.QA[2];
                        fB = ob.QA[2];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = ob.QA[3];
                        fB = ob.QA[3];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = b.WS[0];
                        wB = b.WS[0];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = b.WS[1];
                        wB = b.WS[1];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = ob.WS[2];
                        cB = ob.WS[2];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = ob.WS[3];
                        cB = ob.WS[3];
                    }

                    break;

                case FaceDirection.NegativeZ:
                    xA = x;
                    xB = x + 1;
                    zA = z;
                    zB = z;
                    ob = room.Blocks[x, z - 1];
                    qA = b.QA[3];
                    qB = b.QA[2];
                    eA = b.ED[3];
                    eB = b.ED[2];
                    rA = b.RF[3];
                    rB = b.RF[2];
                    wA = b.WS[3];
                    wB = b.WS[2];
                    fA = ob.QA[0];
                    fB = ob.QA[1];
                    cA = ob.WS[0];
                    cB = ob.WS[1];
                    qaFace = BlockFace.NegativeZ_QA;
                    edFace = BlockFace.NegativeZ_ED;
                    middleFace = BlockFace.NegativeZ_Middle;
                    rfFace = BlockFace.NegativeZ_RF;
                    wsFace = BlockFace.NegativeZ_WS;

                    if (b.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = b.WallPortal;
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (room.Flipped && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[x, room.NumZSectors - 2];

                        var qaNearA = nearBlock.QA[0];
                        var qaNearB = nearBlock.QA[1];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZn)
                            qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[0];
                        var wsNearB = nearBlock.WS[1];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                            wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[3];
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[2];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[3];
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[2];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                            wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[3];
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[2];
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[3];
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[2];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = b.QA[3];
                        qB = b.QA[3];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = b.QA[2];
                        qB = b.QA[2];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = ob.QA[0];
                        fB = ob.QA[0];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = ob.QA[1];
                        fB = ob.QA[1];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = b.WS[3];
                        wB = b.WS[3];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = b.WS[2];
                        wB = b.WS[2];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = ob.WS[0];
                        cB = ob.WS[0];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = ob.WS[1];
                        cB = ob.WS[1];
                    }

                    break;

                case FaceDirection.PositiveX:
                    xA = x + 1;
                    xB = x + 1;
                    zA = z;
                    zB = z + 1;
                    ob = room.Blocks[x + 1, z];
                    qA = b.QA[2];
                    qB = b.QA[1];
                    eA = b.ED[2];
                    eB = b.ED[1];
                    rA = b.RF[2];
                    rB = b.RF[1];
                    wA = b.WS[2];
                    wB = b.WS[1];
                    fA = ob.QA[3];
                    fB = ob.QA[0];
                    cA = ob.WS[3];
                    cB = ob.WS[0];
                    qaFace = BlockFace.PositiveX_QA;
                    edFace = BlockFace.PositiveX_ED;
                    middleFace = BlockFace.PositiveX_Middle;
                    rfFace = BlockFace.PositiveX_RF;
                    wsFace = BlockFace.PositiveX_WS;

                    if (b.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = b.WallPortal;
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (room.Flipped && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[1, z];

                        var qaNearA = nearBlock.QA[3];
                        var qaNearB = nearBlock.QA[0];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[3];
                        var wsNearB = nearBlock.WS[0];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                            wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[2];
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[1];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZn)
                            qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[2];
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[1];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                            wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[2];
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[1];
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[2];
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[1];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = b.QA[1];
                        qB = b.QA[1];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = b.QA[2];
                        qB = b.QA[2];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = ob.QA[0];
                        fB = ob.QA[0];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = ob.QA[3];
                        fB = ob.QA[3];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = b.WS[1];
                        wB = b.WS[1];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = b.WS[2];
                        wB = b.WS[2];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = ob.WS[0];
                        cB = ob.WS[0];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = ob.WS[3];
                        cB = ob.WS[3];
                    }

                    break;

                case FaceDirection.DiagonalFloor:
                    switch (b.FloorDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            qA = b.QA[1];
                            qB = b.QA[3];
                            eA = b.ED[1];
                            eB = b.ED[3];
                            rA = b.RF[1];
                            rB = b.RF[3];
                            wA = b.WS[1];
                            wB = b.WS[3];
                            fA = b.QA[0];
                            fB = b.QA[0];
                            cA = b.WS[0];
                            cB = b.WS[0];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZn:
                            xA = x + 1;
                            xB = x;
                            zA = z;
                            zB = z + 1;
                            qA = b.QA[2];
                            qB = b.QA[0];
                            eA = b.ED[2];
                            eB = b.ED[0];
                            rA = b.RF[2];
                            rB = b.RF[0];
                            wA = b.WS[2];
                            wB = b.WS[0];
                            fA = b.QA[1];
                            fB = b.QA[1];
                            cA = b.WS[1];
                            cB = b.WS[1];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZp:
                            xA = x;
                            xB = x + 1;
                            zA = z;
                            zB = z + 1;
                            qA = b.QA[3];
                            qB = b.QA[1];
                            eA = b.ED[3];
                            eB = b.ED[1];
                            rA = b.RF[3];
                            rB = b.RF[1];
                            wA = b.WS[3];
                            wB = b.WS[1];
                            fA = b.QA[2];
                            fB = b.QA[2];
                            cA = b.WS[2];
                            cB = b.WS[2];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        default:
                            xA = x;
                            xB = x + 1;
                            zA = z + 1;
                            zB = z;
                            qA = b.QA[0];
                            qB = b.QA[2];
                            eA = b.ED[0];
                            eB = b.ED[2];
                            rA = b.RF[0];
                            rB = b.RF[2];
                            wA = b.WS[0];
                            wB = b.WS[2];
                            fA = b.QA[3];
                            fB = b.QA[3];
                            cA = b.WS[3];
                            cB = b.WS[3];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    switch (b.CeilingDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            qA = b.QA[1];
                            qB = b.QA[3];
                            eA = b.ED[1];
                            eB = b.ED[3];
                            rA = b.RF[1];
                            rB = b.RF[3];
                            wA = b.WS[1];
                            wB = b.WS[3];
                            fA = b.QA[0];
                            fB = b.QA[0];
                            cA = b.WS[0];
                            cB = b.WS[0];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZn:
                            xA = x + 1;
                            xB = x;
                            zA = z;
                            zB = z + 1;
                            qA = b.QA[2];
                            qB = b.QA[0];
                            eA = b.ED[2];
                            eB = b.ED[0];
                            rA = b.RF[2];
                            rB = b.RF[0];
                            wA = b.WS[2];
                            wB = b.WS[0];
                            fA = b.QA[1];
                            fB = b.QA[1];
                            cA = b.WS[1];
                            cB = b.WS[1];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        case DiagonalSplit.XnZp:
                            xA = x;
                            xB = x + 1;
                            zA = z;
                            zB = z + 1;
                            qA = b.QA[3];
                            qB = b.QA[1];
                            eA = b.ED[3];
                            eB = b.ED[1];
                            rA = b.RF[3];
                            rB = b.RF[1];
                            wA = b.WS[3];
                            wB = b.WS[1];
                            fA = b.QA[2];
                            fB = b.QA[2];
                            cA = b.WS[2];
                            cB = b.WS[2];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                        default:
                            xA = x;
                            xB = x + 1;
                            zA = z + 1;
                            zB = z;
                            qA = b.QA[0];
                            qB = b.QA[2];
                            eA = b.ED[0];
                            eB = b.ED[2];
                            rA = b.RF[0];
                            rB = b.RF[2];
                            wA = b.WS[0];
                            wB = b.WS[2];
                            fA = b.QA[3];
                            fB = b.QA[3];
                            cA = b.WS[3];
                            cB = b.WS[3];
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                    }

                    break;

                default:
                    xA = x;
                    xB = x;
                    zA = z + 1;
                    zB = z;
                    ob = room.Blocks[x - 1, z];
                    qA = b.QA[0];
                    qB = b.QA[3];
                    eA = b.ED[0];
                    eB = b.ED[3];
                    rA = b.RF[0];
                    rB = b.RF[3];
                    wA = b.WS[0];
                    wB = b.WS[3];
                    fA = ob.QA[1];
                    fB = ob.QA[2];
                    cA = ob.WS[1];
                    cB = ob.WS[2];
                    qaFace = BlockFace.NegativeX_QA;
                    edFace = BlockFace.NegativeX_ED;
                    middleFace = BlockFace.NegativeX_Middle;
                    rfFace = BlockFace.NegativeX_RF;
                    wsFace = BlockFace.NegativeX_WS;

                    if (b.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = b.WallPortal;
                        var adjoiningRoom = portal.AdjoiningRoom;
                        if (room.Flipped && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Flipped && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[room.NumXSectors - 2, z];

                        var qaNearA = nearBlock.QA[1];
                        var qaNearB = nearBlock.QA[2];
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            qaNearA = qaNearB;
                        if (nearBlock.FloorDiagonalSplit == DiagonalSplit.XnZn)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.WS[1];
                        var wsNearB = nearBlock.WS[2];
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                            wsNearA = wsNearB;
                        if (nearBlock.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[0];
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.QA[3];
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            qAportal = qBportal;
                        if (adjoiningBlock.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[0];
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.WS[3];
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                            wAportal = wBportal;
                        if (adjoiningBlock.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[1];
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.ED[2];
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[1];
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.RF[2];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = b.QA[0];
                        qB = b.QA[0];
                    }

                    if (b.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = b.QA[3];
                        qB = b.QA[3];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = ob.QA[1];
                        fB = ob.QA[1];
                    }

                    if (ob.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = ob.QA[2];
                        fB = ob.QA[2];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = b.WS[0];
                        wB = b.WS[0];
                    }

                    if (b.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = b.WS[3];
                        wB = b.WS[3];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = ob.WS[1];
                        cB = ob.WS[1];
                    }

                    if (ob.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = ob.WS[2];
                        cB = ob.WS[2];
                    }

                    break;
            }

            var subdivide = false;

            // Always check these
            if (qA >= cA && qB >= cB)
            {
                qA = cA;
                qB = cB;
            }

            if (wA <= fA && wB <= fB)
            {
                wA = fA;
                wB = fB;
            }

            // Following checks are only for wall's faces
            if (b.IsAnyWall)
            {

                if (qA > fA && qB < fB || qA < fA && qB > fB)
                {
                    qA = fA;
                    qB = fB;
                }

                if (qA > cA && qB < cB || qA < cA && qB > cB)
                {
                    qA = cA;
                    qB = cB;
                }

                if (wA > cA && wB < cB || wA < cA && wB > cB)
                {
                    wA = cA;
                    wB = cB;
                }

                if (wA > fA && wB < fB || wA < fA && wB > fB)
                {
                    wA = fA;
                    wB = fB;
                }
            }

            if (!(qA == fA && qB == fB) && floor)
            {
                // Check for subdivision
                yA = fA;
                yB = fB;

                if (eA >= yA && eB >= yB && qA >= eA && qB >= eB && !(eA == yA && eB == yB))
                {
                    subdivide = true;
                    yA = eA;
                    yB = eB;
                }

                // QA and ED
                face = b.GetFaceTexture(qaFace);

                // QA
                if (qA > yA && qB > yB)
                    AddQuad(x, z, qaFace,
                        new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                        new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                        new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                        new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                        face, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                else if (qA == yA && qB > yB)
                    AddTriangle(x, z, qaFace,
                        new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                        new Vector3(xB * 1024.0f, qB * 256.0f, zB * 1024.0f),
                        new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                        face, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
                else if (qA > yA && qB == yB)
                    AddTriangle(x, z, qaFace,
                        new Vector3(xA * 1024.0f, qA * 256.0f, zA * 1024.0f),
                        new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                        new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                        face, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);

                // ED
                if (subdivide)
                {
                    yA = fA;
                    yB = fB;

                    face = b.GetFaceTexture(edFace);

                    if (eA > yA && eB > yB)
                        AddQuad(x, z, edFace,
                            new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                    else if (eA > yA && eB == yB)
                        AddTriangle(x, z, edFace,
                            new Vector3(xA * 1024.0f, eA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                    else if (eA == yA && eB > yB)
                        AddTriangle(x, z, edFace,
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, eB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            face, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
                }
            }

            subdivide = false;

            if (!(wA == cA && wB == cB) && ceiling)
            {
                // Check for subdivision
                yA = cA;
                yB = cB;

                if (rA <= yA && rB <= yB && wA <= rA && wB <= rB && !(rA == yA && rB == yB))
                {
                    subdivide = true;
                    yA = rA;
                    yB = rB;
                }

                // WS and RF
                face = b.GetFaceTexture(wsFace);

                // WS
                if (wA < yA && wB < yB)
                    AddQuad(x, z, wsFace,
                        new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                        new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                        new Vector3(xB * 1024.0f, wB * 256.0f, zB * 1024.0f),
                        new Vector3(xA * 1024.0f, wA * 256.0f, zA * 1024.0f),
                        face, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                else if (wA < yA && wB == yB)
                    AddTriangle(x, z, wsFace,
                        new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                        new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                        new Vector3(xA * 1024.0f, wA * 256.0f, zA * 1024.0f),
                        face, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                else if (wA == yA && wB < yB)
                    AddTriangle(x, z, wsFace,
                        new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                        new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                        new Vector3(xB * 1024.0f, wB * 256.0f, zB * 1024.0f),
                        face, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);

                // RF
                if (subdivide)
                {
                    yA = cA;
                    yB = cB;

                    face = b.GetFaceTexture(rfFace);

                    if (rA < yA && rB < yB)
                        AddQuad(x, z, rfFace,
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, rB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, rA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                    else if (rA < yA && rB == yB)
                        AddTriangle(x, z, rfFace,
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xA * 1024.0f, rA * 256.0f, zA * 1024.0f),
                            face, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                    else if (rA == yA && rB < yB)
                        AddTriangle(x, z, rfFace,
                            new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                            new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                            new Vector3(xB * 1024.0f, rB * 256.0f, zB * 1024.0f),
                            face, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
                }
            }

            if (!middle)
                return;

            face = b.GetFaceTexture(middleFace);

            yA = wA >= cA ? cA : wA;
            yB = wB >= cB ? cB : wB;
            int yD = qA <= fA ? fA : qA;
            int yC = qB <= fB ? fB : qB;

            // Middle
            if (yA != yD && yB != yC)
                AddQuad(x, z, middleFace,
                    new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                    new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                    new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                    new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                    face, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            else if (yA != yD && yB == yC)
                AddTriangle(x, z, middleFace,
                    new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                    new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                    new Vector3(xA * 1024.0f, yD * 256.0f, zA * 1024.0f),
                    face, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);

            else if (yA == yD && yB != yC)
                AddTriangle(x, z, middleFace,
                    new Vector3(xA * 1024.0f, yA * 256.0f, zA * 1024.0f),
                    new Vector3(xB * 1024.0f, yB * 256.0f, zB * 1024.0f),
                    new Vector3(xB * 1024.0f, yC * 256.0f, zB * 1024.0f),
                    face, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
        }

        private void AddQuad(int x, int z, BlockFace face, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
                             TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, Vector2 editorUV3,
                             bool isForCeiling = false)
        {
            if (texture.DoubleSided)
                DoubleSidedTriangleCount += 2;
            VertexRangeLookup.Add(new SectorInfo(x, z, face), new VertexRange(VertexPositions.Count, 6));

            VertexPositions.Add(p1);
            VertexPositions.Add(p2);
            VertexPositions.Add(p0);
            VertexPositions.Add(p3);
            VertexPositions.Add(p0);
            VertexPositions.Add(p2);

            VertexEditorUVs.Add(editorUV1);
            VertexEditorUVs.Add(editorUV2);
            VertexEditorUVs.Add(editorUV0);
            VertexEditorUVs.Add(editorUV3);
            VertexEditorUVs.Add(editorUV0);
            VertexEditorUVs.Add(editorUV2);

            TextureArea texture0 = texture;
            texture0.TexCoord0 = texture.TexCoord2;
            texture0.TexCoord1 = texture.TexCoord3;
            texture0.TexCoord2 = texture.TexCoord1;
            TriangleTextureAreas.Add(texture0);
            TriangleSectorInfo.Add(new SectorInfo(x, z, face));

            TextureArea texture1 = texture;
            texture1.TexCoord0 = texture.TexCoord0;
            texture1.TexCoord1 = texture.TexCoord1;
            texture1.TexCoord2 = texture.TexCoord3;
            TriangleTextureAreas.Add(texture1);
            TriangleSectorInfo.Add(new SectorInfo(x, z, face));
        }

        private void AddTriangle(int x, int z, BlockFace face, Vector3 p0, Vector3 p1, Vector3 p2, TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, bool isXEqualYDiagonal)
        {
            if (texture.DoubleSided)
                DoubleSidedTriangleCount += 1;
            Vector2 editorUvFactor = new Vector2(isXEqualYDiagonal ? -1.0f : 1.0f, -1.0f);
            VertexRangeLookup.Add(new SectorInfo(x, z, face), new VertexRange(VertexPositions.Count, 3));

            VertexPositions.Add(p0);
            VertexPositions.Add(p1);
            VertexPositions.Add(p2);

            VertexEditorUVs.Add(editorUV0 * editorUvFactor);
            VertexEditorUVs.Add(editorUV1 * editorUvFactor);
            VertexEditorUVs.Add(editorUV2 * editorUvFactor);

            TriangleTextureAreas.Add(texture);
            TriangleSectorInfo.Add(new SectorInfo(x, z, face));
        }

        public bool IsQuad(int i)
        {
            if (i + 6 > VertexPositions.Count)
                return false;
            if (VertexPositions[i + 1] != VertexPositions[i + 5] ||
                VertexPositions[i + 2] != VertexPositions[i + 4])
                return false;
            if (VertexColors[i + 1] != VertexColors[i + 5] ||
                VertexColors[i + 2] != VertexColors[i + 4])
                return false;
            TextureArea firstTexture = TriangleTextureAreas[i / 3];
            TextureArea secondTexture = TriangleTextureAreas[i / 3];
            firstTexture.TexCoord0 = secondTexture.TexCoord0;
            firstTexture.TexCoord3 = secondTexture.TexCoord3;
            Swap.Do(ref firstTexture.TexCoord1, ref firstTexture.TexCoord2);
            if (firstTexture != secondTexture)
                return false;
            return true;
        }

        private bool RayTraceCheckFloorCeiling(Room room, int x, int y, int z, int xLight, int zLight)
        {
            int currentX = x / 1024 - (x > xLight ? 1 : 0);
            int currentZ = z / 1024 - (z > zLight ? 1 : 0);

            Block block = room.Blocks[currentX, currentZ];
            int floorMin = block.FloorMin;
            int ceilingMax = block.CeilingMax;

            return floorMin <= y / 256 && ceilingMax >= y / 256;
        }

        private bool RayTraceX(Room room, int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minX;
            int maxX;

            yLight = -yLight;
            y = -y;

            int yPoint = y;
            int zPoint = z;

            if (x <= xLight)
            {
                deltaX = xLight - x;
                deltaY = yLight - y;
                deltaZ = zLight - z;

                minX = x;
                maxX = xLight;
            }
            else
            {
                deltaX = x - xLight;
                deltaY = y - yLight;
                deltaZ = z - zLight;

                minX = xLight;
                maxX = x;

                yPoint = yLight;
                zPoint = zLight;
            }

            // deltaY *= -1;

            if (deltaX == 0)
                return true;

            int fracX = (((minX >> 10) + 1) << 10) - minX;
            int currentX = ((minX >> 10) + 1) << 10;
            int currentZ = deltaZ * fracX / (deltaX + 1) + zPoint;
            int currentY = deltaY * fracX / (deltaX + 1) + yPoint;

            if (currentX > maxX)
                return true;

            do
            {
                int currentXblock = currentX / 1024;
                int currentZblock = currentZ / 1024;

                if (currentZblock < 0 || currentXblock >= room.NumXSectors || currentZblock >= room.NumZSectors)
                {
                    if (currentX == maxX)
                        return true;
                }
                else
                {
                    int currentYclick = currentY / -256;

                    if (currentXblock > 0)
                    {
                        Block currentBlock = room.Blocks[currentXblock - 1, currentZblock];

                        if ((currentBlock.QA[0] + currentBlock.QA[3]) / 2 > currentYclick ||
                            (currentBlock.WS[0] + currentBlock.WS[3]) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall)
                        {
                            return false;
                        }
                    }

                    if (currentX == maxX)
                    {
                        return true;
                    }

                    if (currentXblock > 0)
                    {
                        var currentBlock = room.Blocks[currentXblock - 1, currentZblock];
                        var nextBlock = room.Blocks[currentXblock, currentZblock];

                        if ((currentBlock.QA[2] + currentBlock.QA[1]) / 2 > currentYclick ||
                            (currentBlock.WS[2] + currentBlock.WS[1]) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall ||
                            (nextBlock.QA[0] + nextBlock.QA[3]) / 2 > currentYclick ||
                            (nextBlock.WS[0] + nextBlock.WS[3]) / 2 < currentYclick ||
                            nextBlock.Type == BlockType.Wall)
                        {
                            return false;
                        }
                    }
                }

                currentX += 1024;
                currentZ += (deltaZ << 10) / (deltaX + 1);
                currentY += (deltaY << 10) / (deltaX + 1);
            }
            while (currentX <= maxX);

            return true;
        }

        private bool RayTraceZ(Room room, int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minZ;
            int maxZ;

            yLight = -yLight;
            y = -y;

            int yPoint = y;
            int xPoint = x;

            if (z <= zLight)
            {
                deltaX = xLight - x;
                deltaY = yLight - y;
                deltaZ = zLight - z;

                minZ = z;
                maxZ = zLight;
            }
            else
            {
                deltaX = x - xLight;
                deltaY = y - yLight;
                deltaZ = z - zLight;

                minZ = zLight;
                maxZ = z;

                xPoint = xLight;
                yPoint = yLight;
            }

            //deltaY *= -1;

            if (deltaZ == 0)
                return true;

            int fracZ = (((minZ >> 10) + 1) << 10) - minZ;
            int currentZ = ((minZ >> 10) + 1) << 10;
            int currentX = deltaX * fracZ / (deltaZ + 1) + xPoint;
            int currentY = deltaY * fracZ / (deltaZ + 1) + yPoint;

            if (currentZ > maxZ)
                return true;

            do
            {
                int currentXblock = currentX / 1024;
                int currentZblock = currentZ / 1024;

                if (currentXblock < 0 || currentZblock >= room.NumZSectors || currentXblock >= room.NumXSectors)
                {
                    if (currentZ == maxZ)
                        return true;
                }
                else
                {
                    int currentYclick = currentY / -256;

                    if (currentZblock > 0)
                    {
                        var currentBlock = room.Blocks[currentXblock, currentZblock - 1];

                        if ((currentBlock.QA[2] + currentBlock.QA[3]) / 2 > currentYclick ||
                            (currentBlock.WS[2] + currentBlock.WS[3]) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall)
                        {
                            return false;
                        }
                    }

                    if (currentZ == maxZ)
                    {
                        return true;
                    }

                    if (currentZblock > 0)
                    {
                        var currentBlock = room.Blocks[currentXblock, currentZblock - 1];
                        var nextBlock = room.Blocks[currentXblock, currentZblock];

                        if ((currentBlock.QA[0] + currentBlock.QA[1]) / 2 > currentYclick ||
                            (currentBlock.WS[0] + currentBlock.WS[1]) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall ||
                            (nextBlock.QA[2] + nextBlock.QA[3]) / 2 > currentYclick ||
                            (nextBlock.WS[2] + nextBlock.WS[3]) / 2 < currentYclick ||
                            nextBlock.Type == BlockType.Wall)
                        {
                            return false;
                        }
                    }
                }

                currentZ += 1024;
                currentX += (deltaX << 10) / (deltaZ + 1);
                currentY += (deltaY << 10) / (deltaZ + 1);
            }
            while (currentZ <= maxZ);

            return true;
        }

        public void Relight(Room room)
        {
            // Collect lights
            List<LightInstance> lights = new List<LightInstance>();
            foreach (var instance in room.Objects)
            {
                LightInstance light = instance as LightInstance;
                if (light != null)
                    lights.Add(light);
            }

            // Calculate lighting
            for (int i = 0; i < VertexPositions.Count; i += 3)
            {
                var normal = Vector3.Cross(
                    VertexPositions[i + 1] - VertexPositions[i],
                    VertexPositions[i + 2] - VertexPositions[i]);
                normal = Vector3.Normalize(normal);

                for (int j = 0; j < 3; ++j)
                {
                    var position = VertexPositions[i + j];
                    Vector3 color = room.AmbientLight * 128;

                    foreach (var light in lights) // No Linq here because it's slow
                    {
                        if (!light.Enabled || !light.IsStaticallyUsed)
                            continue;

                        switch (light.Type)
                        {
                            case LightType.Point:
                            case LightType.Shadow:
                                if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                                {
                                    // Get the distance between light and vertex
                                    float distance = Math.Abs((position - light.Position).Length());

                                    // If distance is greater than light out radius, then skip this light
                                    if (distance > light.OuterRange * 1024.0f)
                                        continue;

                                    // Calculate light diffuse value
                                    int diffuse = (int)(light.Intensity * 8192);

                                    // Calculate the length squared of the normal vector
                                    float dotN = Vector3.Dot(normal, normal);

                                    // Do raytracing
                                    if (dotN <= 0 ||
                                        !RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                        !RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                        !RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                    {
                                        if (light.IsObstructedByRoomGeometry)
                                            continue;
                                    }

                                    // Calculate the attenuation
                                    float attenuaton = (light.OuterRange * 1024.0f - distance) / (light.OuterRange * 1024.0f - light.InnerRange * 1024.0f);
                                    if (attenuaton > 1.0f)
                                        attenuaton = 1.0f;
                                    if (attenuaton <= 0.0f)
                                        continue;

                                    // Calculate final light color
                                    float finalIntensity = dotN * attenuaton * diffuse;
                                    color += finalIntensity * light.Color * (1.0f / 64.0f);
                                }
                                break;
                            case LightType.Effect:
                                if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                                {
                                    int x1 = (int)(Math.Floor(light.Position.X / 1024.0f) * 1024);
                                    int z1 = (int)(Math.Floor(light.Position.Z / 1024.0f) * 1024);
                                    int x2 = (int)(Math.Ceiling(light.Position.X / 1024.0f) * 1024);
                                    int z2 = (int)(Math.Ceiling(light.Position.Z / 1024.0f) * 1024);

                                    // TODO: winroomedit was supporting effect lights placed on vertical faces and effects light was applied to owning face
                                    // ReSharper disable CompareOfFloatsByEqualityOperator
                                    if ((position.X == x1 && position.Z == z1 || position.X == x1 && position.Z == z2 || position.X == x2 && position.Z == z1 ||
                                         position.X == x2 && position.Z == z2) && position.Y <= light.Position.Y)
                                    {
                                        float finalIntensity = light.Intensity * 8192 * 0.25f;
                                        color += finalIntensity * light.Color * (1.0f / 64.0f);
                                    }
                                    // ReSharper restore CompareOfFloatsByEqualityOperator
                                }
                                break;
                            case LightType.Sun:
                                {
                                    // Do raytracing now for saving CPU later
                                    if (!RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                        !RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                        !RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                    {
                                        if (light.IsObstructedByRoomGeometry)
                                            continue;
                                    }

                                    // Calculate the light direction
                                    Vector3 lightDirection = light.GetDirection();

                                    // calcolo la luce diffusa
                                    float diffuse = -Vector3.Dot(lightDirection, normal);

                                    if (diffuse <= 0)
                                        continue;

                                    if (diffuse > 1)
                                        diffuse = 1.0f;


                                    float finalIntensity = diffuse * light.Intensity * 8192.0f;
                                    if (finalIntensity < 0)
                                        continue;
                                    color += finalIntensity * light.Color * (1.0f / 64.0f);
                                }
                                break;
                            case LightType.Spot:
                                if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                                {
                                    // Calculate the ray from light to vertex
                                    Vector3 lightVector = Vector3.Normalize(position - light.Position);

                                    // Get the distance between light and vertex
                                    float distance = Math.Abs((position - light.Position).Length());

                                    // If distance is greater than light length, then skip this light
                                    if (distance > light.OuterRange * 1024.0f)
                                        continue;

                                    // Calculate the light direction
                                    Vector3 lightDirection = light.GetDirection();

                                    // Calculate the cosines values for In, Out
                                    double d = Vector3.Dot(lightVector, lightDirection);
                                    double cosI2 = Math.Cos(light.InnerAngle * (Math.PI / 180));
                                    double cosO2 = Math.Cos(light.OuterAngle * (Math.PI / 180));

                                    if (d < cosO2)
                                        continue;

                                    if (!RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                                        !RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                                        !RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z))
                                    {
                                        if (light.IsObstructedByRoomGeometry)
                                            continue;
                                    }

                                    // Calculate light diffuse value
                                    float factor = (float)(1.0f - (d - cosI2) / (cosO2 - cosI2));
                                    if (factor > 1.0f)
                                        factor = 1.0f;
                                    if (factor <= 0.0f)
                                        continue;

                                    float attenuation = 1.0f;
                                    if (distance >= light.InnerRange * 1024.0f)
                                        attenuation = 1.0f - (distance - light.InnerRange * 1024.0f) / (light.OuterRange * 1024.0f - light.InnerRange * 1024.0f);

                                    if (attenuation > 1.0f)
                                        attenuation = 1.0f;
                                    if (attenuation < 0.0f)
                                        continue;

                                    float dot1 = -Vector3.Dot(lightDirection, normal);
                                    if (dot1 < 0.0f)
                                        continue;
                                    if (dot1 > 1.0f)
                                        dot1 = 1.0f;

                                    float finalIntensity = attenuation * dot1 * factor * light.Intensity * 8192.0f;
                                    color += finalIntensity * light.Color * (1.0f / 64.0f);
                                }
                                break;
                        }
                    }

                    // Apply color
                    VertexColors[i + j] = Vector3.Max(color, new Vector3()) * (1.0f / 128.0f);
                }
            }

            // Calculate light average for shared vertices
            foreach (var pair in SharedVertices)
            {
                Vector3 faceColorSum = new Vector3();
                foreach (var vertexIndex in pair.Value)
                    faceColorSum += VertexColors[vertexIndex];
                faceColorSum /= pair.Value.Count;
                foreach (var vertexIndex in pair.Value)
                    VertexColors[vertexIndex] = faceColorSum;
            }
        }

        public struct IntersectionInfo
        {
            public VectorInt2 Pos;
            public BlockFace Face;
            public float Distance;
            public float VerticalCoord;
        }

        public IntersectionInfo? RayIntersectsGeometry(Ray ray)
        {
            IntersectionInfo result = new IntersectionInfo { Distance = float.NaN };
            foreach (var entry in VertexRangeLookup)
                for (int i = 0; i < entry.Value.Count; i += 3)
                {
                    var p0 = VertexPositions[entry.Value.Start + i];
                    var p1 = VertexPositions[entry.Value.Start + i + 1];
                    var p2 = VertexPositions[entry.Value.Start + i + 2];

                    Vector3 position;
                    if (Collision.RayIntersectsTriangle(ray, p0, p1, p2, out position))
                    {
                        float distance = (position - ray.Position).Length();
                        var normal = Vector3.Cross(p1 - p0, p2 - p0);
                        if (Vector3.Dot(ray.Direction, normal) <= 0)
                            if (!(distance > result.Distance))
                                result = new IntersectionInfo() { Distance = distance, Face = entry.Key.Face, Pos = entry.Key.Pos, VerticalCoord = position.Y };
                    }
                }

            if (float.IsNaN(result.Distance))
                return null;
            return result;
        }
    }

    public struct SectorInfo : IEquatable<SectorInfo>, IComparable, IComparable<SectorInfo>
    {
        public VectorInt2 Pos;
        public BlockFace Face;

        public SectorInfo(int x, int z, BlockFace face) { Pos = new VectorInt2(x, z); Face = face; }
        public override bool Equals(object other) => other is SectorInfo ? ((SectorInfo)other).Equals(other) : false;
        public bool Equals(SectorInfo other) => Pos == other.Pos && Face == other.Face;
        public override int GetHashCode() => Pos.GetHashCode() ^ (1200049507 * (int)Face); // Random prime
        public int CompareTo(SectorInfo other)
        {
            if (Pos.X != other.Pos.X)
                return Pos.X > other.Pos.X ? 1 : -1;
            if (Pos.Y != other.Pos.Y)
                return Pos.Y > other.Pos.Y ? 1 : -1;
            if (Face != other.Face)
                return Face > other.Face ? 1 : -1;
            return 0;
        }
        int IComparable.CompareTo(object other) => CompareTo((SectorInfo)other);
    }

    public struct VertexRange
    {
        public int Start;
        public int Count;

        public VertexRange(int start, int count) { Start = start; Count = count; }
    }
}
