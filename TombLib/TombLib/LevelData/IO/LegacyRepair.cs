using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
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
        Sector sector = room.GetSectorTry(x, z);

        if (sector is null)
            return;

        SectorSplit split = sector.ExtraFloorSplits.FirstOrDefault();

        if (split is null)
            return;

        RoomSectorPair
            xn = room.GetSectorTryThroughPortal(x - 1, z),
            xp = room.GetSectorTryThroughPortal(x + 1, z),
            zn = room.GetSectorTryThroughPortal(x, z - 1),
            zp = room.GetSectorTryThroughPortal(x, z + 1);

        if (xn.Sector is not null)
        {
            if (split.XnZn > xn.Sector.Ceiling.XpZn || split.XnZp > xn.Sector.Ceiling.XpZp)
                sector.SetFaceTexture(new(SectorFace.Wall_NegativeX_Floor2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_NegativeX_QA, FaceLayer.Base)));
        }

        if (xp.Sector is not null)
        {
            if (split.XpZn > xp.Sector.Ceiling.XnZn || split.XpZp > xp.Sector.Ceiling.XnZp)
                sector.SetFaceTexture(new(SectorFace.Wall_PositiveX_Floor2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_PositiveX_QA, FaceLayer.Base)));
        }

        if (zn.Sector is not null)
        {
            if (split.XnZn > zn.Sector.Ceiling.XnZp || split.XpZn > zn.Sector.Ceiling.XpZp)
                sector.SetFaceTexture(new(SectorFace.Wall_NegativeZ_Floor2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_NegativeZ_QA, FaceLayer.Base)));
        }

        if (zp.Sector is not null)
        {
            if (split.XnZp > zp.Sector.Ceiling.XnZn || split.XpZp > zp.Sector.Ceiling.XpZn)
                sector.SetFaceTexture(new(SectorFace.Wall_PositiveZ_Floor2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_PositiveZ_QA, FaceLayer.Base)));
        }
    }

    /// <summary>
    /// This method swaps vertical ceiling faces, which were affected by the legacy RoomEdit face priority bug, since it has been fixed with the new RoomGeometry code.
    /// </summary>
    private static void SwapCeiling2FacesWhereApplicable(Room room, int x, int z)
    {
        Sector sector = room.GetSectorTry(x, z);

        if (sector is null)
            return;

        SectorSplit split = sector.ExtraCeilingSplits.FirstOrDefault();

        if (split is null)
            return;

        RoomSectorPair
            xn = room.GetSectorTryThroughPortal(x - 1, z),
            xp = room.GetSectorTryThroughPortal(x + 1, z),
            zn = room.GetSectorTryThroughPortal(x, z - 1),
            zp = room.GetSectorTryThroughPortal(x, z + 1);

        if (xn.Sector is not null)
        {
            if (split.XnZn < xn.Sector.Floor.XpZn || split.XnZp < xn.Sector.Floor.XpZp)
                sector.SetFaceTexture(new(SectorFace.Wall_NegativeX_Ceiling2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_NegativeX_WS, FaceLayer.Base)));
        }

        if (xp.Sector is not null)
        {
            if (split.XpZn < xp.Sector.Floor.XnZn || split.XpZp < xp.Sector.Floor.XnZp)
                sector.SetFaceTexture(new(SectorFace.Wall_PositiveX_Ceiling2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_PositiveX_WS, FaceLayer.Base)));
        }

        if (zn.Sector is not null)
        {
            if (split.XnZn < zn.Sector.Floor.XnZp || split.XpZn < zn.Sector.Floor.XpZp)
                sector.SetFaceTexture(new(SectorFace.Wall_NegativeZ_Ceiling2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_NegativeZ_WS, FaceLayer.Base)));
        }

        if (zp.Sector is not null)
        {
            if (split.XnZp < zp.Sector.Floor.XnZn || split.XpZp < zp.Sector.Floor.XpZn)
                sector.SetFaceTexture(new(SectorFace.Wall_PositiveZ_Ceiling2, FaceLayer.Base), sector.GetFaceTexture(new(SectorFace.Wall_PositiveZ_WS, FaceLayer.Base)));
        }
    }

    private static void SwapDiagonalFloor2FacesWhereApplicable(Room room, int x, int z)
    {
        Sector localSector = room.GetSectorTry(x, z),
            probingSector = localSector;

        if (localSector is null)
            return;

        if (localSector.WallPortal is not null)
        {
            RoomSectorPair pair = room.GetSectorTryThroughPortal(x, z);

            if (pair.Room != room && pair.Sector is not null)
                probingSector = pair.Sector;
        }

        if (probingSector.Floor.DiagonalSplit is DiagonalSplit.None)
            return;

        SectorSplit split = localSector.ExtraFloorSplits.FirstOrDefault();

        if (split is null)
            return;

        TextureArea
            qaPositiveZ = localSector.GetFaceTexture(new(SectorFace.Wall_PositiveZ_QA, FaceLayer.Base)),
            qaNegativeZ = localSector.GetFaceTexture(new(SectorFace.Wall_NegativeZ_QA, FaceLayer.Base)),
            qaNegativeX = localSector.GetFaceTexture(new(SectorFace.Wall_NegativeX_QA, FaceLayer.Base)),
            qaPositiveX = localSector.GetFaceTexture(new(SectorFace.Wall_PositiveX_QA, FaceLayer.Base));

        switch (probingSector.Floor.DiagonalSplit)
        {
            case DiagonalSplit.XnZp:
                if (split.XnZn > localSector.Floor.XpZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeZ_Floor2, FaceLayer.Base), qaNegativeZ);

                if (split.XpZp > localSector.Floor.XpZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveX_Floor2, FaceLayer.Base), qaPositiveX);
                break;

            case DiagonalSplit.XpZn:
                if (split.XnZn > localSector.Floor.XnZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeX_Floor2, FaceLayer.Base), qaNegativeX);

                if (split.XpZp > localSector.Floor.XnZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveZ_Floor2, FaceLayer.Base), qaPositiveZ);
                break;

            case DiagonalSplit.XpZp:
                if (split.XpZn > localSector.Floor.XnZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeZ_Floor2, FaceLayer.Base), qaNegativeZ);

                if (split.XnZp > localSector.Floor.XnZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeX_Floor2, FaceLayer.Base), qaNegativeX);
                break;

            case DiagonalSplit.XnZn:
                if (split.XnZp > localSector.Floor.XpZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveZ_Floor2, FaceLayer.Base), qaPositiveZ);

                if (split.XpZn > localSector.Floor.XpZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveX_Floor2, FaceLayer.Base), qaPositiveX);
                break;
        }
    }

    private static void SwapDiagonalCeiling2FacesWhereApplicable(Room room, int x, int z)
    {
        Sector localSector = room.GetSectorTry(x, z),
            probingSector = localSector;

        if (localSector is null)
            return;

        if (localSector.WallPortal is not null)
        {
            RoomSectorPair pair = room.GetSectorTryThroughPortal(x, z);

            if (pair.Room != room && pair.Sector is not null)
                probingSector = pair.Sector;
        }

        if (probingSector.Ceiling.DiagonalSplit is DiagonalSplit.None)
            return;

        SectorSplit split = localSector.ExtraCeilingSplits.FirstOrDefault();

        if (split is null)
            return;

        TextureArea
            wsPositiveZ = localSector.GetFaceTexture(new(SectorFace.Wall_PositiveZ_WS, FaceLayer.Base)),
            wsNegativeZ = localSector.GetFaceTexture(new(SectorFace.Wall_NegativeZ_WS, FaceLayer.Base)),
            wsNegativeX = localSector.GetFaceTexture(new(SectorFace.Wall_NegativeX_WS, FaceLayer.Base)),
            wsPositiveX = localSector.GetFaceTexture(new(SectorFace.Wall_PositiveX_WS, FaceLayer.Base));

        switch (probingSector.Ceiling.DiagonalSplit)
        {
            case DiagonalSplit.XnZp:
                if (split.XnZn < localSector.Ceiling.XpZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeZ_Ceiling2, FaceLayer.Base), wsNegativeZ);

                if (split.XpZp < localSector.Ceiling.XpZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveX_Ceiling2, FaceLayer.Base), wsPositiveX);
                break;

            case DiagonalSplit.XpZn:
                if (split.XnZn < localSector.Ceiling.XnZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeX_Ceiling2, FaceLayer.Base), wsNegativeX);

                if (split.XpZp < localSector.Ceiling.XnZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveZ_Ceiling2, FaceLayer.Base), wsPositiveZ);
                break;

            case DiagonalSplit.XpZp:
                if (split.XpZn < localSector.Ceiling.XnZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeZ_Ceiling2, FaceLayer.Base), wsNegativeZ);

                if (split.XnZp < localSector.Ceiling.XnZn)
                    localSector.SetFaceTexture(new(SectorFace.Wall_NegativeX_Ceiling2, FaceLayer.Base), wsNegativeX);
                break;

            case DiagonalSplit.XnZn:
                if (split.XnZp < localSector.Ceiling.XpZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveZ_Ceiling2, FaceLayer.Base), wsPositiveZ);

                if (split.XpZn < localSector.Ceiling.XpZp)
                    localSector.SetFaceTexture(new(SectorFace.Wall_PositiveX_Ceiling2, FaceLayer.Base), wsPositiveX);
                break;
        }
    }
}
