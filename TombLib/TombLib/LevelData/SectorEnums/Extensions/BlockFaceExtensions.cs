using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData.SectorEnums.Extensions;

public static class BlockFaceExtensions
{
	public static BlockVertical? GetVertical(this BlockFace face)
	{
		switch (face)
		{
			// Floors

			case BlockFace.Wall_PositiveZ_QA:
			case BlockFace.Wall_NegativeZ_QA:
			case BlockFace.Wall_NegativeX_QA:
			case BlockFace.Wall_PositiveX_QA:
			case BlockFace.Wall_Diagonal_QA:
				return BlockVertical.Floor;

			case BlockFace.Wall_PositiveZ_FloorSubdivision2:
			case BlockFace.Wall_NegativeZ_FloorSubdivision2:
			case BlockFace.Wall_NegativeX_FloorSubdivision2:
			case BlockFace.Wall_PositiveX_FloorSubdivision2:
			case BlockFace.Wall_Diagonal_FloorSubdivision2:
				return BlockVertical.FloorSubdivision2;

			case BlockFace.Wall_PositiveZ_FloorSubdivision3:
			case BlockFace.Wall_NegativeZ_FloorSubdivision3:
			case BlockFace.Wall_NegativeX_FloorSubdivision3:
			case BlockFace.Wall_PositiveX_FloorSubdivision3:
			case BlockFace.Wall_Diagonal_FloorSubdivision3:
				return BlockVertical.FloorSubdivision3;

			case BlockFace.Wall_PositiveZ_FloorSubdivision4:
			case BlockFace.Wall_NegativeZ_FloorSubdivision4:
			case BlockFace.Wall_NegativeX_FloorSubdivision4:
			case BlockFace.Wall_PositiveX_FloorSubdivision4:
			case BlockFace.Wall_Diagonal_FloorSubdivision4:
				return BlockVertical.FloorSubdivision4;

			case BlockFace.Wall_PositiveZ_FloorSubdivision5:
			case BlockFace.Wall_NegativeZ_FloorSubdivision5:
			case BlockFace.Wall_NegativeX_FloorSubdivision5:
			case BlockFace.Wall_PositiveX_FloorSubdivision5:
			case BlockFace.Wall_Diagonal_FloorSubdivision5:
				return BlockVertical.FloorSubdivision5;

			case BlockFace.Wall_PositiveZ_FloorSubdivision6:
			case BlockFace.Wall_NegativeZ_FloorSubdivision6:
			case BlockFace.Wall_NegativeX_FloorSubdivision6:
			case BlockFace.Wall_PositiveX_FloorSubdivision6:
			case BlockFace.Wall_Diagonal_FloorSubdivision6:
				return BlockVertical.FloorSubdivision6;

			case BlockFace.Wall_PositiveZ_FloorSubdivision7:
			case BlockFace.Wall_NegativeZ_FloorSubdivision7:
			case BlockFace.Wall_NegativeX_FloorSubdivision7:
			case BlockFace.Wall_PositiveX_FloorSubdivision7:
			case BlockFace.Wall_Diagonal_FloorSubdivision7:
				return BlockVertical.FloorSubdivision7;

			case BlockFace.Wall_PositiveZ_FloorSubdivision8:
			case BlockFace.Wall_NegativeZ_FloorSubdivision8:
			case BlockFace.Wall_NegativeX_FloorSubdivision8:
			case BlockFace.Wall_PositiveX_FloorSubdivision8:
			case BlockFace.Wall_Diagonal_FloorSubdivision8:
				return BlockVertical.FloorSubdivision8;

			case BlockFace.Wall_PositiveZ_FloorSubdivision9:
			case BlockFace.Wall_NegativeZ_FloorSubdivision9:
			case BlockFace.Wall_NegativeX_FloorSubdivision9:
			case BlockFace.Wall_PositiveX_FloorSubdivision9:
			case BlockFace.Wall_Diagonal_FloorSubdivision9:
				return BlockVertical.FloorSubdivision9;

			// Ceilings

			case BlockFace.Wall_PositiveZ_WS:
			case BlockFace.Wall_NegativeZ_WS:
			case BlockFace.Wall_NegativeX_WS:
			case BlockFace.Wall_PositiveX_WS:
			case BlockFace.Wall_Diagonal_WS:
				return BlockVertical.Ceiling;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision2:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision2:
			case BlockFace.Wall_NegativeX_CeilingSubdivision2:
			case BlockFace.Wall_PositiveX_CeilingSubdivision2:
			case BlockFace.Wall_Diagonal_CeilingSubdivision2:
				return BlockVertical.CeilingSubdivision2;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision3:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision3:
			case BlockFace.Wall_NegativeX_CeilingSubdivision3:
			case BlockFace.Wall_PositiveX_CeilingSubdivision3:
			case BlockFace.Wall_Diagonal_CeilingSubdivision3:
				return BlockVertical.CeilingSubdivision3;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision4:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision4:
			case BlockFace.Wall_NegativeX_CeilingSubdivision4:
			case BlockFace.Wall_PositiveX_CeilingSubdivision4:
			case BlockFace.Wall_Diagonal_CeilingSubdivision4:
				return BlockVertical.CeilingSubdivision4;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision5:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision5:
			case BlockFace.Wall_NegativeX_CeilingSubdivision5:
			case BlockFace.Wall_PositiveX_CeilingSubdivision5:
			case BlockFace.Wall_Diagonal_CeilingSubdivision5:
				return BlockVertical.CeilingSubdivision5;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision6:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision6:
			case BlockFace.Wall_NegativeX_CeilingSubdivision6:
			case BlockFace.Wall_PositiveX_CeilingSubdivision6:
			case BlockFace.Wall_Diagonal_CeilingSubdivision6:
				return BlockVertical.CeilingSubdivision6;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision7:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision7:
			case BlockFace.Wall_NegativeX_CeilingSubdivision7:
			case BlockFace.Wall_PositiveX_CeilingSubdivision7:
			case BlockFace.Wall_Diagonal_CeilingSubdivision7:
				return BlockVertical.CeilingSubdivision7;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision8:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision8:
			case BlockFace.Wall_NegativeX_CeilingSubdivision8:
			case BlockFace.Wall_PositiveX_CeilingSubdivision8:
			case BlockFace.Wall_Diagonal_CeilingSubdivision8:
				return BlockVertical.CeilingSubdivision8;

			case BlockFace.Wall_PositiveZ_CeilingSubdivision9:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision9:
			case BlockFace.Wall_NegativeX_CeilingSubdivision9:
			case BlockFace.Wall_PositiveX_CeilingSubdivision9:
			case BlockFace.Wall_Diagonal_CeilingSubdivision9:
				return BlockVertical.CeilingSubdivision9;

			default:
				return null;
		}
	}

