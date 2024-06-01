using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.LevelData.Geometry;

/// <summary>
/// Represents the wall of a single sector.
/// </summary>
public struct SectorWall
{
	/// <summary>
	/// The direction the wall is facing.
	/// </summary>
	public Direction Direction;

	/// <summary>
	/// The (X, Z) position and the minimum and maximum Y coordinate of the start corner of the wall.
	/// </summary>
	public WallEnd Start;

	/// <summary>
	/// The (X, Z) position and the minimum and maximum Y coordinate of the end corner of the wall.
	/// </summary>
	public WallEnd End;

	/// <summary>
	/// Main floor split of the wall.
	/// </summary>
	public WallSplit QA;

	/// <summary>
	/// Main ceiling split of the wall.
	/// </summary>
	public WallSplit WS;

	/// <summary>
	/// Extra floor subdivisions of the wall.
	/// </summary>
	public List<WallSplit> ExtraFloorSubdivisions;

	/// <summary>
	/// Extra ceiling subdivisions of the wall.
	/// </summary>
	public List<WallSplit> ExtraCeilingSubdivisions;

	public SectorWall() // TODO: Add a proper ctor!!!
	{
		Direction = Direction.None;
		Start = new();
		End = new();
		QA = new();
		WS = new();
		ExtraFloorSubdivisions = new();
		ExtraCeilingSubdivisions = new();
	}

	/// <summary>
	/// Returns true if the QA part of the wall is fully above the maximum Y coordinate of the wall.
	/// </summary>
	public readonly bool IsQaFullyAboveMaxY => QA.StartY >= Start.MaxY && QA.EndY >= End.MaxY;

	/// <summary>
	/// Returns true if the WS part of the wall is fully below the minimum Y coordinate of the wall.
	/// </summary>
	public readonly bool IsWsFullyBelowMinY => WS.StartY <= Start.MinY && WS.EndY <= End.MinY;

	/// <summary>
	/// Returns true if the diagonal wall direction can have a non-diagonal (square from top-down) floor part.
	/// </summary>
	public readonly bool CanHaveNonDiagonalFloorPart(DiagonalSplit diagonalFloorSplitOfBlock)
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
	public readonly bool CanHaveNonDiagonalCeilingPart(DiagonalSplit diagonalCeilingSplitOfBlock)
	{
		return
			(diagonalCeilingSplitOfBlock is DiagonalSplit.XnZp && Direction is Direction.NegativeZ or Direction.PositiveX) ||
			(diagonalCeilingSplitOfBlock is DiagonalSplit.XpZn && Direction is Direction.NegativeX or Direction.PositiveZ) ||
			(diagonalCeilingSplitOfBlock is DiagonalSplit.XpZp && Direction is Direction.NegativeZ or Direction.NegativeX) ||
			(diagonalCeilingSplitOfBlock is DiagonalSplit.XnZn && Direction is Direction.PositiveZ or Direction.PositiveX);
	}

