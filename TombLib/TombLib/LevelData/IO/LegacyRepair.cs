using System.Collections.Generic;
using System.Linq;
using TombLib.Utils;

namespace TombLib.LevelData.IO;

internal static class LegacyRepair
{
    public static void SwapFacesWhereApplicable(IEnumerable<Room> rooms, bool doFloor, bool doCeiling)
    {
        foreach (Room room in rooms)
            for (int x = room.LocalArea.X0; x <= room.LocalArea.X1; x++)
                for (int z = room.LocalArea.Y0; z <= room.LocalArea.Y1; z++)
                {
                    if (doFloor)
                    {
                        SwapFloor2FacesWhereApplicable(room, x, z);
                        SwapDiagonalFloor2FacesWhereApplicable(room, x, z);
                    }

                    if (doCeiling)
                    {
                        SwapCeiling2FacesWhereApplicable(room, x, z);
                        SwapDiagonalCeiling2FacesWhereApplicable(room, x, z);
                    }
                }
    }

    /// <summary>
    /// This method swaps vertical floor faces, which were affected by the legacy RoomEdit face priority bug, since it has been fixed with the new RoomGeometry code.
    /// </summary>
    private static void SwapFloor2FacesWhereApplicable(Room room, int x, int z)
    {
        Block block = room.Blocks[x, z];
        Subdivision subdivision = block.ExtraFloorSubdivisions[0]; // This will definitely have an element at 0 in legacy prj2s

        RoomBlockPair
            xn = room.GetBlockTryThroughPortal(x - 1, z),
            xp = room.GetBlockTryThroughPortal(x + 1, z),
            zn = room.GetBlockTryThroughPortal(x, z - 1),
            zp = room.GetBlockTryThroughPortal(x, z + 1);

        if (xn.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] > xn.Block.Ceiling.XpZn || subdivision.Edges[(int)BlockEdge.XnZp] > xn.Block.Ceiling.XpZp)
                block.SetFaceTexture(BlockFace.Wall_NegativeX_FloorSubdivision2, block.GetFaceTexture(BlockFace.Wall_NegativeX_QA));
        }

        if (xp.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XpZn] > xp.Block.Ceiling.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] > xp.Block.Ceiling.XnZp)
                block.SetFaceTexture(BlockFace.Wall_PositiveX_FloorSubdivision2, block.GetFaceTexture(BlockFace.Wall_PositiveX_QA));
        }

        if (zn.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] > zn.Block.Ceiling.XnZp || subdivision.Edges[(int)BlockEdge.XpZn] > zn.Block.Ceiling.XpZp)
                block.SetFaceTexture(BlockFace.Wall_NegativeZ_FloorSubdivision2, block.GetFaceTexture(BlockFace.Wall_NegativeZ_QA));
        }

        if (zp.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZp] > zp.Block.Ceiling.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] > zp.Block.Ceiling.XpZn)
                block.SetFaceTexture(BlockFace.Wall_PositiveZ_FloorSubdivision2, block.GetFaceTexture(BlockFace.Wall_PositiveZ_QA));
        }
    }

    /// <summary>
    /// This method swaps vertical ceiling faces, which were affected by the legacy RoomEdit face priority bug, since it has been fixed with the new RoomGeometry code.
    /// </summary>
    private static void SwapCeiling2FacesWhereApplicable(Room room, int x, int z)
    {
        Block block = room.Blocks[x, z];
        Subdivision subdivision = block.ExtraCeilingSubdivisions[0]; // This will definitely have an element at 0 in legacy prj2s

        RoomBlockPair
            xn = room.GetBlockTryThroughPortal(x - 1, z),
            xp = room.GetBlockTryThroughPortal(x + 1, z),
            zn = room.GetBlockTryThroughPortal(x, z - 1),
            zp = room.GetBlockTryThroughPortal(x, z + 1);

        if (xn.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] < xn.Block.Floor.XpZn || subdivision.Edges[(int)BlockEdge.XnZp] < xn.Block.Floor.XpZp)
                block.SetFaceTexture(BlockFace.Wall_NegativeX_CeilingSubdivision2, block.GetFaceTexture(BlockFace.Wall_NegativeX_WS));
        }

        if (xp.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XpZn] < xp.Block.Floor.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] < xp.Block.Floor.XnZp)
                block.SetFaceTexture(BlockFace.Wall_PositiveX_CeilingSubdivision2, block.GetFaceTexture(BlockFace.Wall_PositiveX_WS));
        }

        if (zn.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] < zn.Block.Floor.XnZp || subdivision.Edges[(int)BlockEdge.XpZn] < zn.Block.Floor.XpZp)
                block.SetFaceTexture(BlockFace.Wall_NegativeZ_CeilingSubdivision2, block.GetFaceTexture(BlockFace.Wall_NegativeZ_WS));
        }

        if (zp.Block != null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZp] < zp.Block.Floor.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] < zp.Block.Floor.XpZn)
                block.SetFaceTexture(BlockFace.Wall_PositiveZ_CeilingSubdivision2, block.GetFaceTexture(BlockFace.Wall_PositiveZ_WS));
        }
    }

    private static void SwapDiagonalFloor2FacesWhereApplicable(Room room, int x, int z)
    {
        Block localBlock = room.Blocks[x, z],
            probingBlock = localBlock;

        if (localBlock.WallPortal != null)
        {
            RoomBlockPair pair = room.GetBlockTryThroughPortal(x, z);

            if (pair.Room != room && pair.Block != null)
                probingBlock = pair.Block;
        }

        if (probingBlock.Floor.DiagonalSplit == DiagonalSplit.None)
            return;

        Subdivision subdivision = localBlock.ExtraFloorSubdivisions.ElementAtOrDefault(0);

        if (subdivision is null)
            return;

        TextureArea
            qaPositiveZ = localBlock.GetFaceTexture(BlockFace.Wall_PositiveZ_QA),
            qaNegativeZ = localBlock.GetFaceTexture(BlockFace.Wall_NegativeZ_QA),
            qaNegativeX = localBlock.GetFaceTexture(BlockFace.Wall_NegativeX_QA),
            qaPositiveX = localBlock.GetFaceTexture(BlockFace.Wall_PositiveX_QA);

        switch (probingBlock.Floor.DiagonalSplit)
        {
            case DiagonalSplit.XnZp:
                if (subdivision.Edges[(int)BlockEdge.XnZn] > localBlock.Floor.XpZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeZ_FloorSubdivision2, qaNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XpZp] > localBlock.Floor.XpZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveX_FloorSubdivision2, qaPositiveX);
                break;

            case DiagonalSplit.XpZn:
                if (subdivision.Edges[(int)BlockEdge.XnZn] > localBlock.Floor.XnZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeX_FloorSubdivision2, qaNegativeX);

                if (subdivision.Edges[(int)BlockEdge.XpZp] > localBlock.Floor.XnZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveZ_FloorSubdivision2, qaPositiveZ);
                break;

            case DiagonalSplit.XpZp:
                if (subdivision.Edges[(int)BlockEdge.XpZn] > localBlock.Floor.XnZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeZ_FloorSubdivision2, qaNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XnZp] > localBlock.Floor.XnZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeX_FloorSubdivision2, qaNegativeX);
                break;

            case DiagonalSplit.XnZn:
                if (subdivision.Edges[(int)BlockEdge.XnZp] > localBlock.Floor.XpZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveZ_FloorSubdivision2, qaPositiveZ);

                if (subdivision.Edges[(int)BlockEdge.XpZn] > localBlock.Floor.XpZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveX_FloorSubdivision2, qaPositiveX);
                break;
        }
    }

    private static void SwapDiagonalCeiling2FacesWhereApplicable(Room room, int x, int z)
    {
        Block localBlock = room.Blocks[x, z],
            probingBlock = localBlock;

        if (localBlock.WallPortal != null)
        {
            RoomBlockPair pair = room.GetBlockTryThroughPortal(x, z);

            if (pair.Room != room && pair.Block != null)
                probingBlock = pair.Block;
        }

        if (probingBlock.Ceiling.DiagonalSplit == DiagonalSplit.None)
            return;

        Subdivision subdivision = localBlock.ExtraCeilingSubdivisions.ElementAtOrDefault(0);

        if (subdivision is null)
            return;

        TextureArea
            wsPositiveZ = localBlock.GetFaceTexture(BlockFace.Wall_PositiveZ_WS),
            wsNegativeZ = localBlock.GetFaceTexture(BlockFace.Wall_NegativeZ_WS),
            wsNegativeX = localBlock.GetFaceTexture(BlockFace.Wall_NegativeX_WS),
            wsPositiveX = localBlock.GetFaceTexture(BlockFace.Wall_PositiveX_WS);

        switch (probingBlock.Ceiling.DiagonalSplit)
        {
            case DiagonalSplit.XnZp:
                if (subdivision.Edges[(int)BlockEdge.XnZn] < localBlock.Ceiling.XpZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeZ_CeilingSubdivision2, wsNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XpZp] < localBlock.Ceiling.XpZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveX_CeilingSubdivision2, wsPositiveX);
                break;

            case DiagonalSplit.XpZn:
                if (subdivision.Edges[(int)BlockEdge.XnZn] < localBlock.Ceiling.XnZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeX_CeilingSubdivision2, wsNegativeX);

                if (subdivision.Edges[(int)BlockEdge.XpZp] < localBlock.Ceiling.XnZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveZ_CeilingSubdivision2, wsPositiveZ);
                break;

            case DiagonalSplit.XpZp:
                if (subdivision.Edges[(int)BlockEdge.XpZn] < localBlock.Ceiling.XnZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeZ_CeilingSubdivision2, wsNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XnZp] < localBlock.Ceiling.XnZn)
                    localBlock.SetFaceTexture(BlockFace.Wall_NegativeX_CeilingSubdivision2, wsNegativeX);
                break;

            case DiagonalSplit.XnZn:
                if (subdivision.Edges[(int)BlockEdge.XnZp] < localBlock.Ceiling.XpZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveZ_CeilingSubdivision2, wsPositiveZ);

                if (subdivision.Edges[(int)BlockEdge.XpZn] < localBlock.Ceiling.XpZp)
                    localBlock.SetFaceTexture(BlockFace.Wall_PositiveX_CeilingSubdivision2, wsPositiveX);
                break;
        }
    }
}
