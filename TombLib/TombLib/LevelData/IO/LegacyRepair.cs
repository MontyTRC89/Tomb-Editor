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
        Block block = room.GetBlockTry(x, z);

        if (block is null)
            return;

        Subdivision subdivision = block.ExtraFloorSubdivisions.FirstOrDefault();

        if (subdivision is null)
            return;

        RoomBlockPair
            xn = room.GetBlockTryThroughPortal(x - 1, z),
            xp = room.GetBlockTryThroughPortal(x + 1, z),
            zn = room.GetBlockTryThroughPortal(x, z - 1),
            zp = room.GetBlockTryThroughPortal(x, z + 1);

        if (xn.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] > xn.Block.Ceiling.XpZn || subdivision.Edges[(int)BlockEdge.XnZp] > xn.Block.Ceiling.XpZp)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_FloorSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_QA, FaceLayer.Base)));
        }

        if (xp.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XpZn] > xp.Block.Ceiling.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] > xp.Block.Ceiling.XnZp)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_FloorSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_QA, FaceLayer.Base)));
        }

        if (zn.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] > zn.Block.Ceiling.XnZp || subdivision.Edges[(int)BlockEdge.XpZn] > zn.Block.Ceiling.XpZp)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_FloorSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_QA, FaceLayer.Base)));
        }

        if (zp.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZp] > zp.Block.Ceiling.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] > zp.Block.Ceiling.XpZn)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_FloorSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_QA, FaceLayer.Base)));
        }
    }

    /// <summary>
    /// This method swaps vertical ceiling faces, which were affected by the legacy RoomEdit face priority bug, since it has been fixed with the new RoomGeometry code.
    /// </summary>
    private static void SwapCeiling2FacesWhereApplicable(Room room, int x, int z)
    {
        Block block = room.GetBlockTry(x, z);

        if (block is null)
            return;

        Subdivision subdivision = block.ExtraCeilingSubdivisions.FirstOrDefault();

        if (subdivision is null)
            return;

        RoomBlockPair
            xn = room.GetBlockTryThroughPortal(x - 1, z),
            xp = room.GetBlockTryThroughPortal(x + 1, z),
            zn = room.GetBlockTryThroughPortal(x, z - 1),
            zp = room.GetBlockTryThroughPortal(x, z + 1);

        if (xn.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] < xn.Block.Floor.XpZn || subdivision.Edges[(int)BlockEdge.XnZp] < xn.Block.Floor.XpZp)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_CeilingSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_WS, FaceLayer.Base)));
        }

        if (xp.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XpZn] < xp.Block.Floor.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] < xp.Block.Floor.XnZp)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_CeilingSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_WS, FaceLayer.Base)));
        }

        if (zn.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZn] < zn.Block.Floor.XnZp || subdivision.Edges[(int)BlockEdge.XpZn] < zn.Block.Floor.XpZp)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_CeilingSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_WS, FaceLayer.Base)));
        }

        if (zp.Block is not null)
        {
            if (subdivision.Edges[(int)BlockEdge.XnZp] < zp.Block.Floor.XnZn || subdivision.Edges[(int)BlockEdge.XpZp] < zp.Block.Floor.XpZn)
                block.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_CeilingSubdivision2, FaceLayer.Base), block.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_WS, FaceLayer.Base)));
        }
    }

    private static void SwapDiagonalFloor2FacesWhereApplicable(Room room, int x, int z)
    {
        Block localBlock = room.GetBlockTry(x, z),
            probingBlock = localBlock;

        if (localBlock is null)
            return;

        if (localBlock.WallPortal is not null)
        {
            RoomBlockPair pair = room.GetBlockTryThroughPortal(x, z);

            if (pair.Room != room && pair.Block is not null)
                probingBlock = pair.Block;
        }

        if (probingBlock.Floor.DiagonalSplit is DiagonalSplit.None)
            return;

        Subdivision subdivision = localBlock.ExtraFloorSubdivisions.FirstOrDefault();

        if (subdivision is null)
            return;

        TextureArea
            qaPositiveZ = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_QA, FaceLayer.Base)),
            qaNegativeZ = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_QA, FaceLayer.Base)),
            qaNegativeX = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_QA, FaceLayer.Base)),
            qaPositiveX = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_QA, FaceLayer.Base));

        switch (probingBlock.Floor.DiagonalSplit)
        {
            case DiagonalSplit.XnZp:
                if (subdivision.Edges[(int)BlockEdge.XnZn] > localBlock.Floor.XpZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_FloorSubdivision2, FaceLayer.Base), qaNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XpZp] > localBlock.Floor.XpZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_FloorSubdivision2, FaceLayer.Base), qaPositiveX);
                break;

            case DiagonalSplit.XpZn:
                if (subdivision.Edges[(int)BlockEdge.XnZn] > localBlock.Floor.XnZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_FloorSubdivision2, FaceLayer.Base), qaNegativeX);

                if (subdivision.Edges[(int)BlockEdge.XpZp] > localBlock.Floor.XnZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_FloorSubdivision2, FaceLayer.Base), qaPositiveZ);
                break;

            case DiagonalSplit.XpZp:
                if (subdivision.Edges[(int)BlockEdge.XpZn] > localBlock.Floor.XnZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_FloorSubdivision2, FaceLayer.Base), qaNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XnZp] > localBlock.Floor.XnZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_FloorSubdivision2, FaceLayer.Base), qaNegativeX);
                break;

            case DiagonalSplit.XnZn:
                if (subdivision.Edges[(int)BlockEdge.XnZp] > localBlock.Floor.XpZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_FloorSubdivision2, FaceLayer.Base), qaPositiveZ);

                if (subdivision.Edges[(int)BlockEdge.XpZn] > localBlock.Floor.XpZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_FloorSubdivision2, FaceLayer.Base), qaPositiveX);
                break;
        }
    }

    private static void SwapDiagonalCeiling2FacesWhereApplicable(Room room, int x, int z)
    {
        Block localBlock = room.GetBlockTry(x, z),
            probingBlock = localBlock;

        if (localBlock is null)
            return;

        if (localBlock.WallPortal is not null)
        {
            RoomBlockPair pair = room.GetBlockTryThroughPortal(x, z);

            if (pair.Room != room && pair.Block is not null)
                probingBlock = pair.Block;
        }

        if (probingBlock.Ceiling.DiagonalSplit is DiagonalSplit.None)
            return;

        Subdivision subdivision = localBlock.ExtraCeilingSubdivisions.FirstOrDefault();

        if (subdivision is null)
            return;

        TextureArea
            wsPositiveZ = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_WS, FaceLayer.Base)),
            wsNegativeZ = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_WS, FaceLayer.Base)),
            wsNegativeX = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_WS, FaceLayer.Base)),
            wsPositiveX = localBlock.GetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_WS, FaceLayer.Base));

        switch (probingBlock.Ceiling.DiagonalSplit)
        {
            case DiagonalSplit.XnZp:
                if (subdivision.Edges[(int)BlockEdge.XnZn] < localBlock.Ceiling.XpZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_CeilingSubdivision2, FaceLayer.Base), wsNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XpZp] < localBlock.Ceiling.XpZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_CeilingSubdivision2, FaceLayer.Base), wsPositiveX);
                break;

            case DiagonalSplit.XpZn:
                if (subdivision.Edges[(int)BlockEdge.XnZn] < localBlock.Ceiling.XnZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_CeilingSubdivision2, FaceLayer.Base), wsNegativeX);

                if (subdivision.Edges[(int)BlockEdge.XpZp] < localBlock.Ceiling.XnZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_CeilingSubdivision2, FaceLayer.Base), wsPositiveZ);
                break;

            case DiagonalSplit.XpZp:
                if (subdivision.Edges[(int)BlockEdge.XpZn] < localBlock.Ceiling.XnZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeZ_CeilingSubdivision2, FaceLayer.Base), wsNegativeZ);

                if (subdivision.Edges[(int)BlockEdge.XnZp] < localBlock.Ceiling.XnZn)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_NegativeX_CeilingSubdivision2, FaceLayer.Base), wsNegativeX);
                break;

            case DiagonalSplit.XnZn:
                if (subdivision.Edges[(int)BlockEdge.XnZp] < localBlock.Ceiling.XpZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveZ_CeilingSubdivision2, FaceLayer.Base), wsPositiveZ);

                if (subdivision.Edges[(int)BlockEdge.XpZn] < localBlock.Ceiling.XpZp)
                    localBlock.SetFaceTexture(new FaceLayerInfo(BlockFace.Wall_PositiveX_CeilingSubdivision2, FaceLayer.Base), wsPositiveX);
                break;
        }
    }
}