	public static BlockFaceType GetFaceType(this BlockFace face)
	{
		if (face <= BlockFace.Wall_Diagonal_FloorSubdivision2 || face.IsExtraFloorSubdivision())
			return BlockFaceType.Floor;
		else if (face is >= BlockFace.Wall_PositiveZ_WS and <= BlockFace.Wall_Diagonal_CeilingSubdivision2 || face.IsExtraCeilingSubdivision())
			return BlockFaceType.Ceiling;
		else if (face is >= BlockFace.Wall_PositiveZ_Middle and <= BlockFace.Wall_Diagonal_Middle)
			return BlockFaceType.Wall;
		else
			throw new ArgumentException();
	}

	public static Direction GetDirection(this BlockFace face)
	{
		switch (face)
		{
			case BlockFace.Wall_PositiveZ_QA:
			case BlockFace.Wall_PositiveZ_FloorSubdivision2:
			case BlockFace.Wall_PositiveZ_Middle:
			case BlockFace.Wall_PositiveZ_WS:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision2:
			case BlockFace.Wall_PositiveZ_FloorSubdivision3:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision3:
			case BlockFace.Wall_PositiveZ_FloorSubdivision4:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision4:
			case BlockFace.Wall_PositiveZ_FloorSubdivision5:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision5:
			case BlockFace.Wall_PositiveZ_FloorSubdivision6:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision6:
			case BlockFace.Wall_PositiveZ_FloorSubdivision7:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision7:
			case BlockFace.Wall_PositiveZ_FloorSubdivision8:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision8:
			case BlockFace.Wall_PositiveZ_FloorSubdivision9:
			case BlockFace.Wall_PositiveZ_CeilingSubdivision9:
				return Direction.PositiveZ;

			case BlockFace.Wall_NegativeZ_QA:
			case BlockFace.Wall_NegativeZ_FloorSubdivision2:
			case BlockFace.Wall_NegativeZ_Middle:
			case BlockFace.Wall_NegativeZ_WS:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision2:
			case BlockFace.Wall_NegativeZ_FloorSubdivision3:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision3:
			case BlockFace.Wall_NegativeZ_FloorSubdivision4:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision4:
			case BlockFace.Wall_NegativeZ_FloorSubdivision5:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision5:
			case BlockFace.Wall_NegativeZ_FloorSubdivision6:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision6:
			case BlockFace.Wall_NegativeZ_FloorSubdivision7:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision7:
			case BlockFace.Wall_NegativeZ_FloorSubdivision8:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision8:
			case BlockFace.Wall_NegativeZ_FloorSubdivision9:
			case BlockFace.Wall_NegativeZ_CeilingSubdivision9:
				return Direction.NegativeZ;

			case BlockFace.Wall_NegativeX_QA:
			case BlockFace.Wall_NegativeX_FloorSubdivision2:
			case BlockFace.Wall_NegativeX_Middle:
			case BlockFace.Wall_NegativeX_WS:
			case BlockFace.Wall_NegativeX_CeilingSubdivision2:
			case BlockFace.Wall_NegativeX_FloorSubdivision3:
			case BlockFace.Wall_NegativeX_CeilingSubdivision3:
			case BlockFace.Wall_NegativeX_FloorSubdivision4:
			case BlockFace.Wall_NegativeX_CeilingSubdivision4:
			case BlockFace.Wall_NegativeX_FloorSubdivision5:
			case BlockFace.Wall_NegativeX_CeilingSubdivision5:
			case BlockFace.Wall_NegativeX_FloorSubdivision6:
			case BlockFace.Wall_NegativeX_CeilingSubdivision6:
			case BlockFace.Wall_NegativeX_FloorSubdivision7:
			case BlockFace.Wall_NegativeX_CeilingSubdivision7:
			case BlockFace.Wall_NegativeX_FloorSubdivision8:
			case BlockFace.Wall_NegativeX_CeilingSubdivision8:
			case BlockFace.Wall_NegativeX_FloorSubdivision9:
			case BlockFace.Wall_NegativeX_CeilingSubdivision9:
				return Direction.NegativeX;

			case BlockFace.Wall_PositiveX_QA:
			case BlockFace.Wall_PositiveX_FloorSubdivision2:
			case BlockFace.Wall_PositiveX_Middle:
			case BlockFace.Wall_PositiveX_WS:
			case BlockFace.Wall_PositiveX_CeilingSubdivision2:
			case BlockFace.Wall_PositiveX_FloorSubdivision3:
			case BlockFace.Wall_PositiveX_CeilingSubdivision3:
			case BlockFace.Wall_PositiveX_FloorSubdivision4:
			case BlockFace.Wall_PositiveX_CeilingSubdivision4:
			case BlockFace.Wall_PositiveX_FloorSubdivision5:
			case BlockFace.Wall_PositiveX_CeilingSubdivision5:
			case BlockFace.Wall_PositiveX_FloorSubdivision6:
			case BlockFace.Wall_PositiveX_CeilingSubdivision6:
			case BlockFace.Wall_PositiveX_FloorSubdivision7:
			case BlockFace.Wall_PositiveX_CeilingSubdivision7:
			case BlockFace.Wall_PositiveX_FloorSubdivision8:
			case BlockFace.Wall_PositiveX_CeilingSubdivision8:
			case BlockFace.Wall_PositiveX_FloorSubdivision9:
			case BlockFace.Wall_PositiveX_CeilingSubdivision9:
				return Direction.PositiveX;

			case BlockFace.Wall_Diagonal_QA:
			case BlockFace.Wall_Diagonal_FloorSubdivision2:
			case BlockFace.Wall_Diagonal_Middle:
			case BlockFace.Wall_Diagonal_WS:
			case BlockFace.Wall_Diagonal_CeilingSubdivision2:
			case BlockFace.Wall_Diagonal_FloorSubdivision3:
			case BlockFace.Wall_Diagonal_CeilingSubdivision3:
			case BlockFace.Wall_Diagonal_FloorSubdivision4:
			case BlockFace.Wall_Diagonal_CeilingSubdivision4:
			case BlockFace.Wall_Diagonal_FloorSubdivision5:
			case BlockFace.Wall_Diagonal_CeilingSubdivision5:
			case BlockFace.Wall_Diagonal_FloorSubdivision6:
			case BlockFace.Wall_Diagonal_CeilingSubdivision6:
			case BlockFace.Wall_Diagonal_FloorSubdivision7:
			case BlockFace.Wall_Diagonal_CeilingSubdivision7:
			case BlockFace.Wall_Diagonal_FloorSubdivision8:
			case BlockFace.Wall_Diagonal_CeilingSubdivision8:
			case BlockFace.Wall_Diagonal_FloorSubdivision9:
			case BlockFace.Wall_Diagonal_CeilingSubdivision9:
				return Direction.Diagonal;

			default:
				return Direction.None;
		}
	}

