using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
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

        // useLegacyCode is used for converting legacy .PRJ files to .PRJ2 files
        public void Build(Room room, bool useLegacyCode = false)
        {
            VertexPositions.Clear();
            VertexEditorUVs.Clear();
            VertexColors.Clear();
            TriangleTextureAreas.Clear();
            TriangleSectorInfo.Clear();
            SharedVertices.Clear();
            VertexRangeLookup.Clear();
            DoubleSidedTriangleCount = 0;

            const int xMin = 0;
            const int zMin = 0;
            int xMax = room.NumXSectors - 1;
            int zMax = room.NumZSectors - 1;
            Blocks Blocks = room.Blocks;

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
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, false, useLegacyCode);
                    }


                    // -Z direction
                    if (x > 0 && x < room.NumXSectors - 1 && z > 1 && z < room.NumZSectors - 1 &&
                        !(Blocks[x, z - 1].Type == BlockType.Wall &&
                         (Blocks[x, z - 1].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x, z - 1].Floor.DiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z - 1].Floor.DiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall ||
                            (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZp || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, false, useLegacyCode);
                    }

                    // +X direction
                    if (z > 0 && z < room.NumZSectors - 1 && x > 0 && x < room.NumXSectors - 2 &&
                        !(Blocks[x + 1, z].Type == BlockType.Wall &&
                        (Blocks[x + 1, z].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x + 1, z].Floor.DiagonalSplit == DiagonalSplit.XnZn || Blocks[x + 1, z].Floor.DiagonalSplit == DiagonalSplit.XnZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZn || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XnZp))
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, false, useLegacyCode);
                    }

                    // -X direction
                    if (z > 0 && z < room.NumZSectors - 1 && x > 1 && x < room.NumXSectors - 1 &&
                        !(Blocks[x - 1, z].Type == BlockType.Wall &&
                        (Blocks[x - 1, z].Floor.DiagonalSplit == DiagonalSplit.None || Blocks[x - 1, z].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x - 1, z].Floor.DiagonalSplit == DiagonalSplit.XpZp)))
                    {
                        if ((Blocks[x, z].Type == BlockType.Wall || (Blocks[x, z].WallPortal?.HasTexturedFaces ?? false)) &&
                            !(Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZn || Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.XpZp))
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, false, useLegacyCode);
                    }

                    // Diagonal faces
                    if (Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type == BlockType.Wall)
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalFloor, true, true, true, useLegacyCode);
                        }
                        else
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalFloor, true, false, false, useLegacyCode);
                        }
                    }

                    if (Blocks[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (Blocks[x, z].Type != BlockType.Wall)
                        {
                            AddVerticalFaces(room, x, z, FaceDirection.DiagonalCeiling, false, true, false, useLegacyCode);
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
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveZ, true, true, false, useLegacyCode);
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
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeZ, true, true, false, useLegacyCode);
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
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.PositiveX, true, true, false, useLegacyCode);
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
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, true, useLegacyCode);
                        else
                            AddVerticalFaces(room, x, z, FaceDirection.NegativeX, true, true, false, useLegacyCode);
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

            Block block = room.Blocks[x, z];

            TextureArea defaultTexture = room.Level.Settings.DefaultTexture;
            bool shouldApplyDefaultTexture1 = block.GetFaceTexture(face1) == TextureArea.None && defaultTexture != TextureArea.None,
                 shouldApplyDefaultTexture2 = block.GetFaceTexture(face2) == TextureArea.None && defaultTexture != TextureArea.None;

            if (shouldApplyDefaultTexture1)
                block.SetFaceTexture(face1, defaultTexture);

            if (shouldApplyDefaultTexture2)
                block.SetFaceTexture(face2, defaultTexture);

            TextureArea
                face1Texture = block.GetFaceTexture(face1),
                face2Texture = block.GetFaceTexture(face2);

            // Build sector
            if (splitType != DiagonalSplit.None)
            {
                switch (splitType)
                {
                    case DiagonalSplit.XpZn:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h0, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                                face1Texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                        }
                        
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                                face2Texture, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                        }
                            
                        break;

                    case DiagonalSplit.XnZn:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1, z * Level.BlockSizeUnit),
                                face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                        }

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                                face2Texture, new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                        }

                        break;

                    case DiagonalSplit.XnZp:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                                face2Texture, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                        }

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                                face1Texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                        }

                        break;

                    case DiagonalSplit.XpZp:
                        if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                        {
                            AddTriangle(x, z, face2,
                                new Vector3((x + 1) * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                                new Vector3(x * Level.BlockSizeUnit, h3, (z + 1) * Level.BlockSizeUnit),
                                face2Texture, new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                        }
                            

                        if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp && blockType != BlockType.Wall)
                        {
                            AddTriangle(x, z, face1,
                                new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                                new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                                face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                        }
                            
                        break;

                    default:
                        throw new NotSupportedException("Unknown FloorDiagonalSplit");
                }
            }
            else if (BlockSurface.IsQuad2(h0, h1, h2, h3) && portalMode == Room.RoomConnectionType.NoPortal)
            {
                AddQuad(x, z, face1,
                    new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                    new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                    new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                    new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                    face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            }
            else if (diagonalSplitXEqualsY || portalMode == Room.RoomConnectionType.TriangularPortalXnZp || portalMode == Room.RoomConnectionType.TriangularPortalXpZn)
            {
                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                {
                    AddTriangle(x, z, face2,
                        new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                        face2Texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
                }

                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                {
                    AddTriangle(x, z, face1,
                        new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                        face1Texture, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), true);
                } 
            }
            else
            {
                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                {
                    AddTriangle(x, z, face1,
                        new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h1, (z + 1) * Level.BlockSizeUnit),
                        new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                        face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), false);
                }

                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                {
                    AddTriangle(x, z, face2,
                        new Vector3((x + 1) * Level.BlockSizeUnit, h2, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h3, z * Level.BlockSizeUnit),
                        new Vector3(x * Level.BlockSizeUnit, h0, (z + 1) * Level.BlockSizeUnit),
                        face2Texture, new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), false);
                }
            }
        }

        public struct TopDownWallPoint
        {
			/// <summary>
			/// X coordinate of the wall point.
			/// </summary>
			public int X;

			/// <summary>
			/// Z coordinate of the wall point.
			/// </summary>
			public int Z;

			/// <summary>
			/// Minimum Y coordinate (height) of the wall point.
			/// </summary>
			public int MinY;

			/// <summary>
			/// Maximum Y coordinate (height) of the wall point.
			/// </summary>
			public int MaxY;
		}

        public struct BlockWallData
		{
            public Direction Direction;

			/// <summary>
			/// (X, Z) coordinates of where the wall starts from top-down view.
			/// </summary>
			public TopDownWallPoint Start;

			/// <summary>
			/// (X, X) coordinates of where the wall ends from top-down view.
			/// </summary>
			public TopDownWallPoint End;

			/// <summary>
			/// Y coordinates of the QA part of the wall.
			/// </summary>
			public (int StartY, int EndY) QA;

			/// <summary>
			/// Y coordinates of the WS part of the wall.
			/// </summary>
			public (int StartY, int EndY) WS;

            /// <summary>
            /// Y coordinates of the wall's floor subdivisions
            /// </summary>
            public List<(int StartY, int EndY)> FloorSubdivisions;

            /// <summary>
            /// Y coordinates of the wall's ceiling subdivisions
            /// </summary>
            public List<(int StartY, int EndY)> CeilingSubdivisions;

			public BlockWallData() // TODO: Add a proper ctor!!!
			{
                Direction = Direction.None;
                Start = new();
				End = new();
				QA = new();
				WS = new();
				FloorSubdivisions = new();
                CeilingSubdivisions = new();
			}

			/// <summary>
			/// Returns true if the QA part of the wall is fully above the maximum Y coordinate of the wall.
			/// </summary>
			public readonly bool IsQaFullyAboveMaxY => QA.StartY >= Start.MaxY && QA.EndY >= End.MaxY; // Technically should be classified as a full wall if true

			/// <summary>
			/// Returns true if the WS part of the wall is fully below the minimum Y coordinate of the wall.
			/// </summary>
			public readonly bool IsWsFullyBelowMinY => WS.StartY <= Start.MinY && WS.EndY <= End.MinY; // Technically should be classified as a full wall if true

			/// <summary>
			/// Returns true if the diagonal wall direction can have a non-diagonal (square from top-down) floor part.
			/// </summary>
			public bool CanHaveNonDiagonalFloorPart(DiagonalSplit diagonalFloorSplitOfBlock)
            {
                return
                    (diagonalFloorSplitOfBlock is DiagonalSplit.XnZp && Direction is Direction.NegativeZ or Direction.PositiveX) ||
					(diagonalFloorSplitOfBlock is DiagonalSplit.XpZn && Direction is Direction.NegativeX or Direction.PositiveZ) ||
					(diagonalFloorSplitOfBlock is DiagonalSplit.XpZp && Direction is Direction.NegativeZ or Direction.NegativeX) ||
					(diagonalFloorSplitOfBlock is DiagonalSplit.XnZn && Direction is Direction.PositiveZ or Direction.PositiveX);
			}

			/// <summary>
			/// Returns true if the diagonal wall direction can have a non-diagonal (square from top-down) ceiling part.
			/// </summary>
			public bool CanHaveNonDiagonalCeilingPart(DiagonalSplit diagonalCeilingSplitOfBlock)
			{
				return
					(diagonalCeilingSplitOfBlock is DiagonalSplit.XnZp && Direction is Direction.NegativeZ or Direction.PositiveX) ||
					(diagonalCeilingSplitOfBlock is DiagonalSplit.XpZn && Direction is Direction.NegativeX or Direction.PositiveZ) ||
					(diagonalCeilingSplitOfBlock is DiagonalSplit.XpZp && Direction is Direction.NegativeZ or Direction.NegativeX) ||
					(diagonalCeilingSplitOfBlock is DiagonalSplit.XnZn && Direction is Direction.PositiveZ or Direction.PositiveX);
			}

			public static BlockWallData GetPositiveZWallData(Room room, int x, int z, bool normalize)
			{
				Block block = room.Blocks[x, z];
				Block neighborBlock = room.Blocks[x, z + 1];

				var wall = new BlockWallData
				{
					Direction = Direction.PositiveZ,

					Start = new TopDownWallPoint
					{
						X = x + 1,
						Z = z + 1,
						MinY = neighborBlock.Floor.XpZn,
						MaxY = neighborBlock.Ceiling.XpZn
					},

					End = new TopDownWallPoint
					{
						X = x,
						Z = z + 1,
						MinY = neighborBlock.Floor.XnZn,
						MaxY = neighborBlock.Ceiling.XnZn
					}
				};

				wall.QA.StartY = block.Floor.XpZp;
				wall.QA.EndY = block.Floor.XnZp;
				wall.WS.StartY = block.Ceiling.XpZp;
				wall.WS.EndY = block.Ceiling.XnZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));
				}

				if (block.WallPortal is not null)
				{
					// Get the adjoining room of the portal
					Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

					if (room.Alternated &&
						room.AlternateBaseRoom is not null &&
						adjoiningRoom.Alternated &&
						adjoiningRoom.AlternateRoom is not null)
					{
						adjoiningRoom = adjoiningRoom.AlternateRoom;
					}

					// Get the near block in current room
					Block nearBlock = room.Blocks[x, 1];

					int qaNearStart = nearBlock.Floor.XpZn;
					int qaNearEnd = nearBlock.Floor.XnZn;

					if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
						qaNearStart = qaNearEnd;
                    else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
						qaNearEnd = qaNearStart;

					int wsNearStart = nearBlock.Ceiling.XpZn;
					int wsNearEnd = nearBlock.Ceiling.XnZn;

					if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
						wsNearStart = wsNearEnd;
                    else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
						wsNearEnd = wsNearStart;

					// Now get the facing block on the adjoining room and calculate the correct heights
					int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

					Block adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;

					int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZp;
					int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZp;

					if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
						qAportal = qBportal;
                    else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
						qBportal = qAportal;

					wall.QA.StartY = room.Position.Y + qaNearStart;
					wall.QA.EndY = room.Position.Y + qaNearEnd;
					wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
					wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

					int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
					int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;

					if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
						wAportal = wBportal;
                    else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
						wBportal = wAportal;

					wall.WS.StartY = room.Position.Y + wsNearStart;
					wall.WS.EndY = room.Position.Y + wsNearEnd;
					wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
					wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

					(int, int) newSubdivision;

					for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp));

						if (i >= wall.FloorSubdivisions.Count)
							wall.FloorSubdivisions.Add(newSubdivision);
						else
							wall.FloorSubdivisions[i] = newSubdivision;
					}

					for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp));

						if (i >= wall.CeilingSubdivisions.Count)
							wall.CeilingSubdivisions.Add(newSubdivision);
						else
							wall.CeilingSubdivisions[i] = newSubdivision;
					}
				}

				if (block.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.QA.StartY = block.Floor.XnZp;
					wall.QA.EndY = block.Floor.XnZp;
				}
				else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.QA.StartY = block.Floor.XpZp;
					wall.QA.EndY = block.Floor.XpZp;
				}

				if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.Start.MinY = neighborBlock.Floor.XpZn;
					wall.End.MinY = neighborBlock.Floor.XpZn;
				}
				else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.Start.MinY = neighborBlock.Floor.XnZn;
					wall.End.MinY = neighborBlock.Floor.XnZn;
				}

				if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.WS.StartY = block.Ceiling.XnZp;
					wall.WS.EndY = block.Ceiling.XnZp;
				}
				else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.WS.StartY = block.Ceiling.XpZp;
					wall.WS.EndY = block.Ceiling.XpZp;
				}

				if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XpZn;
					wall.End.MaxY = neighborBlock.Ceiling.XpZn;
				}
				else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XnZn;
					wall.End.MaxY = neighborBlock.Ceiling.XnZn;
				}

                if (normalize)
					wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

				return wall;
			}

			public static BlockWallData GetNegativeZWallData(Room room, int x, int z, bool normalize)
			{
				Block block = room.Blocks[x, z];
				Block neighborBlock = room.Blocks[x, z - 1];

				var wall = new BlockWallData
				{
					Direction = Direction.NegativeZ,

					Start = new TopDownWallPoint
					{
						X = x,
						Z = z,
						MinY = neighborBlock.Floor.XnZp,
						MaxY = neighborBlock.Ceiling.XnZp
					},

					End = new TopDownWallPoint
					{
						X = x + 1,
						Z = z,
						MinY = neighborBlock.Floor.XpZp,
						MaxY = neighborBlock.Ceiling.XpZp
					}
				};

				wall.QA.StartY = block.Floor.XnZn;
				wall.QA.EndY = block.Floor.XpZn;
				wall.WS.StartY = block.Ceiling.XnZn;
				wall.WS.EndY = block.Ceiling.XpZn;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));
				}

				if (block.WallPortal is not null)
				{
					// Get the adjoining room of the portal
					Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

					if (room.Alternated &&
						room.AlternateBaseRoom is not null &&
						adjoiningRoom.Alternated &&
						adjoiningRoom.AlternateRoom is not null)
					{
						adjoiningRoom = adjoiningRoom.AlternateRoom;
					}

					// Get the near block in current room
					Block nearBlock = room.Blocks[x, room.NumZSectors - 2];

					int qaNearStart = nearBlock.Floor.XnZp;
					int qaNearEnd = nearBlock.Floor.XpZp;

					if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
						qaNearStart = qaNearEnd;
					else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
						qaNearEnd = qaNearStart;

					int wsNearStart = nearBlock.Ceiling.XnZp;
					int wsNearEnd = nearBlock.Ceiling.XpZp;

					if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
						wsNearStart = wsNearEnd;
                    else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
						wsNearEnd = wsNearStart;

					// Now get the facing block on the adjoining room and calculate the correct heights
					int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

					Block adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;

					int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZn;
					int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZn;

					if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
						qAportal = qBportal;
					else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
						qBportal = qAportal;

					wall.QA.StartY = room.Position.Y + qaNearStart;
					wall.QA.EndY = room.Position.Y + qaNearEnd;
					wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
					wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

					int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
					int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;

					if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
						wAportal = wBportal;
					else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
						wBportal = wAportal;

					wall.WS.StartY = room.Position.Y + wsNearStart;
					wall.WS.EndY = room.Position.Y + wsNearEnd;
					wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
					wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

					(int, int) newSubdivision;

					for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn));

						if (i >= wall.FloorSubdivisions.Count)
							wall.FloorSubdivisions.Add(newSubdivision);
						else
							wall.FloorSubdivisions[i] = newSubdivision;
					}

					for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
						    adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn));

						if (i >= wall.CeilingSubdivisions.Count)
							wall.CeilingSubdivisions.Add(newSubdivision);
						else
							wall.CeilingSubdivisions[i] = newSubdivision;
					}
				}

				if (block.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.QA.StartY = block.Floor.XnZn;
					wall.QA.EndY = block.Floor.XnZn;
				}
				else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.QA.StartY = block.Floor.XpZn;
					wall.QA.EndY = block.Floor.XpZn;
				}

				if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.Start.MinY = neighborBlock.Floor.XnZp;
					wall.End.MinY = neighborBlock.Floor.XnZp;
				}
				else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.Start.MinY = neighborBlock.Floor.XpZp;
					wall.End.MinY = neighborBlock.Floor.XpZp;
				}

				if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.WS.StartY = block.Ceiling.XnZn;
					wall.WS.EndY = block.Ceiling.XnZn;
				}
				else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.WS.StartY = block.Ceiling.XpZn;
					wall.WS.EndY = block.Ceiling.XpZn;
				}

				if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XnZp;
					wall.End.MaxY = neighborBlock.Ceiling.XnZp;
				}
				else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XpZp;
					wall.End.MaxY = neighborBlock.Ceiling.XpZp;
				}

				if (normalize)
					wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

				return wall;
			}

			public static BlockWallData GetPositiveXWallData(Room room, int x, int z, bool normalize)
			{
				Block block = room.Blocks[x, z];
				Block neighborBlock = room.Blocks[x + 1, z];

				var wall = new BlockWallData
				{
					Direction = Direction.PositiveX,

					Start = new TopDownWallPoint
					{
						X = x + 1,
						Z = z,
						MinY = neighborBlock.Floor.XnZn,
						MaxY = neighborBlock.Ceiling.XnZn
					},

					End = new TopDownWallPoint
					{
						X = x + 1,
						Z = z + 1,
						MinY = neighborBlock.Floor.XnZp,
						MaxY = neighborBlock.Ceiling.XnZp
					}
				};

				wall.QA.StartY = block.Floor.XpZn;
				wall.QA.EndY = block.Floor.XpZp;
				wall.WS.StartY = block.Ceiling.XpZn;
				wall.WS.EndY = block.Ceiling.XpZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
				        block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
					    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));
				}

				if (block.WallPortal is not null)
				{
					// Get the adjoining room of the portal
					Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

					if (room.Alternated &&
						room.AlternateBaseRoom is not null &&
						adjoiningRoom.Alternated &&
					    adjoiningRoom.AlternateRoom is not null)
					{
						adjoiningRoom = adjoiningRoom.AlternateRoom;
					}

					// Get the near block in current room
					Block nearBlock = room.Blocks[1, z];

					int qaNearStart = nearBlock.Floor.XnZn;
					int qaNearEnd = nearBlock.Floor.XnZp;

					if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
						qaNearStart = qaNearEnd;
					else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
						qaNearEnd = qaNearStart;

					int wsNearStart = nearBlock.Ceiling.XnZn;
					int wsNearEnd = nearBlock.Ceiling.XnZp;

					if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
						wsNearStart = wsNearEnd;
					else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
						wsNearEnd = wsNearStart;

					// Now get the facing block on the adjoining room and calculate the correct heights
					int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

					Block adjoiningBlock = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;

					int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZn;
					int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZp;

					if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
						qAportal = qBportal;
					else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
						qBportal = qAportal;

					wall.QA.StartY = room.Position.Y + qaNearStart;
					wall.QA.EndY = room.Position.Y + qaNearEnd;
					wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
					wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

					int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
					int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;

					if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
						wAportal = wBportal;
					else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
						wBportal = wAportal;

					wall.WS.StartY = room.Position.Y + wsNearStart;
					wall.WS.EndY = room.Position.Y + wsNearEnd;
					wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
					wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

					(int, int) newSubdivision;

					for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp));

						if (i >= wall.FloorSubdivisions.Count)
							wall.FloorSubdivisions.Add(newSubdivision);
						else
							wall.FloorSubdivisions[i] = newSubdivision;
					}

					for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp));

						if (i >= wall.CeilingSubdivisions.Count)
							wall.CeilingSubdivisions.Add(newSubdivision);
						else
							wall.CeilingSubdivisions[i] = newSubdivision;
					}
				}

				if (block.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.QA.StartY = block.Floor.XpZp;
					wall.QA.EndY = block.Floor.XpZp;
				}
				else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.QA.StartY = block.Floor.XpZn;
					wall.QA.EndY = block.Floor.XpZn;
				}

				if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.Start.MinY = neighborBlock.Floor.XnZp;
					wall.End.MinY = neighborBlock.Floor.XnZp;
				}
				else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.Start.MinY = neighborBlock.Floor.XnZn;
					wall.End.MinY = neighborBlock.Floor.XnZn;
				}

				if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.WS.StartY = block.Ceiling.XpZp;
					wall.WS.EndY = block.Ceiling.XpZp;
				}
				else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.WS.StartY = block.Ceiling.XpZn;
					wall.WS.EndY = block.Ceiling.XpZn;
				}

				if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XnZp;
					wall.End.MaxY = neighborBlock.Ceiling.XnZp;
				}
				else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XnZn;
					wall.End.MaxY = neighborBlock.Ceiling.XnZn;
				}

				if (normalize)
					wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

				return wall;
			}

			public static BlockWallData GetNegativeXWallData(Room room, int x, int z, bool normalize)
			{
				Block block = room.Blocks[x, z];
				Block neighborBlock = room.Blocks[x - 1, z];

				var wall = new BlockWallData
				{
					Direction = Direction.NegativeX,

					Start = new TopDownWallPoint
					{
						X = x,
						Z = z + 1,
						MinY = neighborBlock.Floor.XpZp,
						MaxY = neighborBlock.Ceiling.XpZp
					},

					End = new TopDownWallPoint
					{
						X = x,
						Z = z,
						MinY = neighborBlock.Floor.XpZn,
						MaxY = neighborBlock.Ceiling.XpZn
					}
				};

				wall.QA.StartY = block.Floor.XnZp;
				wall.QA.EndY = block.Floor.XpZp;
				wall.WS.StartY = block.Ceiling.XnZp;
				wall.WS.EndY = block.Ceiling.XpZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add((
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add((
					    block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));
				}

				if (block.WallPortal is not null)
				{
					// Get the adjoining room of the portal
					Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

					if (room.Alternated &&
						room.AlternateBaseRoom is not null &&
						adjoiningRoom.Alternated &&
						adjoiningRoom.AlternateRoom is not null)
					{
						adjoiningRoom = adjoiningRoom.AlternateRoom;
					}

					// Get the near block in current room
					Block nearBlock = room.Blocks[room.NumXSectors - 2, z];

					int qaNearStart = nearBlock.Floor.XpZp;
					int qaNearEnd = nearBlock.Floor.XpZn;

					if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
						qaNearStart = qaNearEnd;
					else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
						qaNearEnd = qaNearStart;

					int wsNearStart = nearBlock.Ceiling.XpZn;
					int wsNearEnd = nearBlock.Ceiling.XpZp;

					if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
						wsNearStart = wsNearEnd;
					else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
						wsNearEnd = wsNearStart;

					// Now get the facing block on the adjoining room and calculate the correct heights
					int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

					Block adjoiningBlock = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;

					int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZp;
					int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZn;

					if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
						qAportal = qBportal;
					else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
						qBportal = qAportal;

					wall.QA.StartY = room.Position.Y + qaNearStart;
					wall.QA.EndY = room.Position.Y + qaNearEnd;
					wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
					wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

					int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
					int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;

					if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
						wAportal = wBportal;
					else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
						wBportal = wAportal;

					wall.WS.StartY = room.Position.Y + wsNearStart;
					wall.WS.EndY = room.Position.Y + wsNearEnd;
					wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
					wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

					(int, int) newSubdivision;

					for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn));

						if (i >= wall.FloorSubdivisions.Count)
							wall.FloorSubdivisions.Add(newSubdivision);
						else
							wall.FloorSubdivisions[i] = newSubdivision;
					}

					for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
					{
						newSubdivision = (adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
							adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn));

						if (i >= wall.CeilingSubdivisions.Count)
							wall.CeilingSubdivisions.Add(newSubdivision);
						else
							wall.CeilingSubdivisions[i] = newSubdivision;
					}
				}

				if (block.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.QA.StartY = block.Floor.XnZp;
					wall.QA.EndY = block.Floor.XnZp;
				}
				else if (block.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.QA.StartY = block.Floor.XnZn;
					wall.QA.EndY = block.Floor.XnZn;
				}

				if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.Start.MinY = neighborBlock.Floor.XpZp;
					wall.End.MinY = neighborBlock.Floor.XpZp;
				}
				else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.Start.MinY = neighborBlock.Floor.XpZn;
					wall.End.MinY = neighborBlock.Floor.XpZn;
				}

				if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				{
					wall.WS.StartY = block.Ceiling.XnZp;
					wall.WS.EndY = block.Ceiling.XnZp;
				}
				else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				{
					wall.WS.StartY = block.Ceiling.XnZn;
					wall.WS.EndY = block.Ceiling.XnZn;
				}

				if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XpZp;
					wall.End.MaxY = neighborBlock.Ceiling.XpZp;
				}
				else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				{
					wall.Start.MaxY = neighborBlock.Ceiling.XpZn;
					wall.End.MaxY = neighborBlock.Ceiling.XpZn;
				}

				if (normalize)
					wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

				return wall;
			}

            public static BlockWallData GetDiagonalWallData(Room room, int x, int z, bool isDiagonalCeiling, bool normalize)
            {
				Block block = room.Blocks[x, z];
                BlockWallData wall;

                switch (isDiagonalCeiling ? block.Ceiling.DiagonalSplit : block.Floor.DiagonalSplit)
				{
					case DiagonalSplit.XpZn:
						wall = new BlockWallData
						{
							Start = new TopDownWallPoint
							{
								X = x + 1,
								Z = z + 1,

								MinY = isDiagonalCeiling
                                    ? (block.IsAnyWall ? block.Floor.XnZp : block.Floor.XpZp) // DiagonalCeiling
									: block.Floor.XnZp, // DiagonalFloor

								MaxY = isDiagonalCeiling
                                    ? block.Ceiling.XnZp // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XpZp) // DiagonalFloor
							},

							End = new TopDownWallPoint
							{
								X = x,
								Z = z,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XnZp : block.Floor.XnZn) // DiagonalCeiling
									: block.Floor.XnZp, // DiagonalFloor

								MaxY = isDiagonalCeiling
									? block.Ceiling.XnZp // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XnZn) // DiagonalFloor
							}
						};

						wall.QA.StartY = block.Floor.XpZp;
						wall.QA.EndY = block.Floor.XnZn;
						wall.WS.StartY = block.Ceiling.XpZp;
						wall.WS.EndY = block.Ceiling.XnZn;

						for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
						{
							wall.FloorSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));
						}

						for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
						{
							wall.CeilingSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));
						}

                        break;

					case DiagonalSplit.XnZn:
						wall = new BlockWallData
						{
							Start = new TopDownWallPoint
							{
								X = x + 1,
								Z = z,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XpZp : block.Floor.XpZn) // DiagonalCeiling
									: block.Floor.XpZp, // DiagonalFloor

								MaxY = isDiagonalCeiling
								    ? block.Ceiling.XpZp // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XpZn) // DiagonalFloor
							},

							End = new TopDownWallPoint
							{
								X = x,
								Z = z + 1,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XpZp : block.Floor.XnZp) // DiagonalCeiling
									: block.Floor.XpZp, // DiagonalFloor

								MaxY = isDiagonalCeiling
								    ? block.Ceiling.XpZp // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XnZp) // DiagonalFloor
							}
						};

						wall.QA.StartY = block.Floor.XpZn;
						wall.QA.EndY = block.Floor.XnZp;
						wall.WS.StartY = block.Ceiling.XpZn;
						wall.WS.EndY = block.Ceiling.XnZp;

						for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
						{
							wall.FloorSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));
						}

						for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
						{
							wall.CeilingSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));
						}

						break;

					case DiagonalSplit.XnZp:
						wall = new BlockWallData
						{
							Start = new TopDownWallPoint
							{
								X = x,
								Z = z,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XpZn : block.Floor.XnZn) // DiagonalCeiling
									: block.Floor.XpZn, // DiagonalFloor

								MaxY = isDiagonalCeiling
								    ? block.Ceiling.XpZn // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XnZn) // DiagonalFloor
							},

							End = new TopDownWallPoint
							{
								X = x + 1,
								Z = z + 1,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XpZn : block.Floor.XpZp) // DiagonalCeiling
									: block.Floor.XpZn, // DiagonalFloor

								MaxY = isDiagonalCeiling
									? block.Ceiling.XpZn // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XpZp) // DiagonalFloor
							}
						};

						wall.QA.StartY = block.Floor.XnZn;
						wall.QA.EndY = block.Floor.XpZp;
						wall.WS.StartY = block.Ceiling.XnZn;
						wall.WS.EndY = block.Ceiling.XpZp;

						for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
							wall.FloorSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));

						for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
							wall.CeilingSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));

						break;

					default:
						wall = new BlockWallData
						{
							Start = new TopDownWallPoint
							{
								X = x,
								Z = z + 1,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XnZn : block.Floor.XnZp) // DiagonalCeiling
									: block.Floor.XnZn, // DiagonalFloor

								MaxY = isDiagonalCeiling
									? block.Ceiling.XnZn // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XnZp) // DiagonalFloor
							},

							End = new TopDownWallPoint
							{
								X = x + 1,
								Z = z,

								MinY = isDiagonalCeiling
									? (block.IsAnyWall ? block.Floor.XnZn : block.Floor.XpZn) // DiagonalCeiling
									: block.Floor.XnZn, // DiagonalFloor

								MaxY = isDiagonalCeiling
								    ? block.Ceiling.XnZn // DiagonalCeiling
									: (block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XpZn) // DiagonalFloor
							}
						};

						wall.QA.StartY = block.Floor.XnZp;
						wall.QA.EndY = block.Floor.XpZn;
						wall.WS.StartY = block.Ceiling.XnZp;
						wall.WS.EndY = block.Ceiling.XpZn;

						for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
						{
							wall.FloorSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
								block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));
						}

						for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
						{
							wall.CeilingSubdivisions.Add((
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
								block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));
						}

						break;
				}

				wall.Direction = Direction.Diagonal;

				if (normalize)
					wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

				return wall;
			}

			private void Normalize(DiagonalSplit diagonalFloorSplit, DiagonalSplit diagonalCeilingSplit, bool isAnyWall)
            {
				QA = NormalizeFloorSplit(QA, diagonalFloorSplit, isAnyWall, out bool isFloorSplitInFloorVoid);
				WS = NormalizeCeilingSplit(WS, diagonalCeilingSplit, isAnyWall, out bool isCeilingSplitInCeilingVoid);

				if (!isFloorSplitInFloorVoid)
				{
					for (int i = 0; i < FloorSubdivisions.Count; i++)
					{
						(int StartY, int EndY) normalizedSubdivision = NormalizeFloorSplit(FloorSubdivisions[i], diagonalFloorSplit, isAnyWall, out isFloorSplitInFloorVoid);

						if (isFloorSplitInFloorVoid)
						{
                            // Remove the rest as it will also be in the void, therefore not rendered
							FloorSubdivisions.RemoveRange(i, FloorSubdivisions.Count - i);
							break;
						}

						FloorSubdivisions[i] = normalizedSubdivision;
					}
				}

				if (!isCeilingSplitInCeilingVoid)
				{
					for (int i = 0; i < CeilingSubdivisions.Count; i++)
					{
						(int StartY, int EndY) normalizedSubdivision = NormalizeCeilingSplit(CeilingSubdivisions[i], diagonalCeilingSplit, isAnyWall, out isCeilingSplitInCeilingVoid);

						if (isCeilingSplitInCeilingVoid)
						{
							// Remove the rest as it will also be in the void, therefore not rendered
							CeilingSubdivisions.RemoveRange(i, CeilingSubdivisions.Count - i);
							break;
						}

						CeilingSubdivisions[i] = normalizedSubdivision;
					}
				}
			}

            private (int StartY, int EndY) NormalizeFloorSplit((int StartY, int EndY) split, DiagonalSplit diagonalFloorSplit, bool isAnyWall, out bool isInFloorVoid)
            {
                isInFloorVoid = true;

                bool canHaveNonDiagonalFloorPart = CanHaveNonDiagonalFloorPart(diagonalFloorSplit); // The wall bit under the flat floor triangle of a diagonal wall
				bool isFaceInFloorVoid = split.StartY < Start.MinY || split.EndY < End.MinY || (split.StartY == Start.MinY && split.EndY == End.MinY);

				if (isFaceInFloorVoid && isAnyWall && !canHaveNonDiagonalFloorPart) // Part of overdraw prevention
					return split; // Stop the loop, since the rest of the subdivisions will also be in the void

				isInFloorVoid = false;

				bool isQaFullyAboveCeiling = QA.StartY >= Start.MaxY && QA.EndY >= End.MaxY; // Technically should be classified as a wall if true

				bool isEitherStartPointAboveCeiling = split.StartY > Start.MaxY || split.EndY > End.MaxY; // If either start point A or B is in the void above ceiling
				bool areBothStartPointsAboveCeiling = split.StartY >= Start.MaxY && split.EndY >= End.MaxY; // Are both start points A and B in the void above ceiling

				// Walls can't have overdraw, so if either point is in void, then snap it to ceiling
				// Diagonal walls are an exception, since, even though they are walls, they have a flat floor bit, so we can allow overdraw
				if ((isEitherStartPointAboveCeiling && (isAnyWall || isQaFullyAboveCeiling) && !canHaveNonDiagonalFloorPart) || areBothStartPointsAboveCeiling)
				{
					// Snap points to ceiling
					split.StartY = Start.MaxY;
					split.EndY = End.MaxY;
				}

				// If either split point is above QA
				if (split.StartY > QA.StartY || split.EndY > QA.EndY)
				{
					// Snap points to the heights of QA
					split.StartY = QA.StartY;
					split.EndY = QA.EndY;
				}

				return split;
			}

            private (int StartY, int EndY) NormalizeCeilingSplit((int StartY, int EndY) split, DiagonalSplit diagonalCeilingSplit, bool isAnyWall, out bool isInCeilingVoid)
            {
				isInCeilingVoid = true;

                bool canHaveNonDiagonalCeilingPart = CanHaveNonDiagonalCeilingPart(diagonalCeilingSplit); // The wall bit over the flat ceiling triangle of a diagonal wall
				bool isFaceInCeilingVoid = split.StartY > Start.MaxY || split.EndY > End.MaxY || (split.StartY == Start.MaxY && split.EndY == End.MaxY);

				if (isFaceInCeilingVoid && isAnyWall && !canHaveNonDiagonalCeilingPart) // Part of overdraw prevention
					return split; // Stop the loop, since the rest of the subdivisions will also be in the void

				isInCeilingVoid = false;

				bool isWsFullyAboveCeiling = WS.StartY <= Start.MinY && WS.EndY <= End.MinY; // Technically should be classified as a wall if true

				bool isEitherStartPointBelowFloor = split.StartY < Start.MinY || split.EndY < End.MinY; // If either start point A or B is in the void below floor
				bool areBothStartPointsBelowFloor = split.StartY <= Start.MinY && split.EndY <= End.MinY; // Are both start points A and B in the void below floor

				// Walls can't have overdraw, so if either point is in void, then snap it to floor
				// Diagonal walls are an exception, since, even though they are walls, they have a flat ceiling bit, so we can allow overdraw
				if ((isEitherStartPointBelowFloor && (isAnyWall || isWsFullyAboveCeiling) && !canHaveNonDiagonalCeilingPart) || areBothStartPointsBelowFloor)
				{
					// Snap points to floor
					split.StartY = Start.MinY;
					split.EndY = End.MinY;
				}

				// If either split point is below WS
				if (split.StartY < WS.StartY || split.EndY < WS.EndY)
				{
					// Snap points to the heights of WS
					split.StartY = WS.StartY;
					split.EndY = WS.EndY;
				}

				return split;
			}
		}

        private void AddVerticalFaces(Room room, int x, int z, FaceDirection faceDirection, bool hasFloorPart, bool hasCeilingPart, bool hasMiddlePart, bool useLegacyCode = false)
		{
			BlockWallData wallData = faceDirection switch
			{
				FaceDirection.PositiveZ => BlockWallData.GetPositiveZWallData(room, x, z, true),
				FaceDirection.NegativeZ => BlockWallData.GetNegativeZWallData(room, x, z, true),
				FaceDirection.PositiveX => BlockWallData.GetPositiveXWallData(room, x, z, true),
				FaceDirection.NegativeX => BlockWallData.GetNegativeXWallData(room, x, z, true),
                FaceDirection.DiagonalFloor => BlockWallData.GetDiagonalWallData(room, x, z, false, true),
				FaceDirection.DiagonalCeiling => BlockWallData.GetDiagonalWallData(room, x, z, true, true),
				_ => throw new NotSupportedException("Unsupported face direction.")
			};

            Block block = room.Blocks[x, z];

            if (hasFloorPart)
            {
				IReadOnlyList<FaceData> verticalFloorPartFaces = GetVerticalFloorPartFaces(wallData, block.Floor.DiagonalSplit, block.IsAnyWall);

				for (int i = 0; i < verticalFloorPartFaces.Count; i++)
				{
					FaceData face = verticalFloorPartFaces[i];
					TextureArea texture = block.GetFaceTexture(face.BlockFace);

					if (face.IsQuad)
						AddQuad(x, z, face.BlockFace, face.P0, face.P1, face.P2, face.P3.Value, texture, face.UV0, face.UV1, face.UV2, face.UV3.Value);
					else
						AddTriangle(x, z, face.BlockFace, face.P0, face.P1, face.P2, texture, face.UV0, face.UV1, face.UV2, face.IsXEqualYDiagonal.Value);
				}
			}

            if (hasCeilingPart)
            {
				IReadOnlyList<FaceData> verticalCeilingPartFaces = GetVerticalCeilingPartFaces(wallData, block.Ceiling.DiagonalSplit, block.IsAnyWall);

				for (int i = 0; i < verticalCeilingPartFaces.Count; i++)
				{
					FaceData face = verticalCeilingPartFaces[i];
					TextureArea texture = block.GetFaceTexture(face.BlockFace);

					if (face.IsQuad)
						AddQuad(x, z, face.BlockFace, face.P0, face.P1, face.P2, face.P3.Value, texture, face.UV0, face.UV1, face.UV2, face.UV3.Value);
					else
						AddTriangle(x, z, face.BlockFace, face.P0, face.P1, face.P2, texture, face.UV0, face.UV1, face.UV2, face.IsXEqualYDiagonal.Value);
				}
			}

            if (hasMiddlePart)
            {
				FaceData? middleFace = GetVerticalMiddleFace(wallData);

				if (middleFace.HasValue)
				{
					FaceData face = middleFace.Value;
					TextureArea texture = block.GetFaceTexture(face.BlockFace);

					if (face.IsQuad)
						AddQuad(x, z, face.BlockFace, face.P0, face.P1, face.P2, face.P3.Value, texture, face.UV0, face.UV1, face.UV2, face.UV3.Value);
					else
						AddTriangle(x, z, face.BlockFace, face.P0, face.P1, face.P2, texture, face.UV0, face.UV1, face.UV2, face.IsXEqualYDiagonal.Value);
				}
			}
		}

        public struct FaceData
        {
            public BlockFace BlockFace;

			public Vector3 P0;
			public Vector3 P1;
			public Vector3 P2;
			public Vector3? P3;

            public Vector2 UV0;
			public Vector2 UV1;
			public Vector2 UV2;
			public Vector2? UV3;

            public bool? IsXEqualYDiagonal;

			public readonly bool IsQuad => P3.HasValue;
			public readonly bool IsTriangle => !P3.HasValue;
		}

        private IReadOnlyList<FaceData> GetVerticalFloorPartFaces(BlockWallData wall, DiagonalSplit diagonalFloorSplit, bool isAnyWall)
        {
			static FaceData? CreateFaceData(BlockFace blockFace, (int X, int Z) wallStart, (int X, int Z) wallEnd, (int StartY, int EndY) faceStart, (int StartY, int EndY) faceEnd)
            {
                if (faceStart.StartY > faceEnd.StartY && faceStart.EndY > faceEnd.EndY) // Is quad
                    return new FaceData()
                    {
                        BlockFace = blockFace,
                        P0 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceStart.StartY, wallStart.Z * Level.BlockSizeUnit),
                        P1 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceStart.EndY, wallEnd.Z * Level.BlockSizeUnit),
                        P2 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
                        P3 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
						UV0 = new Vector2(0, 0), UV1 = new Vector2(1, 0), UV2 = new Vector2(1, 1), UV3 = new Vector2(0, 1)
					};
                else if (faceStart.StartY == faceEnd.StartY && faceStart.EndY > faceEnd.EndY) // Is triangle (type 1)
                    return new FaceData()
					{
						BlockFace = blockFace,
						P0 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
						P1 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceStart.EndY, wallEnd.Z * Level.BlockSizeUnit),
						P2 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
						UV0 = new Vector2(1, 1), UV1 = new Vector2(0, 0), UV2 = new Vector2(1, 0),
                        IsXEqualYDiagonal = false
					};
				else if (faceStart.StartY > faceEnd.StartY && faceStart.EndY == faceEnd.EndY) // Is triangle (type 2)
                    return new FaceData()
                    {
						BlockFace = blockFace,
						P0 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceStart.StartY, wallStart.Z * Level.BlockSizeUnit),
						P1 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
						P2 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
						UV0 = new Vector2(0, 1), UV1 = new Vector2(0, 0), UV2 = new Vector2(1, 0),
						IsXEqualYDiagonal = true
					};
				else
                    return null; // Not rendered - failed to meet any of the conditions
			}

			bool isQaFullyAboveCeiling = wall.IsQaFullyAboveMaxY; // Technically should be classified as a wall if true
			bool canHaveDiagonalWallFloorPart = wall.CanHaveNonDiagonalFloorPart(diagonalFloorSplit); // The wall bit under the flat floor triangle of a diagonal wall

			var faces = new List<FaceData>();
            FaceData? faceData;

			for (int extraSubdivisionIndex = -1; extraSubdivisionIndex < wall.FloorSubdivisions.Count; extraSubdivisionIndex++)
			{
				(int yStartA, int yStartB) = extraSubdivisionIndex == -1
                    ? (wall.QA.StartY, wall.QA.EndY) // Render QA face
					: wall.FloorSubdivisions[extraSubdivisionIndex]; // Render subdivision face

                BlockFace blockFace = extraSubdivisionIndex == -1
					? BlockFaceExtensions.GetQaFace(wall.Direction) // QA face
					: BlockFaceExtensions.GetExtraFloorSubdivisionFace(wall.Direction, extraSubdivisionIndex); // Subdivision face

				// Start with the floor as a baseline for the bottom end of the face
				(int yEndA, int yEndB) = (wall.Start.MinY, wall.End.MinY);

				if (extraSubdivisionIndex + 1 < wall.FloorSubdivisions.Count) // If a next floor subdivision exists
				{
					int yNextSubdivStart = wall.FloorSubdivisions[extraSubdivisionIndex + 1].StartY,
						yNextSubdivEnd = wall.FloorSubdivisions[extraSubdivisionIndex + 1].EndY;

					if ((canHaveDiagonalWallFloorPart || isQaFullyAboveCeiling) && (yNextSubdivStart > wall.QA.StartY || yNextSubdivEnd > wall.QA.EndY))
						continue; // Skip it, since it's above the flat, walkable triangle of a diagonal wall

					if (yNextSubdivStart >= wall.Start.MinY && yNextSubdivEnd >= wall.End.MinY) // If next subdivision is NOT in void below floor
					{
						// Make the next subdivision the bottom end of the face
						yEndA = yNextSubdivStart;
						yEndB = yNextSubdivEnd;
					}
				}

				if (yStartA <= yEndA && yStartB <= yEndB)
					continue; // 0 or negative height subdivision, don't render it

				faceData = CreateFaceData(blockFace, (wall.Start.X, wall.Start.Z), (wall.End.X, wall.End.Z), (yStartA, yStartB), (yEndA, yEndB));

				if (!faceData.HasValue)
				{
					// Try overdraw

					bool isQA = yStartA == wall.QA.StartY && yStartB == wall.QA.EndY;
					bool isValidOverdraw = isQA && (!isAnyWall || canHaveDiagonalWallFloorPart);

					if (!isValidOverdraw)
						continue;

					// Find lowest point between subdivision and baseline, then try and create an overdraw face out of it
					int lowest = Math.Min(Math.Min(yStartA, yStartB), Math.Min(yEndA, yEndB));
					faceData = CreateFaceData(blockFace, (wall.Start.X, wall.Start.Z), (wall.End.X, wall.End.Z), (yStartA, yStartB), (lowest, lowest));
				}

				if (faceData.HasValue)
					faces.Add(faceData.Value);
			}

			return faces;
		}

        private IReadOnlyList<FaceData> GetVerticalCeilingPartFaces(BlockWallData wall, DiagonalSplit diagonalCeilingSplit, bool isAnyWall)
        {
			static FaceData? CreateFaceData(BlockFace blockFace, (int X, int Z) wallStart, (int X, int Z) wallEnd, (int StartY, int EndY) faceStart, (int StartY, int EndY) faceEnd)
			{
				if (faceStart.StartY < faceEnd.StartY && faceStart.EndY < faceEnd.EndY) // Is quad
					return new FaceData()
					{
						BlockFace = blockFace,
						P0 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
						P1 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
						P2 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceStart.EndY, wallEnd.Z * Level.BlockSizeUnit),
						P3 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceStart.StartY, wallStart.Z * Level.BlockSizeUnit),
						UV0 = new Vector2(0, 0), UV1 = new Vector2(1, 0), UV2 = new Vector2(1, 1), UV3 = new Vector2(0, 1)
					};
				else if (faceStart.StartY < faceEnd.StartY && faceStart.EndY == faceEnd.EndY) // Is triangle (type 1)
					return new FaceData()
					{
						BlockFace = blockFace,
						P0 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
						P1 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
						P2 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceStart.StartY, wallStart.Z * Level.BlockSizeUnit),
						UV0 = new Vector2(0, 1), UV1 = new Vector2(0, 0), UV2 = new Vector2(1, 0),
						IsXEqualYDiagonal = true
					};
				else if (faceStart.StartY == faceEnd.StartY && faceStart.EndY < faceEnd.EndY) // Is triangle (type 2)
					return new FaceData()
					{
						BlockFace = blockFace,
						P0 = new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
						P1 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
						P2 = new Vector3(wallEnd.X * Level.BlockSizeUnit, faceStart.EndY, wallEnd.Z * Level.BlockSizeUnit),
						UV0 = new Vector2(1, 1), UV1 = new Vector2(0, 0), UV2 = new Vector2(1, 0),
						IsXEqualYDiagonal = false
					};
				else
					return null; // Not rendered - failed to meet any of the conditions
			}

			bool isWsFullyBelowMinY = wall.IsWsFullyBelowMinY; // Technically should be classified as a wall if true
			bool canHaveNonDiagonalCeilingPart = wall.CanHaveNonDiagonalCeilingPart(diagonalCeilingSplit); // The wall bit over the flat ceiling triangle of a diagonal wall

			var faces = new List<FaceData>();
            FaceData? faceData;

			// Render subdivision faces
			for (int extraSubdivisionIndex = -1; extraSubdivisionIndex < wall.CeilingSubdivisions.Count; extraSubdivisionIndex++)
			{
				(int yStartA, int yStartB) = extraSubdivisionIndex == -1
					? (wall.WS.StartY, wall.WS.EndY) // Render WS face
					: wall.CeilingSubdivisions[extraSubdivisionIndex]; // Render subdivision face

				BlockFace blockFace = extraSubdivisionIndex == -1
					? BlockFaceExtensions.GetWsFace(wall.Direction)
					: BlockFaceExtensions.GetExtraCeilingSubdivisionFace(wall.Direction, extraSubdivisionIndex);

				// Start with the ceiling as a baseline for the top end of the face
				(int yEndA, int yEndB) = (wall.Start.MaxY, wall.End.MaxY);

				if (extraSubdivisionIndex + 1 < wall.CeilingSubdivisions.Count) // If a next ceiling subdivision exists
				{
					int yNextSubdivA = wall.CeilingSubdivisions[extraSubdivisionIndex + 1].StartY,
						yNextSubdivB = wall.CeilingSubdivisions[extraSubdivisionIndex + 1].EndY;

					if ((canHaveNonDiagonalCeilingPart || isWsFullyBelowMinY) && (yNextSubdivA < wall.WS.StartY || yNextSubdivB < wall.WS.EndY))
						continue; // Skip it, since it's below the flat ceiling triangle

					if (yNextSubdivA <= wall.Start.MaxY && yNextSubdivB <= wall.End.MaxY) // If next subdivision is NOT in void above ceiling
					{
						// Make the next subdivision the top end of the face
						yEndA = yNextSubdivA;
						yEndB = yNextSubdivB;
					}
				}

				if (yStartA >= yEndA && yStartB >= yEndB)
					continue; // 0 or negative height subdivision, don't render it

				faceData = CreateFaceData(blockFace, (wall.Start.X, wall.Start.Z), (wall.End.X, wall.End.Z), (yStartA, yStartB), (yEndA, yEndB));

				if (faceData is null)
				{
					// Try overdraw

					bool isWS = yStartA == wall.WS.StartY && yStartB == wall.WS.EndY;
					bool isValidOverdraw = isWS && (!isAnyWall || canHaveNonDiagonalCeilingPart);

					if (!isValidOverdraw)
						continue;

					// Find highest point between subdivision and baseline, then try and create an overdraw face out of it
					int highest = Math.Max(Math.Max(yStartA, yStartB), Math.Max(yEndA, yEndB));
					faceData = CreateFaceData(blockFace, (wall.Start.X, wall.Start.Z), (wall.End.X, wall.End.Z), (highest, highest), (yStartA, yStartB));
				}

				if (faceData.HasValue)
					faces.Add(faceData.Value);
			}

			return faces;
		}

        private FaceData? GetVerticalMiddleFace(BlockWallData wall)
        {
			if (wall.QA.StartY < wall.Start.MinY || wall.QA.EndY < wall.End.MinY)
			{
				wall.QA.StartY = wall.Start.MinY;
				wall.QA.EndY = wall.End.MinY;
			}

			if (wall.WS.StartY > wall.Start.MaxY || wall.WS.EndY > wall.End.MaxY)
			{
				wall.WS.StartY = wall.Start.MaxY;
				wall.WS.EndY = wall.End.MaxY;
			}

            int yStartA = wall.QA.StartY <= wall.Start.MinY ? wall.Start.MinY : wall.QA.StartY,
                yStartB = wall.QA.EndY <= wall.End.MinY ? wall.End.MinY : wall.QA.EndY,
                yEndA = wall.WS.StartY >= wall.Start.MaxY ? wall.Start.MaxY : wall.WS.StartY,
                yEndB = wall.WS.EndY >= wall.End.MaxY ? wall.End.MaxY : wall.WS.EndY;

            BlockFace blockFace = BlockFaceExtensions.GetMiddleFace(wall.Direction);

            if (yStartA != yEndA && yStartB != yEndB) // Is quad
				return new FaceData()
                {
					BlockFace = blockFace,
					P0 = new Vector3(wall.Start.X * Level.BlockSizeUnit, yEndA, wall.Start.Z * Level.BlockSizeUnit),
                    P1 = new Vector3(wall.End.X * Level.BlockSizeUnit, yEndB, wall.End.Z * Level.BlockSizeUnit),
                    P2 = new Vector3(wall.End.X * Level.BlockSizeUnit, yStartB, wall.End.Z * Level.BlockSizeUnit),
                    P3 = new Vector3(wall.Start.X * Level.BlockSizeUnit, yStartA, wall.Start.Z * Level.BlockSizeUnit),
					UV0 = new Vector2(0, 0), UV1 = new Vector2(1, 0), UV2 = new Vector2(1, 1), UV3 = new Vector2(0, 1)
				};
			else if (yStartA != yEndA && yStartB == yEndB) // Is triangle (type 1)
				return new FaceData()
                {
					BlockFace = blockFace,
					P0 = new Vector3(wall.Start.X * Level.BlockSizeUnit, yEndA, wall.Start.Z * Level.BlockSizeUnit),
					P1 = new Vector3(wall.End.X * Level.BlockSizeUnit, yEndB, wall.End.Z * Level.BlockSizeUnit),
					P2 = new Vector3(wall.Start.X * Level.BlockSizeUnit, yStartB, wall.Start.Z * Level.BlockSizeUnit),
					UV0 = new Vector2(0, 1), UV1 = new Vector2(0, 0), UV2 = new Vector2(1, 0),
					IsXEqualYDiagonal = true
				};
			else if (yStartA == yEndA && yStartB != yEndB) // Is triangle (type 2)
				return new FaceData()
				{
					BlockFace = blockFace,
					P0 = new Vector3(wall.Start.X * Level.BlockSizeUnit, yEndA, wall.Start.Z * Level.BlockSizeUnit),
					P1 = new Vector3(wall.End.X * Level.BlockSizeUnit, yEndB, wall.End.Z * Level.BlockSizeUnit),
					P2 = new Vector3(wall.End.X * Level.BlockSizeUnit, yStartB, wall.End.Z * Level.BlockSizeUnit),
					UV0 = new Vector2(1, 1), UV1 = new Vector2(0, 0), UV2 = new Vector2(1, 0),
					IsXEqualYDiagonal = false
				};
			else
				return null; // Not rendered - failed to meet any of the conditions
		}

        private struct BlockFaceDTO
        {
            public Block Block { get; set; }
            public BlockFace Face { get; set; }

            public int BlockX { get; set; }
            public int BlockZ { get; set; }

            public int XA { get; set; }
            public int XB { get; set; }

            public int ZA { get; set; }
            public int ZB { get; set; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryRenderFloorWallFace(BlockFaceDTO dto, (int A, int B) yStart, (int A, int B) yEnd, TextureArea defaultTexture)
        {
            bool shouldApplyDefaultTexture = dto.Block.GetFaceTexture(dto.Face) == TextureArea.None && defaultTexture != TextureArea.None;

            if (shouldApplyDefaultTexture)
                dto.Block.SetFaceTexture(dto.Face, defaultTexture);

            TextureArea texture = dto.Block.GetFaceTexture(dto.Face);

            if (yStart.A > yEnd.A && yStart.B > yEnd.B) // Is quad
                AddQuad(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yStart.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yStart.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            else if (yStart.A == yEnd.A && yStart.B > yEnd.B) // Is triangle (type 1)
                AddTriangle(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yStart.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    texture, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
            else if (yStart.A > yEnd.A && yStart.B == yEnd.B)  // Is triangle (type 2)
                AddTriangle(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yStart.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
            else
                return false; // Not rendered - failed to meet any of the conditions

            return true; // Rendered successfully
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryRenderCeilingWallFace(BlockFaceDTO dto, (int A, int B) yStart, (int A, int B) yEnd, TextureArea defaultTexture)
        {
            bool shouldApplyDefaultTexture = dto.Block.GetFaceTexture(dto.Face) == TextureArea.None && defaultTexture != TextureArea.None;

            if (shouldApplyDefaultTexture)
                dto.Block.SetFaceTexture(dto.Face, defaultTexture);

            TextureArea texture = dto.Block.GetFaceTexture(dto.Face);

            if (yStart.A < yEnd.A && yStart.B < yEnd.B)
                AddQuad(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yStart.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XA * Level.BlockSizeUnit, yStart.A, dto.ZA * Level.BlockSizeUnit),
                    texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            else if (yStart.A < yEnd.A && yStart.B == yEnd.B)
                AddTriangle(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XA * Level.BlockSizeUnit, yStart.A, dto.ZA * Level.BlockSizeUnit),
                    texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
            else if (yStart.A == yEnd.A && yStart.B < yEnd.B)
                AddTriangle(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yStart.B, dto.ZB * Level.BlockSizeUnit),
                    texture, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
            else
                return false; // Not rendered - failed to meet any of the conditions

            return true; // Rendered successfully
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryRenderMiddleWallFace(BlockFaceDTO dto, (int A, int B) yStart, (int A, int B) yEnd, TextureArea defaultTexture)
        {
            bool shouldApplyDefaultTexture = dto.Block.GetFaceTexture(dto.Face) == TextureArea.None && defaultTexture != TextureArea.None;

            if (shouldApplyDefaultTexture)
                dto.Block.SetFaceTexture(dto.Face, defaultTexture);

            TextureArea texture = dto.Block.GetFaceTexture(dto.Face);

            if (yStart.A != yEnd.A && yStart.B != yEnd.B)
                AddQuad(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yStart.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XA * Level.BlockSizeUnit, yStart.A, dto.ZA * Level.BlockSizeUnit),
                    texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            else if (yStart.A != yEnd.A && yStart.B == yEnd.B)
                AddTriangle(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XA * Level.BlockSizeUnit, yStart.A, dto.ZA * Level.BlockSizeUnit),
                    texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), true);
            else if (yStart.A == yEnd.A && yStart.B != yEnd.B)
                AddTriangle(dto.BlockX, dto.BlockZ, dto.Face,
                    new Vector3(dto.XA * Level.BlockSizeUnit, yEnd.A, dto.ZA * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yEnd.B, dto.ZB * Level.BlockSizeUnit),
                    new Vector3(dto.XB * Level.BlockSizeUnit, yStart.B, dto.ZB * Level.BlockSizeUnit),
                    texture, new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), false);
            else
                return false; // Not rendered - failed to meet any of the conditions

            return true; // Rendered successfully
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
            int floorMin = Clicks.FromWorld(block.Floor.Min);
            int ceilingMax = Clicks.FromWorld(block.Ceiling.Max);
            int yClicks = Clicks.FromWorld(y);

            return floorMin <= yClicks && ceilingMax >= yClicks;
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
                    int currentYclick = -Clicks.FromWorld(currentY, RoundingMethod.Integer);

                    if (currentXblock > 0)
                    {
                        Block currentBlock = room.Blocks[currentXblock - 1, currentZblock];

                        if ((Clicks.FromWorld(currentBlock.Floor.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentBlock.Ceiling.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
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

                        if ((Clicks.FromWorld(currentBlock.Floor.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Floor.XpZp, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentBlock.Ceiling.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Ceiling.XpZp, RoundingMethod.Integer)) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall ||
                            (Clicks.FromWorld(nextBlock.Floor.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(nextBlock.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(nextBlock.Ceiling.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(nextBlock.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
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
                    int currentYclick = -Clicks.FromWorld(currentY, RoundingMethod.Integer);

                    if (currentZblock > 0)
                    {
                        var currentBlock = room.Blocks[currentXblock, currentZblock - 1];

                        if ((Clicks.FromWorld(currentBlock.Floor.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentBlock.Ceiling.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
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

                        if ((Clicks.FromWorld(currentBlock.Floor.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Floor.XpZp, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentBlock.Ceiling.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentBlock.Ceiling.XpZp, RoundingMethod.Integer)) / 2 < currentYclick ||
                            currentBlock.Type == BlockType.Wall ||
                            (Clicks.FromWorld(nextBlock.Floor.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(nextBlock.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(nextBlock.Ceiling.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(nextBlock.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
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
