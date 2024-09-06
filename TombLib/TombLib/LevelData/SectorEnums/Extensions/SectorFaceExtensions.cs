using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData.SectorEnums.Extensions;

public static class SectorFaceExtensions
{
	public static SectorVerticalPart? GetVertical(this SectorFace face)
	{
		switch (face)
		{
			// Floors

			case SectorFace.Wall_PositiveZ_QA:
			case SectorFace.Wall_NegativeZ_QA:
			case SectorFace.Wall_NegativeX_QA:
			case SectorFace.Wall_PositiveX_QA:
			case SectorFace.Wall_Diagonal_QA:
				return SectorVerticalPart.QA;

			case SectorFace.Wall_PositiveZ_Floor2:
			case SectorFace.Wall_NegativeZ_Floor2:
			case SectorFace.Wall_NegativeX_Floor2:
			case SectorFace.Wall_PositiveX_Floor2:
			case SectorFace.Wall_Diagonal_Floor2:
				return SectorVerticalPart.Floor2;

			case SectorFace.Wall_PositiveZ_Floor3:
			case SectorFace.Wall_NegativeZ_Floor3:
			case SectorFace.Wall_NegativeX_Floor3:
			case SectorFace.Wall_PositiveX_Floor3:
			case SectorFace.Wall_Diagonal_Floor3:
				return SectorVerticalPart.Floor3;

			case SectorFace.Wall_PositiveZ_Floor4:
			case SectorFace.Wall_NegativeZ_Floor4:
			case SectorFace.Wall_NegativeX_Floor4:
			case SectorFace.Wall_PositiveX_Floor4:
			case SectorFace.Wall_Diagonal_Floor4:
				return SectorVerticalPart.Floor4;

			case SectorFace.Wall_PositiveZ_Floor5:
			case SectorFace.Wall_NegativeZ_Floor5:
			case SectorFace.Wall_NegativeX_Floor5:
			case SectorFace.Wall_PositiveX_Floor5:
			case SectorFace.Wall_Diagonal_Floor5:
				return SectorVerticalPart.Floor5;

			case SectorFace.Wall_PositiveZ_Floor6:
			case SectorFace.Wall_NegativeZ_Floor6:
			case SectorFace.Wall_NegativeX_Floor6:
			case SectorFace.Wall_PositiveX_Floor6:
			case SectorFace.Wall_Diagonal_Floor6:
				return SectorVerticalPart.Floor6;

			case SectorFace.Wall_PositiveZ_Floor7:
			case SectorFace.Wall_NegativeZ_Floor7:
			case SectorFace.Wall_NegativeX_Floor7:
			case SectorFace.Wall_PositiveX_Floor7:
			case SectorFace.Wall_Diagonal_Floor7:
				return SectorVerticalPart.Floor7;

			case SectorFace.Wall_PositiveZ_Floor8:
			case SectorFace.Wall_NegativeZ_Floor8:
			case SectorFace.Wall_NegativeX_Floor8:
			case SectorFace.Wall_PositiveX_Floor8:
			case SectorFace.Wall_Diagonal_Floor8:
				return SectorVerticalPart.Floor8;

			case SectorFace.Wall_PositiveZ_Floor9:
			case SectorFace.Wall_NegativeZ_Floor9:
			case SectorFace.Wall_NegativeX_Floor9:
			case SectorFace.Wall_PositiveX_Floor9:
			case SectorFace.Wall_Diagonal_Floor9:
				return SectorVerticalPart.Floor9;

			// Ceilings

			case SectorFace.Wall_PositiveZ_WS:
			case SectorFace.Wall_NegativeZ_WS:
			case SectorFace.Wall_NegativeX_WS:
			case SectorFace.Wall_PositiveX_WS:
			case SectorFace.Wall_Diagonal_WS:
				return SectorVerticalPart.WS;

			case SectorFace.Wall_PositiveZ_Ceiling2:
			case SectorFace.Wall_NegativeZ_Ceiling2:
			case SectorFace.Wall_NegativeX_Ceiling2:
			case SectorFace.Wall_PositiveX_Ceiling2:
			case SectorFace.Wall_Diagonal_Ceiling2:
				return SectorVerticalPart.Ceiling2;

			case SectorFace.Wall_PositiveZ_Ceiling3:
			case SectorFace.Wall_NegativeZ_Ceiling3:
			case SectorFace.Wall_NegativeX_Ceiling3:
			case SectorFace.Wall_PositiveX_Ceiling3:
			case SectorFace.Wall_Diagonal_Ceiling3:
				return SectorVerticalPart.Ceiling3;

			case SectorFace.Wall_PositiveZ_Ceiling4:
			case SectorFace.Wall_NegativeZ_Ceiling4:
			case SectorFace.Wall_NegativeX_Ceiling4:
			case SectorFace.Wall_PositiveX_Ceiling4:
			case SectorFace.Wall_Diagonal_Ceiling4:
				return SectorVerticalPart.Ceiling4;

			case SectorFace.Wall_PositiveZ_Ceiling5:
			case SectorFace.Wall_NegativeZ_Ceiling5:
			case SectorFace.Wall_NegativeX_Ceiling5:
			case SectorFace.Wall_PositiveX_Ceiling5:
			case SectorFace.Wall_Diagonal_Ceiling5:
				return SectorVerticalPart.Ceiling5;

			case SectorFace.Wall_PositiveZ_Ceiling6:
			case SectorFace.Wall_NegativeZ_Ceiling6:
			case SectorFace.Wall_NegativeX_Ceiling6:
			case SectorFace.Wall_PositiveX_Ceiling6:
			case SectorFace.Wall_Diagonal_Ceiling6:
				return SectorVerticalPart.Ceiling6;

			case SectorFace.Wall_PositiveZ_Ceiling7:
			case SectorFace.Wall_NegativeZ_Ceiling7:
			case SectorFace.Wall_NegativeX_Ceiling7:
			case SectorFace.Wall_PositiveX_Ceiling7:
			case SectorFace.Wall_Diagonal_Ceiling7:
				return SectorVerticalPart.Ceiling7;

			case SectorFace.Wall_PositiveZ_Ceiling8:
			case SectorFace.Wall_NegativeZ_Ceiling8:
			case SectorFace.Wall_NegativeX_Ceiling8:
			case SectorFace.Wall_PositiveX_Ceiling8:
			case SectorFace.Wall_Diagonal_Ceiling8:
				return SectorVerticalPart.Ceiling8;

			case SectorFace.Wall_PositiveZ_Ceiling9:
			case SectorFace.Wall_NegativeZ_Ceiling9:
			case SectorFace.Wall_NegativeX_Ceiling9:
			case SectorFace.Wall_PositiveX_Ceiling9:
			case SectorFace.Wall_Diagonal_Ceiling9:
				return SectorVerticalPart.Ceiling9;

			default:
				return null;
		}
	}