	public static IEnumerable<BlockFace> GetWalls()
		=> Enum.GetValues<BlockFace>().Where(face => face.IsWall());

	public static bool IsWall(this BlockFace face)
		=> face is not BlockFace.Floor and not BlockFace.Floor_Triangle2 and not BlockFace.Ceiling and not BlockFace.Ceiling_Triangle2;

	public static bool IsNonWall(this BlockFace face)
		=> face is >= BlockFace.Floor and <= BlockFace.Ceiling_Triangle2;

	public static bool IsNonDiagonalWall(this BlockFace face)
		=> face.IsWall() && face.GetDirection() is not Direction.Diagonal;

	public static bool IsPositiveX(this BlockFace face)
		=> face.GetDirection() is Direction.PositiveX;

	public static bool IsNegativeX(this BlockFace face)
		=> face.GetDirection() is Direction.NegativeX;

	public static bool IsPositiveZ(this BlockFace face)
		=> face.GetDirection() is Direction.PositiveZ;

	public static bool IsNegativeZ(this BlockFace face)
		=> face.GetDirection() is Direction.NegativeZ;

	public static bool IsDiagonal(this BlockFace face)
		=> face.GetDirection() is Direction.Diagonal;

	public static bool IsFloorWall(this BlockFace face)
		=> face <= BlockFace.Wall_Diagonal_FloorSubdivision2 || face.IsExtraFloorSubdivision();

