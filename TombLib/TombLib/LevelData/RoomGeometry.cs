using System;
using System.Collections.Generic;
using System.Numerics;
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
                        BlockFace.Floor, BlockFace.Floor_Triangle2, floorPortalInfo.VisualType);

                    // Ceiling polygons
                    int ceilingStartVertex = VertexPositions.Count;

                    Room.RoomConnectionInfo ceilingPortalInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z));
                    BuildFloorOrCeilingFace(room, x, z, Blocks[x, z].Ceiling.XnZp, Blocks[x, z].Ceiling.XpZp, Blocks[x, z].Ceiling.XpZn, Blocks[x, z].Ceiling.XnZn, Blocks[x, z].Ceiling.DiagonalSplit, Blocks[x, z].Ceiling.SplitDirectionIsXEqualsZ,
                        BlockFace.Ceiling, BlockFace.Ceiling_Triangle2, ceilingPortalInfo.VisualType);

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
            VertexColors.Resize(VertexPositions.Count, room.Properties.AmbientLight);

            // Lighting
            Relight(room);

            room.GenerateAttractors();
        }

        private enum FaceDirection
        {
            PositiveZ, NegativeZ, PositiveX, NegativeX, DiagonalFloor, DiagonalCeiling, DiagonalWall
        }

        private Direction FaceDirectionToDirection(FaceDirection faceDirection) => faceDirection switch
        {
            FaceDirection.PositiveZ => Direction.PositiveZ,
            FaceDirection.NegativeZ => Direction.NegativeZ,
            FaceDirection.PositiveX => Direction.PositiveX,
            FaceDirection.NegativeX => Direction.NegativeX,
            FaceDirection.DiagonalFloor or FaceDirection.DiagonalCeiling or FaceDirection.DiagonalWall => Direction.Diagonal,
            _ => Direction.None
        };

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
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                        }
                        
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                        }
                            
                        break;

                    case DiagonalSplit.XnZn:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                        }

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                        }

                        break;

                    case DiagonalSplit.XnZp:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                        }

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                        }

                        break;

                    case DiagonalSplit.XpZp:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                        }
                            

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                                room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                        }
                            
                        break;

                    default:
                        throw new NotSupportedException("Unknown FloorDiagonalSplit");
                }
            }
            else if (BlockSurface.IsQuad2(h0, h1, h2, h3) && portalMode == Room.RoomConnectionType.NoPortal)
            {
                AddQuad(x, z, face1,
                    new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                    new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                    new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                    new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                    room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            }
            else if (diagonalSplitXEqualsY || portalMode == Room.RoomConnectionType.TriangularPortalXnZp || portalMode == Room.RoomConnectionType.TriangularPortalXpZn)
            {
                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                {
                    AddTriangle(x, z, face2,
                        new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                        room.Blocks[x, z].GetFaceTexture(face2), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                }

                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                {
                    AddTriangle(x, z, face1,
                        new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                        room.Blocks[x, z].GetFaceTexture(face1), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                } 
            }
            else
            {
                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                {
                    AddTriangle(x, z, face1,
                        new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h1 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                        room.Blocks[x, z].GetFaceTexture(face1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                }

                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                {
                    AddTriangle(x, z, face2,
                        new Vector3((x + 1) * Level.BlockSizeUnit, h2 * Level.HeightUnit, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h3 * Level.HeightUnit, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h0 * Level.HeightUnit, (z + 1) * Level.BlockSizeUnit),
                        room.Blocks[x, z].GetFaceTexture(face2), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                }
            }
        }

        private void AddVerticalFaces(Room room, int x, int z, FaceDirection direction, bool hasFloorPart, bool hasCeilingPart, bool hasMiddlePart)
        {
            //                                                 *Walkable floor*
            //                            yQaA (Start of QA)  0################0  yQaB (Start of QA)
            //                                                #                #  
            //                                                #                #  
            //     yEndA (End of QA) / yStartA (Start of ED)  0##              #  
            //                                                #  ###           #  
            //                                                #     ###        #  
            //                                                #        ###     #  
            //                                                #           ###  #  
            //                                                #              ##0  yEndB (End of QA) / yStartB (Start of ED)
            // yEndA (End of ED) / yStartA (Start of Floor3)  0##              #  
            //                                                #  ###           #  
            //                                                #     ###        #  
            //                                                #        ###     #  
            //                                                #           ###  #  
            //                                                #              ##0  yEndB (End of ED) / yStartB (Start of Floor3)
            //                                                #                #  
            //                                                #                #  
            //                         yEndA (End of Floor3)  0################0  yEndB (End of Floor3)
            //
            //                                                   *Floor void*

            int xA, // X coordinate of the first vertex of the wall
                xB, // X coordinate of the second vertex of the wall
                zA, // Z coordinate of the first vertex of the wall
                zB, // Z coordinate of the second vertex of the wall
                yEndA, // First bottom limit (for floors) or top limit (for ceilings) of a face (Point A)
                yEndB; // Second bottom limit (for floors) or top limit (for ceilings) of a face (Point B)

            int yQaA, // First top limit of the QA part of the wall (Point A)
                yQaB, // Second top limit of the QA part of the wall (Point B)
                yWsA, // First bottom limit of the WS part of the wall (Point A)
                yWsB, // Second bottom limit of the WS part of the wall (Point B)
                yFloorA, // Height of the A point of the floor in front of the face
                yFloorB, // Height of the B point of the floor in front of the face
                yCeilingA, // Height of the A point of the ceiling in front of the face
                yCeilingB; // Height of the B point of the ceiling in front of the face

            Block block = room.Blocks[x, z];
            Block neighborBlock;
            TextureArea faceTexture;

            BlockFace qaFace, wsFace, middleFace;
            var floorSubdivisions = new List<(int A, int B)>();
            var ceilingSubdivisions = new List<(int A, int B)>();         

            switch (direction)
            {
                case FaceDirection.PositiveZ:
                    xA = x + 1;
                    xB = x;
                    zA = z + 1;
                    zB = z + 1;
                    neighborBlock = room.Blocks[x, z + 1];
                    yQaA = block.Floor.XpZp;
                    yQaB = block.Floor.XnZp;
                    yWsA = block.Ceiling.XpZp;
                    yWsB = block.Ceiling.XnZp;

                    for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                        floorSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));

                    for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                        ceilingSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));

                    yFloorA = neighborBlock.Floor.XpZn;
                    yFloorB = neighborBlock.Floor.XnZn;
                    yCeilingA = neighborBlock.Ceiling.XpZn;
                    yCeilingB = neighborBlock.Ceiling.XnZn;
                    qaFace = BlockFace.Wall_PositiveZ_QA;
                    middleFace = BlockFace.Wall_PositiveZ_Middle;
                    wsFace = BlockFace.Wall_PositiveZ_WS;

                    if (block.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = block.WallPortal;
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
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                            wsNearA = wsNearB;
                        if (nearBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
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

                        yQaA = room.Position.Y + qaNearA;
                        yQaB = room.Position.Y + qaNearB;
                        yQaA = Math.Max(yQaA, qAportal) - room.Position.Y;
                        yQaB = Math.Max(yQaB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wBportal = wAportal;

                        yWsA = room.Position.Y + wsNearA;
                        yWsB = room.Position.Y + wsNearB;
                        yWsA = Math.Min(yWsA, wAportal) - room.Position.Y;
                        yWsB = Math.Min(yWsB, wBportal) - room.Position.Y;

                        (int, int) newSubdivision;

                        for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp));

                            if (i >= floorSubdivisions.Count)
                                floorSubdivisions.Add(newSubdivision);
                            else
                                floorSubdivisions[i] = newSubdivision;
                        }       

                        for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp));

                            if (i >= ceilingSubdivisions.Count)
                                ceilingSubdivisions.Add(newSubdivision);
                            else
                                ceilingSubdivisions[i] = newSubdivision;
                        }        
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yQaA = block.Floor.XnZp;
                        yQaB = block.Floor.XnZp;
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yQaA = block.Floor.XpZp;
                        yQaB = block.Floor.XpZp;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yFloorA = neighborBlock.Floor.XpZn;
                        yFloorB = neighborBlock.Floor.XpZn;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yFloorA = neighborBlock.Floor.XnZn;
                        yFloorB = neighborBlock.Floor.XnZn;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yWsA = block.Ceiling.XnZp;
                        yWsB = block.Ceiling.XnZp;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yWsA = block.Ceiling.XpZp;
                        yWsB = block.Ceiling.XpZp;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yCeilingA = neighborBlock.Ceiling.XpZn;
                        yCeilingB = neighborBlock.Ceiling.XpZn;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yCeilingA = neighborBlock.Ceiling.XnZn;
                        yCeilingB = neighborBlock.Ceiling.XnZn;
                    }

                    break;

                case FaceDirection.NegativeZ:
                    xA = x;
                    xB = x + 1;
                    zA = z;
                    zB = z;
                    neighborBlock = room.Blocks[x, z - 1];
                    yQaA = block.Floor.XnZn;
                    yQaB = block.Floor.XpZn;
                    yWsA = block.Ceiling.XnZn;
                    yWsB = block.Ceiling.XpZn;

                    for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                        floorSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));

                    for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                        ceilingSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));

                    yFloorA = neighborBlock.Floor.XnZp;
                    yFloorB = neighborBlock.Floor.XpZp;
                    yCeilingA = neighborBlock.Ceiling.XnZp;
                    yCeilingB = neighborBlock.Ceiling.XpZp;
                    qaFace = BlockFace.Wall_NegativeZ_QA;
                    middleFace = BlockFace.Wall_NegativeZ_Middle;
                    wsFace = BlockFace.Wall_NegativeZ_WS;

                    if (block.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = block.WallPortal;
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

                        yQaA = room.Position.Y + qaNearA;
                        yQaB = room.Position.Y + qaNearB;
                        yQaA = Math.Max(yQaA, qAportal) - room.Position.Y;
                        yQaB = Math.Max(yQaB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                            wBportal = wAportal;

                        yWsA = room.Position.Y + wsNearA;
                        yWsB = room.Position.Y + wsNearB;
                        yWsA = Math.Min(yWsA, wAportal) - room.Position.Y;
                        yWsB = Math.Min(yWsB, wBportal) - room.Position.Y;

                        (int, int) newSubdivision;

                        for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn));

                            if (i >= floorSubdivisions.Count)
                                floorSubdivisions.Add(newSubdivision);
                            else
                                floorSubdivisions[i] = newSubdivision;
                        }  

                        for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn));

                            if (i >= ceilingSubdivisions.Count)
                                ceilingSubdivisions.Add(newSubdivision);
                            else
                                ceilingSubdivisions[i] = newSubdivision;	
                        }
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yQaA = block.Floor.XnZn;
                        yQaB = block.Floor.XnZn;
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yQaA = block.Floor.XpZn;
                        yQaB = block.Floor.XpZn;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yFloorA = neighborBlock.Floor.XnZp;
                        yFloorB = neighborBlock.Floor.XnZp;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yFloorA = neighborBlock.Floor.XpZp;
                        yFloorB = neighborBlock.Floor.XpZp;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yWsA = block.Ceiling.XnZn;
                        yWsB = block.Ceiling.XnZn;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yWsA = block.Ceiling.XpZn;
                        yWsB = block.Ceiling.XpZn;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yCeilingA = neighborBlock.Ceiling.XnZp;
                        yCeilingB = neighborBlock.Ceiling.XnZp;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yCeilingA = neighborBlock.Ceiling.XpZp;
                        yCeilingB = neighborBlock.Ceiling.XpZp;
                    }

                    break;

                case FaceDirection.PositiveX:
                    xA = x + 1;
                    xB = x + 1;
                    zA = z;
                    zB = z + 1;
                    neighborBlock = room.Blocks[x + 1, z];
                    yQaA = block.Floor.XpZn;
                    yQaB = block.Floor.XpZp;
                    yWsA = block.Ceiling.XpZn;
                    yWsB = block.Ceiling.XpZp;

                    for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                        floorSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));

                    for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                        ceilingSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));

                    yFloorA = neighborBlock.Floor.XnZn;
                    yFloorB = neighborBlock.Floor.XnZp;
                    yCeilingA = neighborBlock.Ceiling.XnZn;
                    yCeilingB = neighborBlock.Ceiling.XnZp;
                    qaFace = BlockFace.Wall_PositiveX_QA;
                    middleFace = BlockFace.Wall_PositiveX_Middle;
                    wsFace = BlockFace.Wall_PositiveX_WS;

                    if (block.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = block.WallPortal;
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

                        yQaA = room.Position.Y + qaNearA;
                        yQaB = room.Position.Y + qaNearB;
                        yQaA = Math.Max(yQaA, qAportal) - room.Position.Y;
                        yQaB = Math.Max(yQaB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                            wBportal = wAportal;

                        yWsA = room.Position.Y + wsNearA;
                        yWsB = room.Position.Y + wsNearB;
                        yWsA = Math.Min(yWsA, wAportal) - room.Position.Y;
                        yWsB = Math.Min(yWsB, wBportal) - room.Position.Y;

                        (int, int) newSubdivision;

                        for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp));

                            if (i >= floorSubdivisions.Count)
                                floorSubdivisions.Add(newSubdivision);
                            else
                                floorSubdivisions[i] = newSubdivision;
                        }	

                        for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp));

                            if (i >= ceilingSubdivisions.Count)
                                ceilingSubdivisions.Add(newSubdivision);
                            else
                                ceilingSubdivisions[i] = newSubdivision;
                        }	
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yQaA = block.Floor.XpZp;
                        yQaB = block.Floor.XpZp;
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yQaA = block.Floor.XpZn;
                        yQaB = block.Floor.XpZn;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yFloorA = neighborBlock.Floor.XnZp;
                        yFloorB = neighborBlock.Floor.XnZp;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yFloorA = neighborBlock.Floor.XnZn;
                        yFloorB = neighborBlock.Floor.XnZn;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yWsA = block.Ceiling.XpZp;
                        yWsB = block.Ceiling.XpZp;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yWsA = block.Ceiling.XpZn;
                        yWsB = block.Ceiling.XpZn;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yCeilingA = neighborBlock.Ceiling.XnZp;
                        yCeilingB = neighborBlock.Ceiling.XnZp;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yCeilingA = neighborBlock.Ceiling.XnZn;
                        yCeilingB = neighborBlock.Ceiling.XnZn;
                    }

                    break;

                case FaceDirection.DiagonalFloor:
                    switch (block.Floor.DiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            yQaA = block.Floor.XpZp;
                            yQaB = block.Floor.XnZn;
                            yWsA = block.Ceiling.XpZp;
                            yWsB = block.Ceiling.XnZn;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));

                            yFloorA = block.Floor.XnZp;
                            yFloorB = block.Floor.XnZp;
                            yCeilingA = block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XpZp;
                            yCeilingB = block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XnZn;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                        case DiagonalSplit.XnZn:
                            xA = x + 1;
                            xB = x;
                            zA = z;
                            zB = z + 1;
                            yQaA = block.Floor.XpZn;
                            yQaB = block.Floor.XnZp;
                            yWsA = block.Ceiling.XpZn;
                            yWsB = block.Ceiling.XnZp;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));

                            yFloorA = block.Floor.XpZp;
                            yFloorB = block.Floor.XpZp;
                            yCeilingA = block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XpZn;
                            yCeilingB = block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XnZp;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                        case DiagonalSplit.XnZp:
                            xA = x;
                            xB = x + 1;
                            zA = z;
                            zB = z + 1;
                            yQaA = block.Floor.XnZn;
                            yQaB = block.Floor.XpZp;
                            yWsA = block.Ceiling.XnZn;
                            yWsB = block.Ceiling.XpZp;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));

                            yFloorA = block.Floor.XpZn;
                            yFloorB = block.Floor.XpZn;
                            yCeilingA = block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XnZn;
                            yCeilingB = block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XpZp;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                        default:
                            xA = x;
                            xB = x + 1;
                            zA = z + 1;
                            zB = z;
                            yQaA = block.Floor.XnZp;
                            yQaB = block.Floor.XpZn;
                            yWsA = block.Ceiling.XnZp;
                            yWsB = block.Ceiling.XpZn;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));

                            yFloorA = block.Floor.XnZn;
                            yFloorB = block.Floor.XnZn;
                            yCeilingA = block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XnZp;
                            yCeilingB = block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XpZn;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                    }

                    break;

                case FaceDirection.DiagonalCeiling:
                    switch (block.Ceiling.DiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            xA = x + 1;
                            xB = x;
                            zA = z + 1;
                            zB = z;
                            yQaA = block.Floor.XpZp;
                            yQaB = block.Floor.XnZn;
                            yWsA = block.Ceiling.XpZp;
                            yWsB = block.Ceiling.XnZn;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));

                            yFloorA = block.IsAnyWall ? block.Floor.XnZp : block.Floor.XpZp;
                            yFloorB = block.IsAnyWall ? block.Floor.XnZp : block.Floor.XnZn;
                            yCeilingA = block.Ceiling.XnZp;
                            yCeilingB = block.Ceiling.XnZp;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                        case DiagonalSplit.XnZn:
                            xA = x + 1;
                            xB = x;
                            zA = z;
                            zB = z + 1;
                            yQaA = block.Floor.XpZn;
                            yQaB = block.Floor.XnZp;
                            yWsA = block.Ceiling.XpZn;
                            yWsB = block.Ceiling.XnZp;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));

                            yFloorA = block.IsAnyWall ? block.Floor.XpZp : block.Floor.XpZn;
                            yFloorB = block.IsAnyWall ? block.Floor.XpZp : block.Floor.XnZp;
                            yCeilingA = block.Ceiling.XpZp;
                            yCeilingB = block.Ceiling.XpZp;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                        case DiagonalSplit.XnZp:
                            xA = x;
                            xB = x + 1;
                            zA = z;
                            zB = z + 1;
                            yQaA = block.Floor.XnZn;
                            yQaB = block.Floor.XpZp;
                            yWsA = block.Ceiling.XnZn;
                            yWsB = block.Ceiling.XpZp;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));

                            yFloorA = block.IsAnyWall ? block.Floor.XpZn : block.Floor.XnZn;
                            yFloorB = block.IsAnyWall ? block.Floor.XpZn : block.Floor.XpZp;
                            yCeilingA = block.Ceiling.XpZn;
                            yCeilingB = block.Ceiling.XpZn;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                        default:
                            xA = x;
                            xB = x + 1;
                            zA = z + 1;
                            zB = z;
                            yQaA = block.Floor.XnZp;
                            yQaB = block.Floor.XpZn;
                            yWsA = block.Ceiling.XnZp;
                            yWsB = block.Ceiling.XpZn;

                            for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                                floorSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));

                            for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                                ceilingSubdivisions.Add((
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
                                    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));

                            yFloorA = block.IsAnyWall ? block.Floor.XnZn : block.Floor.XnZp;
                            yFloorB = block.IsAnyWall ? block.Floor.XnZn : block.Floor.XpZn;
                            yCeilingA = block.Ceiling.XnZn;
                            yCeilingB = block.Ceiling.XnZn;
                            qaFace = BlockFace.Wall_Diagonal_QA;
                            middleFace = BlockFace.Wall_Diagonal_Middle;
                            wsFace = BlockFace.Wall_Diagonal_WS;
                            break;
                    }

                    break;

                default:
                    xA = x;
                    xB = x;
                    zA = z + 1;
                    zB = z;
                    neighborBlock = room.Blocks[x - 1, z];
                    yQaA = block.Floor.XnZp;
                    yQaB = block.Floor.XnZn;
                    yWsA = block.Ceiling.XnZp;
                    yWsB = block.Ceiling.XnZn;

                    for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
                        floorSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
                            block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));

                    for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
                        ceilingSubdivisions.Add((
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
                            block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));

                    yFloorA = neighborBlock.Floor.XpZp;
                    yFloorB = neighborBlock.Floor.XpZn;
                    yCeilingA = neighborBlock.Ceiling.XpZp;
                    yCeilingB = neighborBlock.Ceiling.XpZn;
                    qaFace = BlockFace.Wall_NegativeX_QA;
                    middleFace = BlockFace.Wall_NegativeX_Middle;
                    wsFace = BlockFace.Wall_NegativeX_WS;

                    if (block.WallPortal != null)
                    {
                        // Get the adjoining room of the portal
                        var portal = block.WallPortal;
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

                        yQaA = room.Position.Y + qaNearA;
                        yQaB = room.Position.Y + qaNearB;
                        yQaA = Math.Max(yQaA, qAportal) - room.Position.Y;
                        yQaB = Math.Max(yQaB, qBportal) - room.Position.Y;

                        int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
                        int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                            wAportal = wBportal;
                        if (adjoiningBlock.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                            wBportal = wAportal;

                        yWsA = room.Position.Y + wsNearA;
                        yWsB = room.Position.Y + wsNearB;
                        yWsA = Math.Min(yWsA, wAportal) - room.Position.Y;
                        yWsB = Math.Min(yWsB, wBportal) - room.Position.Y;

                        (int, int) newSubdivision;

                        for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn));

                            if (i >= floorSubdivisions.Count)
                                floorSubdivisions.Add(newSubdivision);
                            else
                                floorSubdivisions[i] = newSubdivision;
                        }
                        
                        for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
                        {
                            newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
                                adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn));

                            if (i >= ceilingSubdivisions.Count)
                                ceilingSubdivisions.Add(newSubdivision);
                            else
                                ceilingSubdivisions[i] = newSubdivision;
                        }
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yQaA = block.Floor.XnZp;
                        yQaB = block.Floor.XnZp;
                    }

                    if (block.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yQaA = block.Floor.XnZn;
                        yQaB = block.Floor.XnZn;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yFloorA = neighborBlock.Floor.XpZp;
                        yFloorB = neighborBlock.Floor.XpZp;
                    }

                    if (neighborBlock.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yFloorA = neighborBlock.Floor.XpZn;
                        yFloorB = neighborBlock.Floor.XpZn;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XpZn)
                    {
                        yWsA = block.Ceiling.XnZp;
                        yWsB = block.Ceiling.XnZp;
                    }

                    if (block.Ceiling.DiagonalSplit == DiagonalSplit.XpZp)
                    {
                        yWsA = block.Ceiling.XnZn;
                        yWsB = block.Ceiling.XnZn;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZn)
                    {
                        yCeilingA = neighborBlock.Ceiling.XpZp;
                        yCeilingB = neighborBlock.Ceiling.XpZp;
                    }

                    if (neighborBlock.Ceiling.DiagonalSplit == DiagonalSplit.XnZp)
                    {
                        yCeilingA = neighborBlock.Ceiling.XpZn;
                        yCeilingB = neighborBlock.Ceiling.XpZn;
                    }

                    break;
            }

            if (hasFloorPart)
            {
                bool isQaFullyAboveCeiling = yQaA >= yCeilingA && yQaB >= yCeilingB; // Technically should be classified as a wall if true

                bool isDiagonalWallFloorPart = // The wall bit under the flat floor triangle of a diagonal wall
                    (block.Floor.DiagonalSplit == DiagonalSplit.XnZp && direction is FaceDirection.NegativeZ or FaceDirection.PositiveX) ||
                    (block.Floor.DiagonalSplit == DiagonalSplit.XpZn && direction is FaceDirection.NegativeX or FaceDirection.PositiveZ) ||
                    (block.Floor.DiagonalSplit == DiagonalSplit.XpZp && direction is FaceDirection.NegativeZ or FaceDirection.NegativeX) ||
                    (block.Floor.DiagonalSplit == DiagonalSplit.XnZn && direction is FaceDirection.PositiveZ or FaceDirection.PositiveX);

                bool TryRenderFace(BlockFace face, (int A, int B) yStart, (int A, int B) yEnd)
                {
                    TextureArea texture = block.GetFaceTexture(face);

                    if (yStart.A > yEnd.A && yStart.B > yEnd.B) // Is quad
                        AddQuad(x, z, face,
                            new Vector3(xA * Level.BlockSizeUnit, yStart.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yStart.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yEnd.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xA * Level.BlockSizeUnit, yEnd.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                    else if (yStart.A == yEnd.A && yStart.B > yEnd.B) // Is triangle (type 1)
                        AddTriangle(x, z, face,
                            new Vector3(xA * Level.BlockSizeUnit, yEnd.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yStart.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yEnd.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            texture, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
                    else if (yStart.A > yEnd.A && yStart.B == yEnd.B)  // Is triangle (type 2)
                        AddTriangle(x, z, face,
                            new Vector3(xA * Level.BlockSizeUnit, yStart.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yEnd.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xA * Level.BlockSizeUnit, yEnd.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                    else
                        return false; // Not rendered - failed to meet any of the conditions

                    return true; // Rendered successfully
                }

                GeometryRenderResult TryRenderFloorWallGeometry(BlockFace face, ref int yStartA, ref int yStartB, int extraSubdivisionIndex = -1)
                {
                    bool isFaceInFloorVoid = yStartA < yFloorA || yStartB < yFloorB || (yStartA == yFloorA && yStartB == yFloorB);

                    if (isFaceInFloorVoid && block.IsAnyWall && !isDiagonalWallFloorPart) // Part of overdraw prevention
                        return GeometryRenderResult.Stop; // Stop the loop, since the rest of the subdivisions will also be in the void

                    bool isEitherStartPointAboveCeiling = yStartA > yCeilingA || yStartB > yCeilingB; // If either start point A or B is in the void above ceiling
                    bool areBothStartPointsAboveCeiling = yStartA >= yCeilingA && yStartB >= yCeilingB; // Are both start points A and B in the void above ceiling

                    // Walls can't have overdraw, so if either point is in void, then snap it to ceiling
                    // Diagonal walls are an exception, since, even though they are walls, they have a flat floor bit, so we can allow overdraw
                    if ((isEitherStartPointAboveCeiling && (block.IsAnyWall || isQaFullyAboveCeiling) && !isDiagonalWallFloorPart) || areBothStartPointsAboveCeiling)
                    {
                        // Snap points to ceiling
                        yStartA = yCeilingA;
                        yStartB = yCeilingB;
                    }

                    // If the face is a portal or a diagonal wall's floor part (below the flat, walkable triangle)
                    // and either subdivision point is above the lowest flat triangle point
                    if ((block.IsAnyPortal || isDiagonalWallFloorPart) && (yStartA > yQaA || yStartB > yQaB))
                    {
                        // Snap points to the heights of the flat, walkable triangle
                        yStartA = yQaA;
                        yStartB = yQaB;
                    }

                    // Start with the floor as a baseline for the bottom end of the face
                    (int yEndA, int yEndB) = (yFloorA, yFloorB);

                    if (extraSubdivisionIndex + 1 < floorSubdivisions.Count) // If a next floor subdivision exists
                    {
                        int yNextSubdivA = floorSubdivisions[extraSubdivisionIndex + 1].A,
                            yNextSubdivB = floorSubdivisions[extraSubdivisionIndex + 1].B;

                        if ((isDiagonalWallFloorPart || isQaFullyAboveCeiling) && (yNextSubdivA > yQaA || yNextSubdivB > yQaB))
                            return GeometryRenderResult.Skip; // Skip it, since it's above the flat, walkable triangle

                        if (yNextSubdivA >= yFloorA && yNextSubdivB >= yFloorB) // If next subdivision is NOT in void below floor
                        {
                            // Make the next subdivision the bottom end of the face
                            yEndA = yNextSubdivA;
                            yEndB = yNextSubdivB;
                        }
                    }

                    if (yStartA <= yEndA && yStartB <= yEndB)
                        return GeometryRenderResult.Skip; // 0 or negative height subdivision, don't render it

                    bool success = TryRenderFace(face, (yStartA, yStartB), (yEndA, yEndB));

                    if (!success)
                    {
                        // Try overdraw

                        bool isQA = yStartA == yQaA && yStartB == yQaB;
                        bool isValidOverdraw = isQA && (block.Type == BlockType.Floor || isDiagonalWallFloorPart);

                        if (!isValidOverdraw)
                            return GeometryRenderResult.Skip;

                        // Find lowest point between subdivision and baseline, then try and create an overdraw face out of it
                        int lowest = Math.Min(Math.Min(yStartA, yStartB), Math.Min(yEndA, yEndB));
                        success = TryRenderFace(face, (yStartA, yStartB), (lowest, lowest));
                    }

                    return success ? GeometryRenderResult.Success : GeometryRenderResult.Skip;
                }

                // Render QA face
                GeometryRenderResult renderResult = TryRenderFloorWallGeometry(qaFace, ref yQaA, ref yQaB);

                if (renderResult != GeometryRenderResult.Stop)
                {
                    // Render subdivision faces
                    for (int i = 0; i < floorSubdivisions.Count; i++)
                    {
                        (int a, int b) = floorSubdivisions[i];
                        BlockFace currentFace = BlockFaceExtensions.GetExtraFloorSubdivisionFace(FaceDirectionToDirection(direction), i);
                        renderResult = TryRenderFloorWallGeometry(currentFace, ref a, ref b, i);

                        if (renderResult == GeometryRenderResult.Stop)
                            break; 
                    }
                }
            }

            if (hasCeilingPart) 
            {
                bool isWsFullyAboveCeiling = yWsA <= yFloorA && yWsB <= yFloorB; // Technically should be classified as a wall if true

                bool isDiagonalWallCeilingPart = // The wall bit over the flat ceiling triangle of a diagonal wall
                    (block.Ceiling.DiagonalSplit == DiagonalSplit.XnZp && direction is FaceDirection.NegativeZ or FaceDirection.PositiveX) ||
                    (block.Ceiling.DiagonalSplit == DiagonalSplit.XpZn && direction is FaceDirection.NegativeX or FaceDirection.PositiveZ) ||
                    (block.Ceiling.DiagonalSplit == DiagonalSplit.XpZp && direction is FaceDirection.NegativeZ or FaceDirection.NegativeX) ||
                    (block.Ceiling.DiagonalSplit == DiagonalSplit.XnZn && direction is FaceDirection.PositiveZ or FaceDirection.PositiveX);

                bool TryRenderFace(BlockFace face, (int A, int B) yStart, (int A, int B) yEnd)
                {
                    TextureArea texture = block.GetFaceTexture(face);

                    if (yStart.A < yEnd.A && yStart.B < yEnd.B)
                        AddQuad(x, z, face,
                            new Vector3(xA * Level.BlockSizeUnit, yEnd.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yEnd.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yStart.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xA * Level.BlockSizeUnit, yStart.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                    else if (yStart.A < yEnd.A && yStart.B == yEnd.B)
                        AddTriangle(x, z, face,
                            new Vector3(xA * Level.BlockSizeUnit, yEnd.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yEnd.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xA * Level.BlockSizeUnit, yStart.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                    else if (yStart.A == yEnd.A && yStart.B < yEnd.B)
                        AddTriangle(x, z, face,
                            new Vector3(xA * Level.BlockSizeUnit, yEnd.A * Level.HeightUnit, zA * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yEnd.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            new Vector3(xB * Level.BlockSizeUnit, yStart.B * Level.HeightUnit, zB * Level.BlockSizeUnit),
                            texture, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
                    else
                        return false; // Not rendered - failed to meet any of the conditions

                    return true; // Rendered successfully
                }

                GeometryRenderResult TryRenderCeilingWallGeometry(BlockFace face, ref int yStartA, ref int yStartB, int extraSubdivisionIndex = -1)
                {
                    bool isFaceInCeilingVoid = yStartA > yCeilingA || yStartB > yCeilingB || (yStartA == yCeilingA && yStartB == yCeilingB);

                    if (isFaceInCeilingVoid && block.IsAnyWall && !isDiagonalWallCeilingPart) // Part of overdraw prevention
                        return GeometryRenderResult.Stop; // Stop the loop, since the rest of the subdivisions will also be in the void

                    bool isEitherStartPointBelowFloor = yStartA < yFloorA || yStartB < yFloorB; // If either start point A or B is in the void below floor
                    bool areBothStartPointsBelowFloor = yStartA <= yFloorA && yStartB <= yFloorB; // Are both start points A and B in the void below floor

                    // Walls can't have overdraw, so if either point is in void, then snap it to floor
                    // Diagonal walls are an exception, since, even though they are walls, they have a flat ceiling bit, so we can allow overdraw
                    if ((isEitherStartPointBelowFloor && (block.IsAnyWall || isWsFullyAboveCeiling) && !isDiagonalWallCeilingPart) || areBothStartPointsBelowFloor)
                    {
                        // Snap points to floor
                        yStartA = yFloorA;
                        yStartB = yFloorB;
                    }

                    // If the face is a portal or a diagonal wall's ceiling part (above the flat ceiling triangle)
                    // and either subdivision point is below the highest flat triangle point
                    if ((block.IsAnyPortal || isDiagonalWallCeilingPart) && (yStartA < yWsA || yStartB < yWsB))
                    {
                        // Snap points to the heights of the flat ceiling triangle
                        yStartA = yWsA;
                        yStartB = yWsB;
                    }

                    // Start with the ceiling as a baseline for the top end of the face
                    (int yEndA, int yEndB) = (yCeilingA, yCeilingB);

                    if (extraSubdivisionIndex + 1 < ceilingSubdivisions.Count) // If a next ceiling subdivision exists
                    {
                        int yNextSubdivA = ceilingSubdivisions[extraSubdivisionIndex + 1].A,
                            yNextSubdivB = ceilingSubdivisions[extraSubdivisionIndex + 1].B;

                        if ((isDiagonalWallCeilingPart || isWsFullyAboveCeiling) && (yNextSubdivA < yWsA || yNextSubdivB < yWsB))
                            return GeometryRenderResult.Skip; // Skip it, since it's below the flat ceiling triangle

                        if (yNextSubdivA <= yCeilingA && yNextSubdivB <= yCeilingB) // If next subdivision is NOT in void above ceiling
                        {
                            // Make the next subdivision the top end of the face
                            yEndA = yNextSubdivA;
                            yEndB = yNextSubdivB;
                        }
                    }

                    if (yStartA >= yEndA && yStartB >= yEndB)
                        return GeometryRenderResult.Skip; // 0 or negative height subdivision, don't render it

                    bool success = TryRenderFace(face, (yStartA, yStartB), (yEndA, yEndB));

                    if (!success)
                    {
                        // Try overdraw

                        bool isWS = yStartA == yWsA && yStartB == yWsB;
                        bool isValidOverdraw = isWS && (block.Type == BlockType.Floor || isDiagonalWallCeilingPart);

                        if (!isValidOverdraw)
                            return GeometryRenderResult.Skip;

                        // Find highest point between subdivision and baseline, then try and create an overdraw face out of it
                        int highest = Math.Max(Math.Max(yStartA, yStartB), Math.Max(yEndA, yEndB));
                        success = TryRenderFace(face, (yStartA, yStartB), (highest, highest));
                    }

                    return success ? GeometryRenderResult.Success : GeometryRenderResult.Skip;
                }

                // Render WS face
                GeometryRenderResult renderResult = TryRenderCeilingWallGeometry(wsFace, ref yWsA, ref yWsB);

                if (renderResult != GeometryRenderResult.Stop)
                {
                    // Render subdivision faces
                    for (int i = 0; i < ceilingSubdivisions.Count; i++)
                    {
                        (int a, int b) = ceilingSubdivisions[i];
                        BlockFace currentFace = BlockFaceExtensions.GetExtraCeilingSubdivisionFace(FaceDirectionToDirection(direction), i);
                        renderResult = TryRenderCeilingWallGeometry(currentFace, ref a, ref b, i);

                        if (renderResult == GeometryRenderResult.Stop)
                            break;
                    }
                }
            }

            if (!hasMiddlePart)
                return;

            faceTexture = block.GetFaceTexture(middleFace);

            if (yQaA < yFloorA || yQaB < yFloorB)
            {
                yQaA = yFloorA;
                yQaB = yFloorB;
            }

            if (yWsA > yCeilingA || yWsB > yCeilingB)
            {
                yWsA = yCeilingA;
                yWsB = yCeilingB;
            }

            yEndA = yWsA >= yCeilingA ? yCeilingA : yWsA;
            yEndB = yWsB >= yCeilingB ? yCeilingB : yWsB;
            int yD = yQaA <= yFloorA ? yFloorA : yQaA;
            int yC = yQaB <= yFloorB ? yFloorB : yQaB;

            // Middle
            if (yEndA != yD && yEndB != yC)
                AddQuad(x, z, middleFace,
                    new Vector3(xA * Level.BlockSizeUnit, yEndA * Level.HeightUnit, zA * Level.BlockSizeUnit),
                    new Vector3(xB * Level.BlockSizeUnit, yEndB * Level.HeightUnit, zB * Level.BlockSizeUnit),
                    new Vector3(xB * Level.BlockSizeUnit, yC * Level.HeightUnit, zB * Level.BlockSizeUnit),
                    new Vector3(xA * Level.BlockSizeUnit, yD * Level.HeightUnit, zA * Level.BlockSizeUnit),
                    faceTexture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            else if (yEndA != yD && yEndB == yC)
                AddTriangle(x, z, middleFace,
                    new Vector3(xA * Level.BlockSizeUnit, yEndA * Level.HeightUnit, zA * Level.BlockSizeUnit),
                    new Vector3(xB * Level.BlockSizeUnit, yEndB * Level.HeightUnit, zB * Level.BlockSizeUnit),
                    new Vector3(xA * Level.BlockSizeUnit, yD * Level.HeightUnit, zA * Level.BlockSizeUnit),
                    faceTexture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
            else if (yEndA == yD && yEndB != yC)
                AddTriangle(x, z, middleFace,
                    new Vector3(xA * Level.BlockSizeUnit, yEndA * Level.HeightUnit, zA * Level.BlockSizeUnit),
                    new Vector3(xB * Level.BlockSizeUnit, yEndB * Level.HeightUnit, zB * Level.BlockSizeUnit),
                    new Vector3(xB * Level.BlockSizeUnit, yC * Level.HeightUnit, zB * Level.BlockSizeUnit),
                    faceTexture, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
            else
                return;
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
            int currentX = x / (int)Level.BlockSizeUnit - (x > xLight ? 1 : 0);
            int currentZ = z / (int)Level.BlockSizeUnit - (z > zLight ? 1 : 0);

            if (currentX < 0 || currentX >= room.NumXSectors ||
                currentZ < 0 || currentZ >= room.NumZSectors)
                return false;

            Block block = room.Blocks[currentX, currentZ];
            int floorMin = block.Floor.Min;
            int ceilingMax = block.Ceiling.Max;

            return floorMin <= y / (int)Level.HeightUnit && ceilingMax >= y / (int)Level.HeightUnit;
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
                int currentXblock = currentX / (int)Level.BlockSizeUnit;
                int currentZblock = currentZ / (int)Level.BlockSizeUnit;

                if (currentZblock < 0 || currentXblock >= room.NumXSectors || currentZblock >= room.NumZSectors)
                {
                    if (currentX == maxX)
                        return true;
                }
                else
                {
                    int currentYclick = currentY / -(int)Level.HeightUnit;

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

                currentX += (int)Level.BlockSizeUnit;
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
                int currentXblock = currentX / (int)Level.BlockSizeUnit;
                int currentZblock = currentZ / (int)Level.BlockSizeUnit;

                if (currentXblock < 0 || currentZblock >= room.NumZSectors || currentXblock >= room.NumXSectors)
                {
                    if (currentZ == maxZ)
                        return true;
                }
                else
                {
                    int currentYclick = currentY / -(int)Level.HeightUnit;

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

                currentZ += (int)Level.BlockSizeUnit;
                currentX += (deltaX << 10) / (deltaZ + 1);
                currentY += (deltaY << 10) / (deltaZ + 1);
            }
            while (currentZ <= maxZ);

            return true;
        }

        private static bool LightRayTrace(Room room, Vector3 position, Vector3 lightPosition)
        {
            return !(
            RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)lightPosition.X, (int)lightPosition.Z) &&
            RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)lightPosition.X, (int)lightPosition.Y, (int)lightPosition.Z) &&
            RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)lightPosition.X, (int)lightPosition.Y, (int)lightPosition.Z));
        }

        private static int GetLightSampleCount(LightInstance light, LightQuality defaultQuality = LightQuality.Low)
        {
            int numSamples = 1;
            LightQuality finalQuality = light.Quality == LightQuality.Default ? defaultQuality : light.Quality;

            switch (finalQuality)
            {
                case LightQuality.Low:
                    numSamples = 1;
                    break;
                case LightQuality.Medium:
                    numSamples = 3;
                    break;
                case LightQuality.High:
                    numSamples = 5;
                    break;
            }
            return numSamples;
        }

        private static float GetSampleSumFromLightTracing(int numSamples, Room room, Vector3 position, LightInstance light)
        {
//             object lockingObject = new object();
//             float sampleSum = 0;
//             Parallel.For((int)(-numSamples / 2.0f), (int)(numSamples / 2.0f) + 1, (x) =>
//             {
//                 Parallel.For((int)(-numSamples / 2.0f), (int)(numSamples / 2.0f) + 1, (y) =>
//                 {
//                     Parallel.For((int)(-numSamples / 2.0f), (int)(numSamples / 2.0f) + 1, (z) =>
//                     {
//                         Vector3 samplePos = new Vector3(x * 256, y * 256, z * 256);
//                         if (!LightRayTrace(room, position, light.Position + samplePos))
//                             lock (lockingObject)
//                             {
//                                 sampleSum += 1.0f;
//                             }
//                     });
//                 });
//             });
            if (numSamples == 1) {
                if (light.IsObstructedByRoomGeometry) {
                    if (!LightRayTrace(room, position, light.Position))
                        return 1;
                    else
                        return 0;
                }
            }
            float sampleSum = 0.0f;
            for (int x = (int)(-numSamples / 2.0f); x <= (int)(numSamples / 2.0f); x++)
                for (int y = 0; y <= 0; y++)
                    for (int z = (int)(-numSamples / 2.0f); z <= (int)(numSamples / 2.0f); z++)
                    {
                        Vector3 samplePos = new Vector3(x * 256, y * 256, z * 256);
                        if (light.IsObstructedByRoomGeometry)
                        {
                            if (!LightRayTrace(room, position, light.Position + samplePos))
                                sampleSum += 1.0f;
                        }
                        else
                            sampleSum += 1.0f;

                    }
            sampleSum /= (numSamples * 1 * numSamples);
            return sampleSum;
        }

        public static float GetRaytraceResult(Room room, LightInstance light, Vector3 position, bool highQuality)
        {
            float result = 1.0f;
            if (light.Type != LightType.Effect && light.Type != LightType.FogBulb)
            {
                int numSamples;
                if (highQuality)
                    numSamples = GetLightSampleCount(light, room.Level.Settings.DefaultLightQuality);
                else
                    numSamples = 1;
                result = GetSampleSumFromLightTracing(numSamples, room, position, light);

                if (result < 0.000001f)
                    return 0;
            }
            return result;
        }

        public static Vector3 CalculateLightForVertex(Room room, LightInstance light, Vector3 position, 
                                                      Vector3 normal, bool legacyPointLightModel, bool highQuality)
        {
            if (!light.Enabled)
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

                    if (distance + 64.0f <= light.OuterRange * Level.BlockSizeUnit)
                    {     
                        // If distance is greater than light out radius, then skip this light
                        if (distance > light.OuterRange * Level.BlockSizeUnit)
                            return Vector3.Zero;

                        // Calculate light diffuse value
                        int diffuse = (int)(light.Intensity * 8192);

                        // Calculate the attenuation
                        float attenuaton = (light.OuterRange * Level.BlockSizeUnit - distance) / (light.OuterRange * Level.BlockSizeUnit - light.InnerRange * Level.BlockSizeUnit);
                        if (attenuaton > 1.0f)
                            attenuaton = 1.0f;
                        if (attenuaton <= 0.0f)
                            return Vector3.Zero;

                        // Calculate the length squared of the normal vector
                        float dotN = Vector3.Dot((legacyPointLightModel ? normal : -lightVector), normal);
                        if (dotN <= 0)
                            return Vector3.Zero;

                        // Get raytrace result
                        var raytraceResult = GetRaytraceResult(room, light, position, highQuality);

                        // Calculate final light color
                        float diffuseIntensity = dotN * attenuaton * raytraceResult;
                        diffuseIntensity = Math.Max(0, diffuseIntensity);
                        float finalIntensity = diffuseIntensity * diffuse;
                        return finalIntensity * light.Color * (1.0f / 64.0f);
                    }
                    break;

                case LightType.Effect:
                    if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * Level.BlockSizeUnit)
                    {
                        int x1 = (int)(Math.Floor(light.Position.X / Level.BlockSizeUnit)   * Level.BlockSizeUnit);
                        int z1 = (int)(Math.Floor(light.Position.Z / Level.BlockSizeUnit)   * Level.BlockSizeUnit);
                        int x2 = (int)(Math.Ceiling(light.Position.X / Level.BlockSizeUnit) * Level.BlockSizeUnit);
                        int z2 = (int)(Math.Ceiling(light.Position.Z / Level.BlockSizeUnit) * Level.BlockSizeUnit);

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
                        // Calculate the light direction
                        lightDirection = light.GetDirection();

                        // Calculate diffuse lighting
                        float diffuse = -Vector3.Dot(lightDirection, normal);
                        if (diffuse <= 0)
                            return Vector3.Zero;
                        if (diffuse > 1.0f)
                            diffuse = 1.0f;

                        // Get raytrace result
                        var raytraceResult = GetRaytraceResult(room, light, position, highQuality);

                        // Calculate final light color
                        float finalIntensity = diffuse * light.Intensity * 8192.0f * raytraceResult;
                        if (finalIntensity < 0)
                            return Vector3.Zero;
                        return finalIntensity * light.Color * (1.0f / 64.0f);
                    }

                case LightType.Spot:
                    if (Math.Abs(Vector3.Distance(position, light.Position)) + 64.0f <= light.OuterRange * Level.BlockSizeUnit)
                    {
                        // Calculate the ray from light to vertex
                        lightVector = Vector3.Normalize(position - light.Position);

                        // Get the distance between light and vertex
                        distance = Math.Abs((position - light.Position).Length());

                        // If distance is greater than light length, then skip this light
                        if (distance > light.OuterRange * Level.BlockSizeUnit)
                            return Vector3.Zero;

                        // Calculate the light direction
                        lightDirection = light.GetDirection();

                        // Calculate the cosines values for In, Out
                        double d = Vector3.Dot(lightVector, lightDirection);
                        double cosI2 = Math.Cos(light.InnerAngle * (Math.PI / 180));
                        double cosO2 = Math.Cos(light.OuterAngle * (Math.PI / 180));

                        if (d < cosO2)
                            return Vector3.Zero;

                        // Calculate light diffuse value
                        float factor = (float)(1.0f - (d - cosI2) / (cosO2 - cosI2));
                        if (factor > 1.0f)
                            factor = 1.0f;
                        if (factor <= 0.0f)
                            return Vector3.Zero;

                        float attenuation = 1.0f;
                        if (distance >= light.InnerRange * Level.BlockSizeUnit)
                            attenuation = 1.0f - (distance - light.InnerRange * Level.BlockSizeUnit) / (light.OuterRange * Level.BlockSizeUnit - light.InnerRange * Level.BlockSizeUnit);

                        if (attenuation > 1.0f)
                            attenuation = 1.0f;
                        if (attenuation < 0.0f)
                            return Vector3.Zero;

                        float dot1 = -Vector3.Dot(lightDirection, normal);
                        if (dot1 < 0.0f)
                            return Vector3.Zero;
                        if (dot1 > 1.0f)
                            dot1 = 1.0f;

                        // Get raytrace result
                        var raytraceResult = GetRaytraceResult(room, light, position, highQuality);

                        float finalIntensity = attenuation * dot1 * factor * light.Intensity * 8192.0f * raytraceResult;
                        return finalIntensity * light.Color * (1.0f / 64.0f);
                    }
                    break;
            }

            return Vector3.Zero;
        }

        public void Relight(Room room, bool highQuality = false)
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
                    Vector3 color = room.Properties.AmbientLight * 128;

                    foreach (var light in lights) // No Linq here because it's slow
                    {
                        if (light.IsStaticallyUsed)
                            color += CalculateLightForVertex(room, light, position, normal, true, highQuality);
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
                    if (Collision.RayIntersectsTriangle(ray, p0, p1, p2, true, out position))
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

        public SectorInfo(int x, int z, BlockFace face)
        {
            Pos = new VectorInt2(x, z);
            Face = face;
        }

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

    public enum GeometryRenderResult
    {
        Success,
        Skip,
        Stop
    }
}