	public static SectorFaceType GetFaceType(this SectorFace face)
	{
		if (face <= SectorFace.Wall_Diagonal_Floor2 || face.IsExtraFloorSplit())
			return SectorFaceType.Floor;
		else if (face is >= SectorFace.Wall_PositiveZ_WS and <= SectorFace.Wall_Diagonal_Ceiling2 || face.IsExtraCeilingSplit())
			return SectorFaceType.Ceiling;
		else if (face is >= SectorFace.Wall_PositiveZ_Middle and <= SectorFace.Wall_Diagonal_Middle)
			return SectorFaceType.Wall;
		else
			throw new ArgumentException();
	}

	public static Direction GetDirection(this SectorFace face)
	{
		switch (face)
		{
			case SectorFace.Wall_PositiveZ_QA:
			case SectorFace.Wall_PositiveZ_Floor2:
			case SectorFace.Wall_PositiveZ_Middle:
			case SectorFace.Wall_PositiveZ_WS:
			case SectorFace.Wall_PositiveZ_Ceiling2:
			case SectorFace.Wall_PositiveZ_Floor3:
			case SectorFace.Wall_PositiveZ_Ceiling3:
			case SectorFace.Wall_PositiveZ_Floor4:
			case SectorFace.Wall_PositiveZ_Ceiling4:
			case SectorFace.Wall_PositiveZ_Floor5:
			case SectorFace.Wall_PositiveZ_Ceiling5:
			case SectorFace.Wall_PositiveZ_Floor6:
			case SectorFace.Wall_PositiveZ_Ceiling6:
			case SectorFace.Wall_PositiveZ_Floor7:
			case SectorFace.Wall_PositiveZ_Ceiling7:
			case SectorFace.Wall_PositiveZ_Floor8:
			case SectorFace.Wall_PositiveZ_Ceiling8:
			case SectorFace.Wall_PositiveZ_Floor9:
			case SectorFace.Wall_PositiveZ_Ceiling9:
				return Direction.PositiveZ;

			case SectorFace.Wall_NegativeZ_QA:
			case SectorFace.Wall_NegativeZ_Floor2:
			case SectorFace.Wall_NegativeZ_Middle:
			case SectorFace.Wall_NegativeZ_WS:
			case SectorFace.Wall_NegativeZ_Ceiling2:
			case SectorFace.Wall_NegativeZ_Floor3:
			case SectorFace.Wall_NegativeZ_Ceiling3:
			case SectorFace.Wall_NegativeZ_Floor4:
			case SectorFace.Wall_NegativeZ_Ceiling4:
			case SectorFace.Wall_NegativeZ_Floor5:
			case SectorFace.Wall_NegativeZ_Ceiling5:
			case SectorFace.Wall_NegativeZ_Floor6:
			case SectorFace.Wall_NegativeZ_Ceiling6:
			case SectorFace.Wall_NegativeZ_Floor7:
			case SectorFace.Wall_NegativeZ_Ceiling7:
			case SectorFace.Wall_NegativeZ_Floor8:
			case SectorFace.Wall_NegativeZ_Ceiling8:
			case SectorFace.Wall_NegativeZ_Floor9:
			case SectorFace.Wall_NegativeZ_Ceiling9:
				return Direction.NegativeZ;

			case SectorFace.Wall_NegativeX_QA:
			case SectorFace.Wall_NegativeX_Floor2:
			case SectorFace.Wall_NegativeX_Middle:
			case SectorFace.Wall_NegativeX_WS:
			case SectorFace.Wall_NegativeX_Ceiling2:
			case SectorFace.Wall_NegativeX_Floor3:
			case SectorFace.Wall_NegativeX_Ceiling3:
			case SectorFace.Wall_NegativeX_Floor4:
			case SectorFace.Wall_NegativeX_Ceiling4:
			case SectorFace.Wall_NegativeX_Floor5:
			case SectorFace.Wall_NegativeX_Ceiling5:
			case SectorFace.Wall_NegativeX_Floor6:
			case SectorFace.Wall_NegativeX_Ceiling6:
			case SectorFace.Wall_NegativeX_Floor7:
			case SectorFace.Wall_NegativeX_Ceiling7:
			case SectorFace.Wall_NegativeX_Floor8:
			case SectorFace.Wall_NegativeX_Ceiling8:
			case SectorFace.Wall_NegativeX_Floor9:
			case SectorFace.Wall_NegativeX_Ceiling9:
				return Direction.NegativeX;

			case SectorFace.Wall_PositiveX_QA:
			case SectorFace.Wall_PositiveX_Floor2:
			case SectorFace.Wall_PositiveX_Middle:
			case SectorFace.Wall_PositiveX_WS:
			case SectorFace.Wall_PositiveX_Ceiling2:
			case SectorFace.Wall_PositiveX_Floor3:
			case SectorFace.Wall_PositiveX_Ceiling3:
			case SectorFace.Wall_PositiveX_Floor4:
			case SectorFace.Wall_PositiveX_Ceiling4:
			case SectorFace.Wall_PositiveX_Floor5:
			case SectorFace.Wall_PositiveX_Ceiling5:
			case SectorFace.Wall_PositiveX_Floor6:
			case SectorFace.Wall_PositiveX_Ceiling6:
			case SectorFace.Wall_PositiveX_Floor7:
			case SectorFace.Wall_PositiveX_Ceiling7:
			case SectorFace.Wall_PositiveX_Floor8:
			case SectorFace.Wall_PositiveX_Ceiling8:
			case SectorFace.Wall_PositiveX_Floor9:
			case SectorFace.Wall_PositiveX_Ceiling9:
				return Direction.PositiveX;

			case SectorFace.Wall_Diagonal_QA:
			case SectorFace.Wall_Diagonal_Floor2:
			case SectorFace.Wall_Diagonal_Middle:
			case SectorFace.Wall_Diagonal_WS:
			case SectorFace.Wall_Diagonal_Ceiling2:
			case SectorFace.Wall_Diagonal_Floor3:
			case SectorFace.Wall_Diagonal_Ceiling3:
			case SectorFace.Wall_Diagonal_Floor4:
			case SectorFace.Wall_Diagonal_Ceiling4:
			case SectorFace.Wall_Diagonal_Floor5:
			case SectorFace.Wall_Diagonal_Ceiling5:
			case SectorFace.Wall_Diagonal_Floor6:
			case SectorFace.Wall_Diagonal_Ceiling6:
			case SectorFace.Wall_Diagonal_Floor7:
			case SectorFace.Wall_Diagonal_Ceiling7:
			case SectorFace.Wall_Diagonal_Floor8:
			case SectorFace.Wall_Diagonal_Ceiling8:
			case SectorFace.Wall_Diagonal_Floor9:
			case SectorFace.Wall_Diagonal_Ceiling9:
				return Direction.Diagonal;

			default:
				return Direction.None;
		}
	}

