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
                         (Blocks[x, z + 1].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x, z + 1].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z + 1].Floor.DiagonalSplit == DiagonalSplit.XnZn)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZn))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, false);
                    }


                    // -Z direction
                    if (x > 0 && x < room.NumXSectors - 1 && z > 1 && z < room.NumZSectors - 1 &&
                        !(Blocks[x, z - 1].Type == BlockType.Wall &&
                         (Blocks[x, z - 1].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x, z - 1].Floor.DiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z - 1].Floor.DiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall ||
                            (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, false);
                    }

                    // +X direction
                    if (z > 0 && z < room.NumZSectors - 1 && x > 0 && x < room.NumXSectors - 2 &&
                        !(Blocks[x + 1, z].Type == BlockType.Wall &&
                        (Blocks[x + 1, z].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x + 1, z].Floor.DiagonalSplit == DiagonalSplit.XnZn || Blocks[x + 1, z].Floor.DiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZn || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, false);
                    }

                    // -X direction
                    if (z > 0 && z < room.NumZSectors - 1 && x > 1 && x < room.NumXSectors - 1 &&
                        !(Blocks[x - 1, z].Type == BlockType.Wall &&
                        (Blocks[x - 1, z].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x - 1, z].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x - 1, z].Floor.DiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZp))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, true);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, false);
                    }

                    // Diagonal faces
                    if (Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
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

                    if (Blocks[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type != BlockType.Wall)
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalCeiling, false, true, false);
                        }
                    }

                    // +Z directed border wall
                    if (z == 0 && x != 0 && x != room.NumXSectors - 1 &&
                        !(Blocks[x, 1].Type == BlockType.Wall &&
                         (Blocks[x, 1].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x, 1].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x, 1].Floor.DiagonalSplit == DiagonalSplit.XnZn)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Alternated && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingX = x + (room.Position.X - adjoiningRoom.Position.X);
                            var block = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.Floor.DiagonalSplit == DiagonalSplit.None ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XnZp ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XpZp))
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
                         (Blocks[x, room.NumZSectors - 2].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x, room.NumZSectors - 2].Floor.DiagonalSplit == DiagonalSplit.XpZp || Blocks[x, room.NumZSectors - 2].Floor.DiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Alternated && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingX = x + (room.Position.X - adjoiningRoom.Position.X);
                            var block = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.Floor.DiagonalSplit == DiagonalSplit.None ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XnZn ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XpZn))
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
                         (Blocks[1, z].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[1, z].Floor.DiagonalSplit == DiagonalSplit.XnZn || Blocks[1, z].Floor.DiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Alternated && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);
                            var block = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.Floor.DiagonalSplit == DiagonalSplit.None ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XpZn ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XpZp))
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
                         (Blocks[room.NumXSectors - 2, z].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[room.NumXSectors - 2, z].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[room.NumXSectors - 2, z].Floor.DiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        bool addMiddle = false;

                        if (Blocks[x, z].WallPortal != null)
                        {
                            var portal = Blocks[x, z].WallPortal;
                            var adjoiningRoom = portal.AdjoiningRoom;
                            if (room.Alternated && room.AlternateBaseRoom != null)
                            {
                                if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                    adjoiningRoom = adjoiningRoom.AlternateRoom;
                            }

                            int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);
                            var block = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;
                            if (block.Type == BlockType.Wall &&
                                (block.Floor.DiagonalSplit == DiagonalSplit.None ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XnZn ||
                                 block.Floor.DiagonalSplit == DiagonalSplit.XnZp))
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
                    BuildFloorOrCeilingFace(room, x, z, Blocks[x, z].Floor.XnZp, Blocks[x, z].Floor.XpZp, Blocks[x, z].Floor.XpZn, Blocks[x, z].Floor.XnZn, Blocks[x, z].Floor.DiagonalSplit, Blocks[x, z].Floor.SplitDirectionIsXEqualsZ,
                        BlockFace.Floor, BlockFace.FloorTriangle2, floorPortalInfo.VisualType);

                    // Ceiling polygons
                    int ceilingStartVertex = VertexPositions.Count;

                    Room.RoomConnectionInfo ceilingPortalInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z));
                    BuildFloorOrCeilingFace(room, x, z, Blocks[x, z].Ceiling.XnZp, Blocks[x, z].Ceiling.XpZp, Blocks[x, z].Ceiling.XpZn, Blocks[x, z].Ceiling.XnZn, Blocks[x, z].Ceiling.DiagonalSplit, Blocks[x, z].Ceiling.SplitDirectionIsXEqualsZ,
                        BlockFace.Ceiling, BlockFace.CeilingTriangle2, ceilingPortalInfo.VisualType);

                    // Change vertices order for ceiling polygons
                    for (int i = ceilingStartVertex; i < VertexPositions.Count; i += 3)
                    {
                        var tempPosition = VertexPositions[i + 2];
                        VertexPositions[i + 2] = VertexPositions[i];
                        VertexPositions[i] = tempPosition;
                        var tempEditorUV = VertexEditorUVs[i + 2];
                        VertexEditorUVs[i + 2] = VertexEditorUVs[i];
                        VertexEditorUVs[i] = tempEditorUV;
                        TextureArea textureArea = TriangleTextureAreas[i / 3];
                        Swap.Do(ref textureArea.TexCoord0, ref textureArea.TexCoord2);
                        TriangleTextureAreas[i / 3] = textureArea;
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
                                             BlockFace face1, BlockFace face2, Room.RoomConnectionType portalMode)
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
                    if (BlockSurface.IsQuad2(h0, h1, h2, h3))
                        diagonalSplitXEqualsY = true;
                    break;
                case Room.RoomConnectionType.TriangularPortalXpZp:
                case Room.RoomConnectionType.TriangularPortalXnZn:
                    if (BlockSurface.IsQuad2(h0, h1, h2, h3))
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
            else if (BlockSurface.IsQuad2(h0, h1, h2, h3) && portalMode == Room.RoomConnectionType.NoPortal)
            {
                AddQuad(x, z, face1,
                    new Vector3(x * 1024.0f, h0 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h1 * 256.0f, (z + 1) * 1024.0f),
                    new Vector3((x + 1) * 1024.0f, h2 * 256.0f, z * 1024.0f),
                    new Vector3(x * 1024.0f, h3 * 256.0f, z * 1024.0f),
                    room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
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
                    qA = b.Floor.XpZp;
                    qB = b.Floor.XnZp;
                    eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                    eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                    rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                    rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                    wA = b.Ceiling.XpZp;
                    wB = b.Ceiling.XnZp;
                    fA = ob.Floor.XpZn;
                    fB = ob.Floor.XnZn;
                    cA = ob.Ceiling.XpZn;
                    cB = ob.Ceiling.XnZn;
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
                        if (room.Alternated && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[x, 1];

                        var qaNearA = nearBlock.Floor.XpZn;
                        var qaNearB = nearBlock.Floor.XnZn;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                            qaNearA = qaNearB;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.Ceiling.XpZn;
                        var wsNearB = nearBlock.Ceiling.XnZn;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wsNearA = wsNearB;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZp;
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZp;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            qAportal = qBportal;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = b.Floor.XnZp;
                        qB = b.Floor.XnZp;
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = b.Floor.XpZp;
                        qB = b.Floor.XpZp;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = ob.Floor.XpZn;
                        fB = ob.Floor.XpZn;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = ob.Floor.XnZn;
                        fB = ob.Floor.XnZn;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = b.Ceiling.XnZp;
                        wB = b.Ceiling.XnZp;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = b.Ceiling.XpZp;
                        wB = b.Ceiling.XpZp;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = ob.Ceiling.XpZn;
                        cB = ob.Ceiling.XpZn;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = ob.Ceiling.XnZn;
                        cB = ob.Ceiling.XnZn;
                    }

                    break;

                case FaceDirection.NegativeZ:
                    xA = x;
                    xB = x + 1;
                    zA = z;
                    zB = z;
                    ob = room.Blocks[x, z - 1];
                    qA = b.Floor.XnZn;
                    qB = b.Floor.XpZn;
                    eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                    eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                    rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                    rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                    wA = b.Ceiling.XnZn;
                    wB = b.Ceiling.XpZn;
                    fA = ob.Floor.XnZp;
                    fB = ob.Floor.XpZp;
                    cA = ob.Ceiling.XnZp;
                    cB = ob.Ceiling.XpZp;
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
                        if (room.Alternated && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[x, room.NumZSectors - 2];

                        var qaNearA = nearBlock.Floor.XnZp;
                        var qaNearB = nearBlock.Floor.XpZp;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                            qaNearA = qaNearB;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.Ceiling.XnZp;
                        var wsNearB = nearBlock.Ceiling.XpZp;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wsNearA = wsNearB;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZn;
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZn;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            qAportal = qBportal;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = b.Floor.XnZn;
                        qB = b.Floor.XnZn;
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = b.Floor.XpZn;
                        qB = b.Floor.XpZn;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = ob.Floor.XnZp;
                        fB = ob.Floor.XnZp;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = ob.Floor.XpZp;
                        fB = ob.Floor.XpZp;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = b.Ceiling.XnZn;
                        wB = b.Ceiling.XnZn;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = b.Ceiling.XpZn;
                        wB = b.Ceiling.XpZn;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = ob.Ceiling.XnZp;
                        cB = ob.Ceiling.XnZp;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = ob.Ceiling.XpZp;
                        cB = ob.Ceiling.XpZp;
                    }

                    break;

                case FaceDirection.PositiveX:
                    xA = x + 1;
                    xB = x + 1;
                    zA = z;
                    zB = z + 1;
                    ob = room.Blocks[x + 1, z];
                    qA = b.Floor.XpZn;
                    qB = b.Floor.XpZp;
                    eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                    eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                    rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                    rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                    wA = b.Ceiling.XpZn;
                    wB = b.Ceiling.XpZp;
                    fA = ob.Floor.XnZn;
                    fB = ob.Floor.XnZp;
                    cA = ob.Ceiling.XnZn;
                    cB = ob.Ceiling.XnZp;
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
                        if (room.Alternated && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[1, z];

                        var qaNearA = nearBlock.Floor.XnZn;
                        var qaNearB = nearBlock.Floor.XnZp;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            qaNearA = qaNearB;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.Ceiling.XnZn;
                        var wsNearB = nearBlock.Ceiling.XnZp;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wsNearA = wsNearB;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZn;
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZp;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                            qAportal = qBportal;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        qA = b.Floor.XpZp;
                        qB = b.Floor.XpZp;
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        qA = b.Floor.XpZn;
                        qB = b.Floor.XpZn;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        fA = ob.Floor.XnZp;
                        fB = ob.Floor.XnZp;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        fA = ob.Floor.XnZn;
                        fB = ob.Floor.XnZn;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        wA = b.Ceiling.XpZp;
                        wB = b.Ceiling.XpZp;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        wA = b.Ceiling.XpZn;
                        wB = b.Ceiling.XpZn;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        cA = ob.Ceiling.XnZp;
                        cB = ob.Ceiling.XnZp;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        cA = ob.Ceiling.XnZn;
                        cB = ob.Ceiling.XnZn;
                    }

                    break;

                case FaceDirection.DiagonalFloor:
                    switch (b.Floor.DiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            qA = b.Floor.XpZp;
                            qB = b.Floor.XnZn;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                            wA = b.Ceiling.XpZp;
                            wB = b.Ceiling.XnZn;
                            fA = b.Floor.XnZp;
                            fB = b.Floor.XnZp;
                            cA = b.IsAnyWall ? b.Ceiling.XnZp : b.Ceiling.XpZp;
                            cB = b.IsAnyWall ? b.Ceiling.XnZp : b.Ceiling.XnZn;
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
                            qA = b.Floor.XpZn;
                            qB = b.Floor.XnZp;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                            wA = b.Ceiling.XpZn;
                            wB = b.Ceiling.XnZp;
                            fA = b.Floor.XpZp;
                            fB = b.Floor.XpZp;
                            cA = b.IsAnyWall ? b.Ceiling.XpZp : b.Ceiling.XpZn;
                            cB = b.IsAnyWall ? b.Ceiling.XpZp : b.Ceiling.XnZp;
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
                            qA = b.Floor.XnZn;
                            qB = b.Floor.XpZp;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                            wA = b.Ceiling.XnZn;
                            wB = b.Ceiling.XpZp;
                            fA = b.Floor.XpZn;
                            fB = b.Floor.XpZn;
                            cA = b.IsAnyWall ? b.Ceiling.XpZn : b.Ceiling.XnZn;
                            cB = b.IsAnyWall ? b.Ceiling.XpZn : b.Ceiling.XpZp;
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
                            qA = b.Floor.XnZp;
                            qB = b.Floor.XpZn;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                            wA = b.Ceiling.XnZp;
                            wB = b.Ceiling.XpZn;
                            fA = b.Floor.XnZn;
                            fB = b.Floor.XnZn;
                            cA = b.IsAnyWall ? b.Ceiling.XnZn : b.Ceiling.XnZp;
                            cB = b.IsAnyWall ? b.Ceiling.XnZn : b.Ceiling.XpZn;
                            qaFace = BlockFace.DiagonalQA;
                            edFace = BlockFace.DiagonalED;
                            middleFace = BlockFace.DiagonalMiddle;
                            rfFace = BlockFace.DiagonalRF;
                            wsFace = BlockFace.DiagonalWS;
                            break;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    switch (b.Ceiling.DiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            qA = b.Floor.XpZp;
                            qB = b.Floor.XnZn;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                            wA = b.Ceiling.XpZp;
                            wB = b.Ceiling.XnZn;
                            fA = b.IsAnyWall ? b.Floor.XnZp : b.Floor.XpZp;
                            fB = b.IsAnyWall ? b.Floor.XnZp : b.Floor.XnZn;
                            cA = b.Ceiling.XnZp;
                            cB = b.Ceiling.XnZp;
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
                            qA = b.Floor.XpZn;
                            qB = b.Floor.XnZp;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                            wA = b.Ceiling.XpZn;
                            wB = b.Ceiling.XnZp;
                            fA = b.IsAnyWall ? b.Floor.XpZp : b.Floor.XpZn;
                            fB = b.IsAnyWall ? b.Floor.XpZp : b.Floor.XnZp;
                            cA = b.Ceiling.XpZp;
                            cB = b.Ceiling.XpZp;
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
                            qA = b.Floor.XnZn;
                            qB = b.Floor.XpZp;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZp);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZp);
                            wA = b.Ceiling.XnZn;
                            wB = b.Ceiling.XpZp;
                            fA = b.IsAnyWall ? b.Floor.XpZn : b.Floor.XnZn;
                            fB = b.IsAnyWall ? b.Floor.XpZn : b.Floor.XpZp;
                            cA = b.Ceiling.XpZn;
                            cB = b.Ceiling.XpZn;
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
                            qA = b.Floor.XnZp;
                            qB = b.Floor.XpZn;
                            eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                            eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XpZn);
                            rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                            rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XpZn);
                            wA = b.Ceiling.XnZp;
                            wB = b.Ceiling.XpZn;
                            fA = b.IsAnyWall ? b.Floor.XnZn : b.Floor.XnZp;
                            fB = b.IsAnyWall ? b.Floor.XnZn : b.Floor.XpZn;
                            cA = b.Ceiling.XnZn;
                            cB = b.Ceiling.XnZn;
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
                    qA = b.Floor.XnZp;
                    qB = b.Floor.XnZn;
                    eA = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                    eB = b.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                    rA = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                    rB = b.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                    wA = b.Ceiling.XnZp;
                    wB = b.Ceiling.XnZn;
                    fA = ob.Floor.XpZp;
                    fB = ob.Floor.XpZn;
                    cA = ob.Ceiling.XpZp;
                    cB = ob.Ceiling.XpZn;
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
                        if (room.Alternated && room.AlternateBaseRoom != null)
                        {
                            if (adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom != null)
                                adjoiningRoom = adjoiningRoom.AlternateRoom;
                        }

                        // Get the near block in current room
                        var nearBlock = room.Blocks[room.NumXSectors - 2, z];

                        var qaNearA = nearBlock.Floor.XpZp;
                        var qaNearB = nearBlock.Floor.XpZn;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            qaNearA = qaNearB;
                        if (nearBlock.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                            qaNearB = qaNearA;

                        var wsNearA = nearBlock.Ceiling.XpZp;
                        var wsNearB = nearBlock.Ceiling.XpZn;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                            wsNearA = wsNearB;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wsNearB = wsNearA;

                        // Now get the facing block on the adjoining room and calculate the correct heights
                        int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

                        var adjoiningBlock = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;

                        int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZp;
                        int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZn;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                            qAportal = qBportal;
                        if (adjoiningBlock.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            qBportal = qAportal;

                        qA = room.Position.Y + qaNearA;
                        qB = room.Position.Y + qaNearB;
                        qA = Math.Max(qA, qAportal) - room.Position.Y;
                        qB = Math.Max(qB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wBportal = wAportal;

                        wA = room.Position.Y + wsNearA;
                        wB = room.Position.Y + wsNearB;
                        wA = Math.Min(wA, wAportal) - room.Position.Y;
                        wB = Math.Min(wB, wBportal) - room.Position.Y;

                        eA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XnZp);
                        eB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Ed, BlockEdge.XnZn);
                        rA = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XnZp);
                        rB = adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVertical.Rf, BlockEdge.XnZn);
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        qA = b.Floor.XnZp;
                        qB = b.Floor.XnZp;
                    }

                    if (b.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        qA = b.Floor.XnZn;
                        qB = b.Floor.XnZn;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        fA = ob.Floor.XpZp;
                        fB = ob.Floor.XpZp;
                    }

                    if (ob.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        fA = ob.Floor.XpZn;
                        fB = ob.Floor.XpZn;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        wA = b.Ceiling.XnZp;
                        wB = b.Ceiling.XnZp;
                    }

                    if (b.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        wA = b.Ceiling.XnZn;
                        wB = b.Ceiling.XnZn;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        cA = ob.Ceiling.XpZp;
                        cB = ob.Ceiling.XpZp;
                    }

                    if (ob.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        cA = ob.Ceiling.XpZn;
                        cB = ob.Ceiling.XpZn;
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
                             TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, Vector2 editorUV3)
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

        private static bool RayTraceCheckFloorCeiling(Room room, int x, int y, int z, int xLight, int zLight)
        {
            int currentX = x / 1024 - (x > xLight ? 1 : 0);
            int currentZ = z / 1024 - (z > zLight ? 1 : 0);

            Block block = room.Blocks[currentX, currentZ];
            int floorMin = block.Floor.Min;
            int ceilingMax = block.Ceiling.Max;

            return floorMin <= y / 256 && ceilingMax >= y / 256;
        }

        private static bool RayTraceX(Room room, int x, int y, int z, int xLight, int yLight, int zLight)
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

                        if ((currentBlock.Floor.XnZp + currentBlock.Floor.XnZn) / 2 > currentYclick ||
                            (currentBlock.Ceiling.XnZp + currentBlock.Ceiling.XnZn) / 2 < currentYclick ||
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

                        if ((currentBlock.Floor.XpZn + currentBlock.Floor.XpZp) / 2 > currentYclick ||
                            (currentBlock.Ceiling.XpZn + currentBlock.Ceiling.XpZp) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall ||
                            (nextBlock.Floor.XnZp + nextBlock.Floor.XnZn) / 2 > currentYclick ||
                            (nextBlock.Ceiling.XnZp + nextBlock.Ceiling.XnZn) / 2 < currentYclick ||
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

        private static bool RayTraceZ(Room room, int x, int y, int z, int xLight, int yLight, int zLight)
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

                        if ((currentBlock.Floor.XpZn + currentBlock.Floor.XnZn) / 2 > currentYclick ||
                            (currentBlock.Ceiling.XpZn + currentBlock.Ceiling.XnZn) / 2 < currentYclick ||
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

                        if ((currentBlock.Floor.XnZp + currentBlock.Floor.XpZp) / 2 > currentYclick ||
                            (currentBlock.Ceiling.XnZp + currentBlock.Ceiling.XpZp) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall ||
                            (nextBlock.Floor.XpZn + nextBlock.Floor.XnZn) / 2 > currentYclick ||
                            (nextBlock.Ceiling.XpZn + nextBlock.Ceiling.XnZn) / 2 < currentYclick ||
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

        public static Vector3 CalculateLightForVertex(Room room, LightInstance light, Vector3 position, 
                                                      Vector3 normal, bool forRooms)
        {
            if (!light.Enabled || !light.IsStaticallyUsed)
                return Vector3.Zero;

            Vector3 lightDirection;
            Vector3 lightVector;
            float distance;

            switch (light.Type)
            {
                case LightType.Point:
                case LightType.Shadow:
                    // Get the light vector
                    lightVector = position - light.Position;

                    // Get the distance between light and vertex
                    distance = lightVector.Length();

                    // Normalize the light vector
                    lightVector = Vector3.Normalize(lightVector);

                    if (distance + 64.0f <= light.OuterRange * 1024.0f)
                    {     
                        // If distance is greater than light out radius, then skip this light
                        if (distance > light.OuterRange * 1024.0f)
                            return Vector3.Zero;

                        // Calculate light diffuse value
                        int diffuse = (int)(light.Intensity * 8192);

                        // Calculate the length squared of the normal vector
                        float dotN = Vector3.Dot((!forRooms ? -lightVector : normal), normal);

                        // Do raytracing
                        if (dotN <= 0 || forRooms && (
                            !RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                            !RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z)))
                        {
                            if (light.IsObstructedByRoomGeometry)
                                return Vector3.Zero;
                        }

                        // Calculate the attenuation
                        float attenuaton = (light.OuterRange * 1024.0f - distance) / (light.OuterRange * 1024.0f - light.InnerRange * 1024.0f);
                        if (attenuaton > 1.0f)
                            attenuaton = 1.0f;
                        if (attenuaton <= 0.0f)
                            return Vector3.Zero;

                        // Calculate final light color
                        float finalIntensity = dotN * attenuaton * diffuse;
                        return finalIntensity * light.Color * (1.0f / 64.0f);
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
                        if ((position.X == x1 && position.Z == z1 || position.X == x1 && position.Z == z2 || position.X == x2 && position.Z == z1 ||
                             position.X == x2 && position.Z == z2) && position.Y <= light.Position.Y)
                        {
                            float finalIntensity = light.Intensity * 8192 * 0.25f;
                            return finalIntensity * light.Color * (1.0f / 64.0f);
                        }
                    }
                    break;

                case LightType.Sun:
                    {
                        // Do raytracing now for saving CPU later
                        if (forRooms && (!RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                            !RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z)))
                        {
                            if (light.IsObstructedByRoomGeometry)
                                return Vector3.Zero;
                        }

                        // Calculate the light direction
                        lightDirection = light.GetDirection();

                        // calcolo la luce diffusa
                        float diffuse = -Vector3.Dot(lightDirection, normal);

                        if (diffuse <= 0)
                            return Vector3.Zero;

                        if (diffuse > 1)
                            diffuse = 1.0f;


                        float finalIntensity = diffuse * light.Intensity * 8192.0f;
                        if (finalIntensity < 0)
                            return Vector3.Zero;
                        return finalIntensity * light.Color * (1.0f / 64.0f);
                    }

                case LightType.Spot:
                    if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * 1024.0f)
                    {
                        // Calculate the ray from light to vertex
                        lightVector = Vector3.Normalize(position - light.Position);

                        // Get the distance between light and vertex
                        distance = Math.Abs((position - light.Position).Length());

                        // If distance is greater than light length, then skip this light
                        if (distance > light.OuterRange * 1024.0f)
                            return Vector3.Zero;

                        // Calculate the light direction
                        lightDirection = light.GetDirection();

                        // Calculate the cosines values for In, Out
                        double d = Vector3.Dot(lightVector, lightDirection);
                        double cosI2 = Math.Cos(light.InnerAngle * (Math.PI / 180));
                        double cosO2 = Math.Cos(light.OuterAngle * (Math.PI / 180));

                        if (d < cosO2)
                            return Vector3.Zero;

                        if (forRooms && (!RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Z) ||
                            !RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z) ||
                            !RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)light.Position.X, (int)light.Position.Y, (int)light.Position.Z)))
                        {
                            if (light.IsObstructedByRoomGeometry)
                                return Vector3.Zero;
                        }

                        // Calculate light diffuse value
                        float factor = (float)(1.0f - (d - cosI2) / (cosO2 - cosI2));
                        if (factor > 1.0f)
                            factor = 1.0f;
                        if (factor <= 0.0f)
                            return Vector3.Zero;

                        float attenuation = 1.0f;
                        if (distance >= light.InnerRange * 1024.0f)
                            attenuation = 1.0f - (distance - light.InnerRange * 1024.0f) / (light.OuterRange * 1024.0f - light.InnerRange * 1024.0f);

                        if (attenuation > 1.0f)
                            attenuation = 1.0f;
                        if (attenuation < 0.0f)
                            return Vector3.Zero;

                        float dot1 = -Vector3.Dot(lightDirection, normal);
                        if (dot1 < 0.0f)
                            return Vector3.Zero;
                        if (dot1 > 1.0f)
                            dot1 = 1.0f;

                        float finalIntensity = attenuation * dot1 * factor * light.Intensity * 8192.0f;
                        return finalIntensity * light.Color * (1.0f / 64.0f);
                    }
                    break;

            }

            return Vector3.Zero;
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
                        color += CalculateLightForVertex(room, light, position, normal, true);
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
