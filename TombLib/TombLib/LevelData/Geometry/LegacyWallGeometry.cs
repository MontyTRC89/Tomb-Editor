using System.Collections.Generic;

namespace TombLib.LevelData.Geometry;

public static class LegacyWallGeometry
{
	public static IReadOnlyList<SectorFace> GetVerticalFloorPartFaces(SectorWall wallData, bool isAnyWall)
	{
		var result = new List<SectorFace>();
		bool subdivide = false;

		int yQaA = wallData.QA.StartY,
			yQaB = wallData.QA.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yEdA = wallData.ExtraFloorSubdivisions[0].StartY,
			yEdB = wallData.ExtraFloorSubdivisions[0].EndY,
			yA, yB;

		BlockFace
			qaFace = BlockFaceExtensions.GetQaFace(wallData.Direction),
			edFace = BlockFaceExtensions.GetExtraFloorSubdivisionFace(wallData.Direction, 0);

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

		// Check for subdivision
		yA = yFloorA;
		yB = yFloorB;

		if (yEdA >= yA && yEdB >= yB && yQaA >= yEdA && yQaB >= yEdB && !(yEdA == yA && yEdB == yB))
		{
			subdivide = true;
			yA = yEdA;
			yB = yEdB;
		}

		SectorFace? qaFaceData = SectorFace.CreateVerticalFloorFaceData(qaFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yQaA, yQaB), new(yA, yB));

		if (qaFaceData.HasValue)
			result.Add(qaFaceData.Value);

		if (subdivide)
		{
			SectorFace? edFaceData = SectorFace.CreateVerticalFloorFaceData(edFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yEdA, yEdB), new(yFloorA, yFloorB));

			if (edFaceData.HasValue)
				result.Add(edFaceData.Value);
		}

		return result;
	}

	public static IReadOnlyList<SectorFace> GetVerticalCeilingPartFaces(SectorWall wallData, bool isAnyWall)
	{
		var result = new List<SectorFace>();
		bool subdivide = false;

		int yWsA = wallData.WS.StartY,
			yWsB = wallData.WS.EndY,
			yFloorA = wallData.Start.MinY,
			yFloorB = wallData.End.MinY,
			yCeilingA = wallData.Start.MaxY,
			yCeilingB = wallData.End.MaxY,
			yRfA = wallData.ExtraCeilingSubdivisions[0].StartY,
			yRfB = wallData.ExtraCeilingSubdivisions[0].EndY,
			yA, yB;

		BlockFace
			wsFace = BlockFaceExtensions.GetWsFace(wallData.Direction),
			rfFace = BlockFaceExtensions.GetExtraCeilingSubdivisionFace(wallData.Direction, 0);

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

		// Check for subdivision
		yA = yCeilingA;
		yB = yCeilingB;

		if (yRfA <= yA && yRfB <= yB && yWsA <= yRfA && yWsB <= yRfB && !(yRfA == yA && yRfB == yB))
		{
			subdivide = true;
			yA = yRfA;
			yB = yRfB;
		}

		SectorFace? wsFaceData = SectorFace.CreateVerticalCeilingFaceData(wsFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yWsA, yWsB), new(yA, yB));

		if (wsFaceData.HasValue)
			result.Add(wsFaceData.Value);

		if (subdivide)
		{
			SectorFace? rfFaceData = SectorFace.CreateVerticalCeilingFaceData(rfFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yRfA, yRfB), new(yCeilingA, yCeilingB));

			if (rfFaceData.HasValue)
				result.Add(rfFaceData.Value);
		}

		return result;
	}

	public static SectorFace? GetVerticalMiddlePartFace(SectorWall wallData)
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

		BlockFace middleFace = BlockFaceExtensions.GetMiddleFace(wallData.Direction);

		yA = yWsA >= yCeilingA ? yCeilingA : yWsA;
		yB = yWsB >= yCeilingB ? yCeilingB : yWsB;
		yD = yQaA <= yFloorA ? yFloorA : yQaA;
		yC = yQaB <= yFloorB ? yFloorB : yQaB;

		return SectorFace.CreateVerticalMiddleFaceData(middleFace, (wallData.Start.X, wallData.Start.Z), (wallData.End.X, wallData.End.Z), new(yC, yD), new(yA, yB));
	}
}