	public static IEnumerable<SectorFace> GetWalls()
		=> Enum.GetValues<SectorFace>().Where(face => face.IsWall());

	public static bool IsWall(this SectorFace face)
		=> face is not SectorFace.Floor and not SectorFace.Floor_Triangle2 and not SectorFace.Ceiling and not SectorFace.Ceiling_Triangle2;

	public static bool IsNonWall(this SectorFace face)
		=> face is >= SectorFace.Floor and <= SectorFace.Ceiling_Triangle2;

	public static bool IsNonDiagonalWall(this SectorFace face)
		=> face.IsWall() && face.GetDirection() is not Direction.Diagonal;

	public static bool IsPositiveX(this SectorFace face)
		=> face.GetDirection() is Direction.PositiveX;

	public static bool IsNegativeX(this SectorFace face)
		=> face.GetDirection() is Direction.NegativeX;

	public static bool IsPositiveZ(this SectorFace face)
		=> face.GetDirection() is Direction.PositiveZ;

	public static bool IsNegativeZ(this SectorFace face)
		=> face.GetDirection() is Direction.NegativeZ;

	public static bool IsDiagonal(this SectorFace face)
		=> face.GetDirection() is Direction.Diagonal;

	public static bool IsFloorWall(this SectorFace face)
		=> face <= SectorFace.Wall_Diagonal_Floor2 || face.IsExtraFloorSplit();

