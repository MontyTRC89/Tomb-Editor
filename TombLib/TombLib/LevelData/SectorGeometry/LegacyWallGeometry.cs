using System.Collections.Generic;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.LevelData.SectorGeometry;

public static class LegacyWallGeometry
{
	public static IReadOnlyList<SectorFaceData> GetVerticalFloorPartFaces(SectorWallData wallData, bool isAnyWall)
	{
		var result = new List<SectorFaceData>();
		bool edVisible = false;

		int yQaA = wallData.QA.StartY,
			yQaB = wallData.QA.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yEdA = wallData.ExtraFloorSplits[0].StartY,
			yEdB = wallData.ExtraFloorSplits[0].EndY,
			yA, yB;

		SectorFace
			qaFace = SectorFaceExtensions.GetQaFace(wallData.Direction),
			edFace = SectorFaceExtensions.GetExtraFloorSplitFace(wallData.Direction, 0);

		// Always check these
		if (yQaA >= yCeilingA && yQaB >= yCeilingB)
		{
			yQaA = yCeilingA;
			yQaB = yCeilingB;
		}

		// Following checks are only for wall's faces
		if (isAnyWall)
		{
			if ((yQaA > yFloorA && yQaB < yFloorB) || (yQaA < yFloorA && yQaB > yFloorB))
			{
				yQaA = yFloorA;
				yQaB = yFloorB;
			}

			if ((yQaA > yCeilingA && yQaB < yCeilingB) || (yQaA < yCeilingA && yQaB > yCeilingB))
			{
				yQaA = yCeilingA;
				yQaB = yCeilingB;
			}
		}

		if (yQaA == yFloorA && yQaB == yFloorB)
			return result; // Empty list

		// Check for extra ED split
		yA = yFloorA;
		yB = yFloorB;

		if (yEdA >= yA && yEdB >= yB && yQaA >= yEdA && yQaB >= yEdB && !(yEdA == yA && yEdB == yB))
		{
			edVisible = true;
			yA = yEdA;
			yB = yEdB;
		}

		SectorFaceData? qaFaceData = SectorFaceData.CreateVerticalFloorFaceData(qaFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yQaA, yQaB), new(yA, yB));

		if (qaFaceData.HasValue)
			result.Add(qaFaceData.Value);

		if (edVisible)
		{
			SectorFaceData? edFaceData = SectorFaceData.CreateVerticalFloorFaceData(edFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yEdA, yEdB), new(yFloorA, yFloorB));

			if (edFaceData.HasValue)
				result.Add(edFaceData.Value);
		}

		return result;
	}

	public static IReadOnlyList<SectorFaceData> GetVerticalCeilingPartFaces(SectorWallData wallData, bool isAnyWall)
	{
		var result = new List<SectorFaceData>();
		bool rfVisible = false;

		int yWsA = wallData.WS.StartY,
			yWsB = wallData.WS.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yRfA = wallData.ExtraCeilingSplits[0].StartY,
			yRfB = wallData.ExtraCeilingSplits[0].EndY,
			yA, yB;

		SectorFace
			wsFace = SectorFaceExtensions.GetWsFace(wallData.Direction),
			rfFace = SectorFaceExtensions.GetExtraCeilingSplitFace(wallData.Direction, 0);

		// Always check these
		if (yWsA <= yFloorA && yWsB <= yFloorB)
		{
			yWsA = yFloorA;
			yWsB = yFloorB;
		}

		// Following checks are only for wall's faces
		if (isAnyWall)
		{
			if ((yWsA > yCeilingA && yWsB < yCeilingB) || (yWsA < yCeilingA && yWsB > yCeilingB))
			{
				yWsA = yCeilingA;
				yWsB = yCeilingB;
			}

			if ((yWsA > yFloorA && yWsB < yFloorB) || (yWsA < yFloorA && yWsB > yFloorB))
			{
				yWsA = yFloorA;
				yWsB = yFloorB;
			}
		}

		if (yWsA == yCeilingA && yWsB == yCeilingB)
			return result; // Empty list

		// Check for extra RF split
		yA = yCeilingA;
		yB = yCeilingB;

		if (yRfA <= yA && yRfB <= yB && yWsA <= yRfA && yWsB <= yRfB && !(yRfA == yA && yRfB == yB))
		{
			rfVisible = true;
			yA = yRfA;
			yB = yRfB;
		}

		SectorFaceData? wsFaceData = SectorFaceData.CreateVerticalCeilingFaceData(wsFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yWsA, yWsB), new(yA, yB));

		if (wsFaceData.HasValue)
			result.Add(wsFaceData.Value);

		if (rfVisible)
		{
			SectorFaceData? rfFaceData = SectorFaceData.CreateVerticalCeilingFaceData(rfFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yRfA, yRfB), new(yCeilingA, yCeilingB));

			if (rfFaceData.HasValue)
				result.Add(rfFaceData.Value);
		}

		return result;
	}

	public static SectorFaceData? GetVerticalMiddlePartFace(SectorWallData wallData)
	{
		int yQaA = wallData.QA.StartY,
			yQaB = wallData.QA.EndY,
			yWsA = wallData.WS.StartY,
			yWsB = wallData.WS.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yA, yB, yC, yD;

		SectorFace middleFace = SectorFaceExtensions.GetMiddleFace(wallData.Direction);

		yA = yWsA >= yCeilingA ? yCeilingA : yWsA;
		yB = yWsB >= yCeilingB ? yCeilingB : yWsB;
		yD = yQaA <= yFloorA ? yFloorA : yQaA;
		yC = yQaB <= yFloorB ? yFloorB : yQaB;

		return SectorFaceData.CreateVerticalMiddleFaceData(middleFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yC, yD), new(yA, yB));
	}
}