	public static bool IsCeilingWall(this BlockFace face)
		=> face is >= BlockFace.Wall_PositiveZ_WS and <= BlockFace.Wall_Diagonal_CeilingSubdivision2 || face.IsExtraCeilingSubdivision();

	public static bool IsMiddleWall(this BlockFace face)
		=> face is >= BlockFace.Wall_PositiveZ_Middle and <= BlockFace.Wall_Diagonal_Middle;

	public static bool IsFloor(this BlockFace face)
		=> face is BlockFace.Floor or BlockFace.Floor_Triangle2;

	public static bool IsCeiling(this BlockFace face)
		=> face is BlockFace.Ceiling or BlockFace.Ceiling_Triangle2;

	public static bool IsExtraFloorSubdivision(this BlockFace face)
		=> face.GetVertical()?.IsExtraFloorSubdivision() == true;

	public static bool IsExtraCeilingSubdivision(this BlockFace face)
		=> face.GetVertical()?.IsExtraCeilingSubdivision() == true;

	public static bool IsSpecificFloorSubdivision(this BlockFace face, Direction direction)
		=> face.IsExtraFloorSubdivision() && face.GetDirection() == direction;

	public static bool IsSpecificCeilingSubdivision(this BlockFace face, Direction direction)
		=> face.IsExtraCeilingSubdivision() && face.GetDirection() == direction;