	public static bool IsCeilingWall(this SectorFace face)
		=> face is >= SectorFace.Wall_PositiveZ_WS and <= SectorFace.Wall_Diagonal_Ceiling2 || face.IsExtraCeilingSplit();

	public static bool IsMiddleWall(this SectorFace face)
		=> face is >= SectorFace.Wall_PositiveZ_Middle and <= SectorFace.Wall_Diagonal_Middle;

	public static bool IsFloor(this SectorFace face)
		=> face is SectorFace.Floor or SectorFace.Floor_Triangle2;

	public static bool IsCeiling(this SectorFace face)
		=> face is SectorFace.Ceiling or SectorFace.Ceiling_Triangle2;

	public static bool IsExtraFloorSplit(this SectorFace face)
		=> face.GetVertical()?.IsExtraFloorSplit() == true;

	public static bool IsExtraCeilingSplit(this SectorFace face)
		=> face.GetVertical()?.IsExtraCeilingSplit() == true;

	public static bool IsSpecificFloorSplit(this SectorFace face, Direction direction)
		=> face.IsExtraFloorSplit() && face.GetDirection() == direction;

	public static bool IsSpecificCeilingSplit(this SectorFace face, Direction direction)
		=> face.IsExtraCeilingSplit() && face.GetDirection() == direction;