	public readonly IReadOnlyList<SectorFace> GetVerticalFloorPartFaces(DiagonalSplit diagonalFloorSplit, bool isAnyWall)
	{
		static SectorFace? CreateFaceData(BlockFace blockFace, (int X, int Z) wallStart, (int X, int Z) wallEnd, (int StartY, int EndY) faceStart, (int StartY, int EndY) faceEnd)
		{
			if (faceStart.StartY > faceEnd.StartY && faceStart.EndY > faceEnd.EndY) // Is quad
			{
				return new SectorFace(blockFace,
					p0: new Vector3(wallStart.X * Level.BlockSizeUnit, faceStart.StartY, wallStart.Z * Level.BlockSizeUnit),
					p1: new Vector3(wallEnd.X * Level.BlockSizeUnit, faceStart.EndY, wallEnd.Z * Level.BlockSizeUnit),
					p2: new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
					p3: new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
					uv0: new Vector2(0, 0), uv1: new Vector2(1, 0), uv2: new Vector2(1, 1), uv3: new Vector2(0, 1));
			}
			else if (faceStart.StartY == faceEnd.StartY && faceStart.EndY > faceEnd.EndY) // Is triangle (type 1)
			{
				return new SectorFace(blockFace,
					p0: new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
					p1: new Vector3(wallEnd.X * Level.BlockSizeUnit, faceStart.EndY, wallEnd.Z * Level.BlockSizeUnit),
					p2: new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
					uv0: new Vector2(1, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: false);
			}
			else if (faceStart.StartY > faceEnd.StartY && faceStart.EndY == faceEnd.EndY) // Is triangle (type 2)
			{
				return new SectorFace(blockFace,
					p0: new Vector3(wallStart.X * Level.BlockSizeUnit, faceStart.StartY, wallStart.Z * Level.BlockSizeUnit),
					p1: new Vector3(wallEnd.X * Level.BlockSizeUnit, faceEnd.EndY, wallEnd.Z * Level.BlockSizeUnit),
					p2: new Vector3(wallStart.X * Level.BlockSizeUnit, faceEnd.StartY, wallStart.Z * Level.BlockSizeUnit),
					uv0: new Vector2(0, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: true);
			}
			else
			{
				return null; // Not rendered - failed to meet any of the conditions
			}
		}

		bool isQaFullyAboveCeiling = IsQaFullyAboveMaxY; // Technically should be classified as a wall if true
		bool canHaveDiagonalWallFloorPart = CanHaveNonDiagonalFloorPart(diagonalFloorSplit); // The wall bit under the flat floor triangle of a diagonal wall

		var faces = new List<SectorFace>();
		SectorFace? faceData;

		for (int extraSubdivisionIndex = -1; extraSubdivisionIndex < ExtraFloorSubdivisions.Count; extraSubdivisionIndex++)
		{
			(int yStartA, int yStartB) = extraSubdivisionIndex == -1
				? (QA.StartY, QA.EndY) // Render QA face
				: (ExtraFloorSubdivisions[extraSubdivisionIndex].StartY, ExtraFloorSubdivisions[extraSubdivisionIndex].EndY); // Render subdivision face

			BlockFace blockFace = extraSubdivisionIndex == -1
				? BlockFaceExtensions.GetQaFace(Direction) // QA face
				: BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction, extraSubdivisionIndex); // Subdivision face

			// Start with the floor as a baseline for the bottom end of the face
			(int yEndA, int yEndB) = (Start.MinY, End.MinY);

			if (extraSubdivisionIndex + 1 < ExtraFloorSubdivisions.Count) // If a next floor subdivision exists
			{
				int yNextSubdivStart = ExtraFloorSubdivisions[extraSubdivisionIndex + 1].StartY,
					yNextSubdivEnd = ExtraFloorSubdivisions[extraSubdivisionIndex + 1].EndY;

				if ((canHaveDiagonalWallFloorPart || isQaFullyAboveCeiling) && (yNextSubdivStart > QA.StartY || yNextSubdivEnd > QA.EndY))
					continue; // Skip it, since it's above the flat, walkable triangle of a diagonal wall

				if (yNextSubdivStart >= Start.MinY && yNextSubdivEnd >= End.MinY) // If next subdivision is NOT in void below floor
				{
					// Make the next subdivision the bottom end of the face
					yEndA = yNextSubdivStart;
					yEndB = yNextSubdivEnd;
				}
			}

			if (yStartA <= yEndA && yStartB <= yEndB)
				continue; // 0 or negative height subdivision, don't render it

			faceData = CreateFaceData(blockFace, (Start.X, Start.Z), (End.X, End.Z), (yStartA, yStartB), (yEndA, yEndB));

			if (!faceData.HasValue)
			{
				// Try overdraw

				bool isQA = yStartA == QA.StartY && yStartB == QA.EndY;
				bool isValidOverdraw = isQA && (!isAnyWall || canHaveDiagonalWallFloorPart);

				if (!isValidOverdraw)
					continue;

				// Find lowest point between subdivision and baseline, then try and create an overdraw face out of it
				int lowest = Math.Min(Math.Min(yStartA, yStartB), Math.Min(yEndA, yEndB));
				faceData = CreateFaceData(blockFace, (Start.X, Start.Z), (End.X, End.Z), (yStartA, yStartB), (lowest, lowest));
			}

			if (faceData.HasValue)
				faces.Add(faceData.Value);
		}

		return faces;
	}

	public readonly IReadOnlyList<SectorFace> GetVerticalCeilingPartFaces(DiagonalSplit diagonalCeilingSplit, bool isAnyWall)
	{
		static SectorFace? CreateFaceData(BlockFace blockFace, (int X, int Z) wallStartPoint, (int X, int Z) wallEndPoint, WallSplit faceStartSplit, WallSplit faceEndSplit)
		{
			if (faceStartSplit.StartY < faceEndSplit.StartY && faceStartSplit.EndY < faceEndSplit.EndY) // Is quad
			{
				return new SectorFace(blockFace,
					p0: new Vector3(wallStartPoint.X * Level.BlockSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.BlockSizeUnit),
					p1: new Vector3(wallEndPoint.X * Level.BlockSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.BlockSizeUnit),
					p2: new Vector3(wallEndPoint.X * Level.BlockSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.BlockSizeUnit),
					p3: new Vector3(wallStartPoint.X * Level.BlockSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.BlockSizeUnit),
					uv0: new Vector2(0, 0), uv1: new Vector2(1, 0), uv2: new Vector2(1, 1), uv3: new Vector2(0, 1));
			}
			else if (faceStartSplit.StartY < faceEndSplit.StartY && faceStartSplit.EndY == faceEndSplit.EndY) // Is triangle (type 1)
			{
				return new SectorFace(blockFace,
					p0: new Vector3(wallStartPoint.X * Level.BlockSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.BlockSizeUnit),
					p1: new Vector3(wallEndPoint.X * Level.BlockSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.BlockSizeUnit),
					p2: new Vector3(wallStartPoint.X * Level.BlockSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.BlockSizeUnit),
					uv0: new Vector2(0, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: true);
			}
			else if (faceStartSplit.StartY == faceEndSplit.StartY && faceStartSplit.EndY < faceEndSplit.EndY) // Is triangle (type 2)
			{
				return new SectorFace(blockFace,
					p0: new Vector3(wallStartPoint.X * Level.BlockSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.BlockSizeUnit),
					p1: new Vector3(wallEndPoint.X * Level.BlockSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.BlockSizeUnit),
					p2: new Vector3(wallEndPoint.X * Level.BlockSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.BlockSizeUnit),
					uv0: new Vector2(1, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: false);
			}
			else
			{
				return null; // Not rendered - failed to meet any of the conditions
			}
		}

		bool isWsFullyBelowMinY = IsWsFullyBelowMinY; // Technically should be classified as a wall if true
		bool canHaveNonDiagonalCeilingPart = CanHaveNonDiagonalCeilingPart(diagonalCeilingSplit); // The wall bit over the flat ceiling triangle of a diagonal wall

		var faces = new List<SectorFace>();
		SectorFace? faceData;

		// Render subdivision faces
		for (int extraSubdivisionIndex = -1; extraSubdivisionIndex < ExtraCeilingSubdivisions.Count; extraSubdivisionIndex++)
		{
			(int yStartA, int yStartB) = extraSubdivisionIndex == -1
				? (WS.StartY, WS.EndY) // Render WS face
				: (ExtraCeilingSubdivisions[extraSubdivisionIndex].StartY, ExtraCeilingSubdivisions[extraSubdivisionIndex].EndY); // Render subdivision face

			BlockFace blockFace = extraSubdivisionIndex == -1
				? BlockFaceExtensions.GetWsFace(Direction)
				: BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction, extraSubdivisionIndex);

			// Start with the ceiling as a baseline for the top end of the face
			(int yEndA, int yEndB) = (Start.MaxY, End.MaxY);

			if (extraSubdivisionIndex + 1 < ExtraCeilingSubdivisions.Count) // If a next ceiling subdivision exists
			{
				int yNextSubdivA = ExtraCeilingSubdivisions[extraSubdivisionIndex + 1].StartY,
					yNextSubdivB = ExtraCeilingSubdivisions[extraSubdivisionIndex + 1].EndY;

				if ((canHaveNonDiagonalCeilingPart || isWsFullyBelowMinY) && (yNextSubdivA < WS.StartY || yNextSubdivB < WS.EndY))
					continue; // Skip it, since it's below the flat ceiling triangle

				if (yNextSubdivA <= Start.MaxY && yNextSubdivB <= End.MaxY) // If next subdivision is NOT in void above ceiling
				{
					// Make the next subdivision the top end of the face
					yEndA = yNextSubdivA;
					yEndB = yNextSubdivB;
				}
			}

			if (yStartA >= yEndA && yStartB >= yEndB)
				continue; // 0 or negative height subdivision, don't render it

			faceData = CreateFaceData(blockFace, (Start.X, Start.Z), (End.X, End.Z), new(yStartA, yStartB), new(yEndA, yEndB));

			if (faceData is null)
			{
				// Try overdraw

				bool isWS = yStartA == WS.StartY && yStartB == WS.EndY;
				bool isValidOverdraw = isWS && (!isAnyWall || canHaveNonDiagonalCeilingPart);

				if (!isValidOverdraw)
					continue;

				// Find highest point between subdivision and baseline, then try and create an overdraw face out of it
				int highest = Math.Max(Math.Max(yStartA, yStartB), Math.Max(yEndA, yEndB));
				faceData = CreateFaceData(blockFace, (Start.X, Start.Z), (End.X, End.Z), new(highest, highest), new(yStartA, yStartB));
			}

			if (faceData.HasValue)
				faces.Add(faceData.Value);
		}

		return faces;
	}

	public readonly SectorFace? GetVerticalMiddleFace()
	{
		WallSplit qa = QA, ws = WS;

		if (qa.StartY < Start.MinY || qa.EndY < End.MinY)
		{
			qa.StartY = Start.MinY;
			qa.EndY = End.MinY;
		}

		if (ws.StartY > Start.MaxY || ws.EndY > End.MaxY)
		{
			ws.StartY = Start.MaxY;
			ws.EndY = End.MaxY;
		}

		int yStartA = qa.StartY <= Start.MinY ? Start.MinY : qa.StartY,
			yStartB = qa.EndY <= End.MinY ? End.MinY : qa.EndY,
			yEndA = ws.StartY >= Start.MaxY ? Start.MaxY : ws.StartY,
			yEndB = ws.EndY >= End.MaxY ? End.MaxY : ws.EndY;

		BlockFace blockFace = BlockFaceExtensions.GetMiddleFace(Direction);

		if (yStartA != yEndA && yStartB != yEndB) // Is quad
		{
			return new SectorFace(blockFace,
				p0: new Vector3(Start.X * Level.BlockSizeUnit, yEndA, Start.Z * Level.BlockSizeUnit),
				p1: new Vector3(End.X * Level.BlockSizeUnit, yEndB, End.Z * Level.BlockSizeUnit),
				p2: new Vector3(End.X * Level.BlockSizeUnit, yStartB, End.Z * Level.BlockSizeUnit),
				p3: new Vector3(Start.X * Level.BlockSizeUnit, yStartA, Start.Z * Level.BlockSizeUnit),
				uv0: new Vector2(0, 0), uv1: new Vector2(1, 0), uv2: new Vector2(1, 1), uv3: new Vector2(0, 1));
		}
		else if (yStartA != yEndA && yStartB == yEndB) // Is triangle (type 1)
		{
			return new SectorFace(blockFace,
				p0: new Vector3(Start.X * Level.BlockSizeUnit, yEndA, Start.Z * Level.BlockSizeUnit),
				p1: new Vector3(End.X * Level.BlockSizeUnit, yEndB, End.Z * Level.BlockSizeUnit),
				p2: new Vector3(Start.X * Level.BlockSizeUnit, yStartB, Start.Z * Level.BlockSizeUnit),
				uv0: new Vector2(0, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: true);
		}
		else if (yStartA == yEndA && yStartB != yEndB) // Is triangle (type 2)
		{
			return new SectorFace(blockFace,
				p0: new Vector3(Start.X * Level.BlockSizeUnit, yEndA, Start.Z * Level.BlockSizeUnit),
				p1: new Vector3(End.X * Level.BlockSizeUnit, yEndB, End.Z * Level.BlockSizeUnit),
				p2: new Vector3(End.X * Level.BlockSizeUnit, yStartB, End.Z * Level.BlockSizeUnit),
				uv0: new Vector2(1, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: false);
		}
		else
		{
			return null; // Not rendered - failed to meet any of the conditions
		}
	}

	public void Normalize(DiagonalSplit diagonalFloorSplit, DiagonalSplit diagonalCeilingSplit, bool isAnyWall)
	{
		QA = NormalizeFloorSplit(QA, diagonalFloorSplit, isAnyWall, out bool isFloorSplitInFloorVoid);
		WS = NormalizeCeilingSplit(WS, diagonalCeilingSplit, isAnyWall, out bool isCeilingSplitInCeilingVoid);

		if (!isFloorSplitInFloorVoid)
		{
			for (int i = 0; i < ExtraFloorSubdivisions.Count; i++)
			{
				WallSplit normalizedSubdivision = NormalizeFloorSplit(ExtraFloorSubdivisions[i], diagonalFloorSplit, isAnyWall, out isFloorSplitInFloorVoid);

				if (isFloorSplitInFloorVoid)
				{
					// Remove the rest as it will also be in the void, therefore not rendered
					ExtraFloorSubdivisions.RemoveRange(i, ExtraFloorSubdivisions.Count - i);
					break;
				}

				ExtraFloorSubdivisions[i] = normalizedSubdivision;
			}
		}

		if (!isCeilingSplitInCeilingVoid)
		{
			for (int i = 0; i < ExtraCeilingSubdivisions.Count; i++)
			{
				WallSplit normalizedSubdivision = NormalizeCeilingSplit(ExtraCeilingSubdivisions[i], diagonalCeilingSplit, isAnyWall, out isCeilingSplitInCeilingVoid);

				if (isCeilingSplitInCeilingVoid)
				{
					// Remove the rest as it will also be in the void, therefore not rendered
					ExtraCeilingSubdivisions.RemoveRange(i, ExtraCeilingSubdivisions.Count - i);
					break;
				}

				ExtraCeilingSubdivisions[i] = normalizedSubdivision;
			}
		}
	}

	private readonly WallSplit NormalizeFloorSplit(WallSplit split, DiagonalSplit diagonalFloorSplit, bool isAnyWall, out bool isInFloorVoid)
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

	private readonly WallSplit NormalizeCeilingSplit(WallSplit split, DiagonalSplit diagonalCeilingSplit, bool isAnyWall, out bool isInCeilingVoid)
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