	public static BlockFace GetQaFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => BlockFace.Wall_PositiveZ_QA,
			Direction.NegativeZ => BlockFace.Wall_NegativeZ_QA,
			Direction.NegativeX => BlockFace.Wall_NegativeX_QA,
			Direction.PositiveX => BlockFace.Wall_PositiveX_QA,
			Direction.Diagonal => BlockFace.Wall_Diagonal_QA,
			_ => throw new ArgumentException()
		};
	}

	public static BlockFace GetWsFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => BlockFace.Wall_PositiveZ_WS,
			Direction.NegativeZ => BlockFace.Wall_NegativeZ_WS,
			Direction.NegativeX => BlockFace.Wall_NegativeX_WS,
			Direction.PositiveX => BlockFace.Wall_PositiveX_WS,
			Direction.Diagonal => BlockFace.Wall_Diagonal_WS,
			_ => throw new ArgumentException()
		};
	}

	public static BlockFace GetMiddleFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => BlockFace.Wall_PositiveZ_Middle,
			Direction.NegativeZ => BlockFace.Wall_NegativeZ_Middle,
			Direction.NegativeX => BlockFace.Wall_NegativeX_Middle,
			Direction.PositiveX => BlockFace.Wall_PositiveX_Middle,
			Direction.Diagonal => BlockFace.Wall_Diagonal_Middle,
			_ => throw new ArgumentException()
		};
	}

	public static BlockFace GetExtraFloorSubdivisionFace(Direction direction, int subdivisionIndex)
	{
		return direction switch
		{
			Direction.PositiveZ => subdivisionIndex switch
			{
				0 => BlockFace.Wall_PositiveZ_FloorSubdivision2,
				1 => BlockFace.Wall_PositiveZ_FloorSubdivision3,
				2 => BlockFace.Wall_PositiveZ_FloorSubdivision4,
				3 => BlockFace.Wall_PositiveZ_FloorSubdivision5,
				4 => BlockFace.Wall_PositiveZ_FloorSubdivision6,
				5 => BlockFace.Wall_PositiveZ_FloorSubdivision7,
				6 => BlockFace.Wall_PositiveZ_FloorSubdivision8,
				7 => BlockFace.Wall_PositiveZ_FloorSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.PositiveX => subdivisionIndex switch
			{
				0 => BlockFace.Wall_PositiveX_FloorSubdivision2,
				1 => BlockFace.Wall_PositiveX_FloorSubdivision3,
				2 => BlockFace.Wall_PositiveX_FloorSubdivision4,
				3 => BlockFace.Wall_PositiveX_FloorSubdivision5,
				4 => BlockFace.Wall_PositiveX_FloorSubdivision6,
				5 => BlockFace.Wall_PositiveX_FloorSubdivision7,
				6 => BlockFace.Wall_PositiveX_FloorSubdivision8,
				7 => BlockFace.Wall_PositiveX_FloorSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeZ => subdivisionIndex switch
			{
				0 => BlockFace.Wall_NegativeZ_FloorSubdivision2,
				1 => BlockFace.Wall_NegativeZ_FloorSubdivision3,
				2 => BlockFace.Wall_NegativeZ_FloorSubdivision4,
				3 => BlockFace.Wall_NegativeZ_FloorSubdivision5,
				4 => BlockFace.Wall_NegativeZ_FloorSubdivision6,
				5 => BlockFace.Wall_NegativeZ_FloorSubdivision7,
				6 => BlockFace.Wall_NegativeZ_FloorSubdivision8,
				7 => BlockFace.Wall_NegativeZ_FloorSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeX => subdivisionIndex switch
			{
				0 => BlockFace.Wall_NegativeX_FloorSubdivision2,
				1 => BlockFace.Wall_NegativeX_FloorSubdivision3,
				2 => BlockFace.Wall_NegativeX_FloorSubdivision4,
				3 => BlockFace.Wall_NegativeX_FloorSubdivision5,
				4 => BlockFace.Wall_NegativeX_FloorSubdivision6,
				5 => BlockFace.Wall_NegativeX_FloorSubdivision7,
				6 => BlockFace.Wall_NegativeX_FloorSubdivision8,
				7 => BlockFace.Wall_NegativeX_FloorSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.Diagonal => subdivisionIndex switch
			{
				0 => BlockFace.Wall_Diagonal_FloorSubdivision2,
				1 => BlockFace.Wall_Diagonal_FloorSubdivision3,
				2 => BlockFace.Wall_Diagonal_FloorSubdivision4,
				3 => BlockFace.Wall_Diagonal_FloorSubdivision5,
				4 => BlockFace.Wall_Diagonal_FloorSubdivision6,
				5 => BlockFace.Wall_Diagonal_FloorSubdivision7,
				6 => BlockFace.Wall_Diagonal_FloorSubdivision8,
				7 => BlockFace.Wall_Diagonal_FloorSubdivision9,
				_ => throw new ArgumentException()
			},
			_ => throw new ArgumentException(),
		};
	}

	public static BlockFace GetExtraCeilingSubdivisionFace(Direction direction, int subdivisionIndex)
	{
		return direction switch
		{
			Direction.PositiveZ => subdivisionIndex switch
			{
				0 => BlockFace.Wall_PositiveZ_CeilingSubdivision2,
				1 => BlockFace.Wall_PositiveZ_CeilingSubdivision3,
				2 => BlockFace.Wall_PositiveZ_CeilingSubdivision4,
				3 => BlockFace.Wall_PositiveZ_CeilingSubdivision5,
				4 => BlockFace.Wall_PositiveZ_CeilingSubdivision6,
				5 => BlockFace.Wall_PositiveZ_CeilingSubdivision7,
				6 => BlockFace.Wall_PositiveZ_CeilingSubdivision8,
				7 => BlockFace.Wall_PositiveZ_CeilingSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.PositiveX => subdivisionIndex switch
			{
				0 => BlockFace.Wall_PositiveX_CeilingSubdivision2,
				1 => BlockFace.Wall_PositiveX_CeilingSubdivision3,
				2 => BlockFace.Wall_PositiveX_CeilingSubdivision4,
				3 => BlockFace.Wall_PositiveX_CeilingSubdivision5,
				4 => BlockFace.Wall_PositiveX_CeilingSubdivision6,
				5 => BlockFace.Wall_PositiveX_CeilingSubdivision7,
				6 => BlockFace.Wall_PositiveX_CeilingSubdivision8,
				7 => BlockFace.Wall_PositiveX_CeilingSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeZ => subdivisionIndex switch
			{
				0 => BlockFace.Wall_NegativeZ_CeilingSubdivision2,
				1 => BlockFace.Wall_NegativeZ_CeilingSubdivision3,
				2 => BlockFace.Wall_NegativeZ_CeilingSubdivision4,
				3 => BlockFace.Wall_NegativeZ_CeilingSubdivision5,
				4 => BlockFace.Wall_NegativeZ_CeilingSubdivision6,
				5 => BlockFace.Wall_NegativeZ_CeilingSubdivision7,
				6 => BlockFace.Wall_NegativeZ_CeilingSubdivision8,
				7 => BlockFace.Wall_NegativeZ_CeilingSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeX => subdivisionIndex switch
			{
				0 => BlockFace.Wall_NegativeX_CeilingSubdivision2,
				1 => BlockFace.Wall_NegativeX_CeilingSubdivision3,
				2 => BlockFace.Wall_NegativeX_CeilingSubdivision4,
				3 => BlockFace.Wall_NegativeX_CeilingSubdivision5,
				4 => BlockFace.Wall_NegativeX_CeilingSubdivision6,
				5 => BlockFace.Wall_NegativeX_CeilingSubdivision7,
				6 => BlockFace.Wall_NegativeX_CeilingSubdivision8,
				7 => BlockFace.Wall_NegativeX_CeilingSubdivision9,
				_ => throw new ArgumentException()
			},
			Direction.Diagonal => subdivisionIndex switch
			{
				0 => BlockFace.Wall_Diagonal_CeilingSubdivision2,
				1 => BlockFace.Wall_Diagonal_CeilingSubdivision3,
				2 => BlockFace.Wall_Diagonal_CeilingSubdivision4,
				3 => BlockFace.Wall_Diagonal_CeilingSubdivision5,
				4 => BlockFace.Wall_Diagonal_CeilingSubdivision6,
				5 => BlockFace.Wall_Diagonal_CeilingSubdivision7,
				6 => BlockFace.Wall_Diagonal_CeilingSubdivision8,
				7 => BlockFace.Wall_Diagonal_CeilingSubdivision9,
				_ => throw new ArgumentException()
			},
			_ => throw new ArgumentException(),
		};
	}
}