	public static SectorFace GetQaFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => SectorFace.Wall_PositiveZ_QA,
			Direction.NegativeZ => SectorFace.Wall_NegativeZ_QA,
			Direction.NegativeX => SectorFace.Wall_NegativeX_QA,
			Direction.PositiveX => SectorFace.Wall_PositiveX_QA,
			Direction.Diagonal => SectorFace.Wall_Diagonal_QA,
			_ => throw new ArgumentException()
		};
	}

	public static SectorFace GetWsFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => SectorFace.Wall_PositiveZ_WS,
			Direction.NegativeZ => SectorFace.Wall_NegativeZ_WS,
			Direction.NegativeX => SectorFace.Wall_NegativeX_WS,
			Direction.PositiveX => SectorFace.Wall_PositiveX_WS,
			Direction.Diagonal => SectorFace.Wall_Diagonal_WS,
			_ => throw new ArgumentException()
		};
	}

	public static SectorFace GetMiddleFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => SectorFace.Wall_PositiveZ_Middle,
			Direction.NegativeZ => SectorFace.Wall_NegativeZ_Middle,
			Direction.NegativeX => SectorFace.Wall_NegativeX_Middle,
			Direction.PositiveX => SectorFace.Wall_PositiveX_Middle,
			Direction.Diagonal => SectorFace.Wall_Diagonal_Middle,
			_ => throw new ArgumentException()
		};
	}

	public static SectorFace GetExtraFloorSplitFace(Direction direction, int splitIndex)
	{
		return direction switch
		{
			Direction.PositiveZ => splitIndex switch
			{
				0 => SectorFace.Wall_PositiveZ_Floor2,
				1 => SectorFace.Wall_PositiveZ_Floor3,
				2 => SectorFace.Wall_PositiveZ_Floor4,
				3 => SectorFace.Wall_PositiveZ_Floor5,
				4 => SectorFace.Wall_PositiveZ_Floor6,
				5 => SectorFace.Wall_PositiveZ_Floor7,
				6 => SectorFace.Wall_PositiveZ_Floor8,
				7 => SectorFace.Wall_PositiveZ_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.PositiveX => splitIndex switch
			{
				0 => SectorFace.Wall_PositiveX_Floor2,
				1 => SectorFace.Wall_PositiveX_Floor3,
				2 => SectorFace.Wall_PositiveX_Floor4,
				3 => SectorFace.Wall_PositiveX_Floor5,
				4 => SectorFace.Wall_PositiveX_Floor6,
				5 => SectorFace.Wall_PositiveX_Floor7,
				6 => SectorFace.Wall_PositiveX_Floor8,
				7 => SectorFace.Wall_PositiveX_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeZ => splitIndex switch
			{
				0 => SectorFace.Wall_NegativeZ_Floor2,
				1 => SectorFace.Wall_NegativeZ_Floor3,
				2 => SectorFace.Wall_NegativeZ_Floor4,
				3 => SectorFace.Wall_NegativeZ_Floor5,
				4 => SectorFace.Wall_NegativeZ_Floor6,
				5 => SectorFace.Wall_NegativeZ_Floor7,
				6 => SectorFace.Wall_NegativeZ_Floor8,
				7 => SectorFace.Wall_NegativeZ_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeX => splitIndex switch
			{
				0 => SectorFace.Wall_NegativeX_Floor2,
				1 => SectorFace.Wall_NegativeX_Floor3,
				2 => SectorFace.Wall_NegativeX_Floor4,
				3 => SectorFace.Wall_NegativeX_Floor5,
				4 => SectorFace.Wall_NegativeX_Floor6,
				5 => SectorFace.Wall_NegativeX_Floor7,
				6 => SectorFace.Wall_NegativeX_Floor8,
				7 => SectorFace.Wall_NegativeX_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.Diagonal => splitIndex switch
			{
				0 => SectorFace.Wall_Diagonal_Floor2,
				1 => SectorFace.Wall_Diagonal_Floor3,
				2 => SectorFace.Wall_Diagonal_Floor4,
				3 => SectorFace.Wall_Diagonal_Floor5,
				4 => SectorFace.Wall_Diagonal_Floor6,
				5 => SectorFace.Wall_Diagonal_Floor7,
				6 => SectorFace.Wall_Diagonal_Floor8,
				7 => SectorFace.Wall_Diagonal_Floor9,
				_ => throw new ArgumentException()
			},
			_ => throw new ArgumentException(),
		};
	}

	public static SectorFace GetExtraCeilingSplitFace(Direction direction, int splitIndex)
	{
		return direction switch
		{
			Direction.PositiveZ => splitIndex switch
			{
				0 => SectorFace.Wall_PositiveZ_Ceiling2,
				1 => SectorFace.Wall_PositiveZ_Ceiling3,
				2 => SectorFace.Wall_PositiveZ_Ceiling4,
				3 => SectorFace.Wall_PositiveZ_Ceiling5,
				4 => SectorFace.Wall_PositiveZ_Ceiling6,
				5 => SectorFace.Wall_PositiveZ_Ceiling7,
				6 => SectorFace.Wall_PositiveZ_Ceiling8,
				7 => SectorFace.Wall_PositiveZ_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.PositiveX => splitIndex switch
			{
				0 => SectorFace.Wall_PositiveX_Ceiling2,
				1 => SectorFace.Wall_PositiveX_Ceiling3,
				2 => SectorFace.Wall_PositiveX_Ceiling4,
				3 => SectorFace.Wall_PositiveX_Ceiling5,
				4 => SectorFace.Wall_PositiveX_Ceiling6,
				5 => SectorFace.Wall_PositiveX_Ceiling7,
				6 => SectorFace.Wall_PositiveX_Ceiling8,
				7 => SectorFace.Wall_PositiveX_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeZ => splitIndex switch
			{
				0 => SectorFace.Wall_NegativeZ_Ceiling2,
				1 => SectorFace.Wall_NegativeZ_Ceiling3,
				2 => SectorFace.Wall_NegativeZ_Ceiling4,
				3 => SectorFace.Wall_NegativeZ_Ceiling5,
				4 => SectorFace.Wall_NegativeZ_Ceiling6,
				5 => SectorFace.Wall_NegativeZ_Ceiling7,
				6 => SectorFace.Wall_NegativeZ_Ceiling8,
				7 => SectorFace.Wall_NegativeZ_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeX => splitIndex switch
			{
				0 => SectorFace.Wall_NegativeX_Ceiling2,
				1 => SectorFace.Wall_NegativeX_Ceiling3,
				2 => SectorFace.Wall_NegativeX_Ceiling4,
				3 => SectorFace.Wall_NegativeX_Ceiling5,
				4 => SectorFace.Wall_NegativeX_Ceiling6,
				5 => SectorFace.Wall_NegativeX_Ceiling7,
				6 => SectorFace.Wall_NegativeX_Ceiling8,
				7 => SectorFace.Wall_NegativeX_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.Diagonal => splitIndex switch
			{
				0 => SectorFace.Wall_Diagonal_Ceiling2,
				1 => SectorFace.Wall_Diagonal_Ceiling3,
				2 => SectorFace.Wall_Diagonal_Ceiling4,
				3 => SectorFace.Wall_Diagonal_Ceiling5,
				4 => SectorFace.Wall_Diagonal_Ceiling6,
				5 => SectorFace.Wall_Diagonal_Ceiling7,
				6 => SectorFace.Wall_Diagonal_Ceiling8,
				7 => SectorFace.Wall_Diagonal_Ceiling9,
				_ => throw new ArgumentException()
			},
			_ => throw new ArgumentException(),
		};
	}
}
