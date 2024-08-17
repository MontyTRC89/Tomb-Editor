using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData.SectorEnums.Extensions;

public static class SectorFaceExtensions
{
	public static SectorVerticalPart? GetVertical(this SectorFaceIdentifier face)
	{
		switch (face)
		{
			// Floors

			case SectorFaceIdentifier.Wall_PositiveZ_QA:
			case SectorFaceIdentifier.Wall_NegativeZ_QA:
			case SectorFaceIdentifier.Wall_NegativeX_QA:
			case SectorFaceIdentifier.Wall_PositiveX_QA:
			case SectorFaceIdentifier.Wall_Diagonal_QA:
				return SectorVerticalPart.QA;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor2:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor2:
			case SectorFaceIdentifier.Wall_NegativeX_Floor2:
			case SectorFaceIdentifier.Wall_PositiveX_Floor2:
			case SectorFaceIdentifier.Wall_Diagonal_Floor2:
				return SectorVerticalPart.Floor2;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor3:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor3:
			case SectorFaceIdentifier.Wall_NegativeX_Floor3:
			case SectorFaceIdentifier.Wall_PositiveX_Floor3:
			case SectorFaceIdentifier.Wall_Diagonal_Floor3:
				return SectorVerticalPart.Floor3;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor4:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor4:
			case SectorFaceIdentifier.Wall_NegativeX_Floor4:
			case SectorFaceIdentifier.Wall_PositiveX_Floor4:
			case SectorFaceIdentifier.Wall_Diagonal_Floor4:
				return SectorVerticalPart.Floor4;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor5:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor5:
			case SectorFaceIdentifier.Wall_NegativeX_Floor5:
			case SectorFaceIdentifier.Wall_PositiveX_Floor5:
			case SectorFaceIdentifier.Wall_Diagonal_Floor5:
				return SectorVerticalPart.Floor5;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor6:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor6:
			case SectorFaceIdentifier.Wall_NegativeX_Floor6:
			case SectorFaceIdentifier.Wall_PositiveX_Floor6:
			case SectorFaceIdentifier.Wall_Diagonal_Floor6:
				return SectorVerticalPart.Floor6;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor7:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor7:
			case SectorFaceIdentifier.Wall_NegativeX_Floor7:
			case SectorFaceIdentifier.Wall_PositiveX_Floor7:
			case SectorFaceIdentifier.Wall_Diagonal_Floor7:
				return SectorVerticalPart.Floor7;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor8:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor8:
			case SectorFaceIdentifier.Wall_NegativeX_Floor8:
			case SectorFaceIdentifier.Wall_PositiveX_Floor8:
			case SectorFaceIdentifier.Wall_Diagonal_Floor8:
				return SectorVerticalPart.Floor8;

			case SectorFaceIdentifier.Wall_PositiveZ_Floor9:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor9:
			case SectorFaceIdentifier.Wall_NegativeX_Floor9:
			case SectorFaceIdentifier.Wall_PositiveX_Floor9:
			case SectorFaceIdentifier.Wall_Diagonal_Floor9:
				return SectorVerticalPart.Floor9;

			// Ceilings

			case SectorFaceIdentifier.Wall_PositiveZ_WS:
			case SectorFaceIdentifier.Wall_NegativeZ_WS:
			case SectorFaceIdentifier.Wall_NegativeX_WS:
			case SectorFaceIdentifier.Wall_PositiveX_WS:
			case SectorFaceIdentifier.Wall_Diagonal_WS:
				return SectorVerticalPart.WS;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling2:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling2:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling2:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling2:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling2:
				return SectorVerticalPart.Ceiling2;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling3:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling3:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling3:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling3:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling3:
				return SectorVerticalPart.Ceiling3;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling4:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling4:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling4:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling4:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling4:
				return SectorVerticalPart.Ceiling4;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling5:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling5:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling5:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling5:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling5:
				return SectorVerticalPart.Ceiling5;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling6:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling6:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling6:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling6:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling6:
				return SectorVerticalPart.Ceiling6;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling7:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling7:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling7:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling7:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling7:
				return SectorVerticalPart.Ceiling7;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling8:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling8:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling8:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling8:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling8:
				return SectorVerticalPart.Ceiling8;

			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling9:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling9:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling9:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling9:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling9:
				return SectorVerticalPart.Ceiling9;

			default:
				return null;
		}
	}

	public static SectorFaceType GetFaceType(this SectorFaceIdentifier face)
	{
		if (face <= SectorFaceIdentifier.Wall_Diagonal_Floor2 || face.IsExtraFloorSplit())
			return SectorFaceType.Floor;
		else if (face is >= SectorFaceIdentifier.Wall_PositiveZ_WS and <= SectorFaceIdentifier.Wall_Diagonal_Ceiling2 || face.IsExtraCeilingSplit())
			return SectorFaceType.Ceiling;
		else if (face is >= SectorFaceIdentifier.Wall_PositiveZ_Middle and <= SectorFaceIdentifier.Wall_Diagonal_Middle)
			return SectorFaceType.Wall;
		else
			throw new ArgumentException();
	}

	public static Direction GetDirection(this SectorFaceIdentifier face)
	{
		switch (face)
		{
			case SectorFaceIdentifier.Wall_PositiveZ_QA:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor2:
			case SectorFaceIdentifier.Wall_PositiveZ_Middle:
			case SectorFaceIdentifier.Wall_PositiveZ_WS:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling2:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor3:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling3:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor4:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling4:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor5:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling5:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor6:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling6:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor7:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling7:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor8:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling8:
			case SectorFaceIdentifier.Wall_PositiveZ_Floor9:
			case SectorFaceIdentifier.Wall_PositiveZ_Ceiling9:
				return Direction.PositiveZ;

			case SectorFaceIdentifier.Wall_NegativeZ_QA:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor2:
			case SectorFaceIdentifier.Wall_NegativeZ_Middle:
			case SectorFaceIdentifier.Wall_NegativeZ_WS:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling2:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor3:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling3:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor4:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling4:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor5:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling5:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor6:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling6:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor7:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling7:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor8:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling8:
			case SectorFaceIdentifier.Wall_NegativeZ_Floor9:
			case SectorFaceIdentifier.Wall_NegativeZ_Ceiling9:
				return Direction.NegativeZ;

			case SectorFaceIdentifier.Wall_NegativeX_QA:
			case SectorFaceIdentifier.Wall_NegativeX_Floor2:
			case SectorFaceIdentifier.Wall_NegativeX_Middle:
			case SectorFaceIdentifier.Wall_NegativeX_WS:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling2:
			case SectorFaceIdentifier.Wall_NegativeX_Floor3:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling3:
			case SectorFaceIdentifier.Wall_NegativeX_Floor4:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling4:
			case SectorFaceIdentifier.Wall_NegativeX_Floor5:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling5:
			case SectorFaceIdentifier.Wall_NegativeX_Floor6:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling6:
			case SectorFaceIdentifier.Wall_NegativeX_Floor7:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling7:
			case SectorFaceIdentifier.Wall_NegativeX_Floor8:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling8:
			case SectorFaceIdentifier.Wall_NegativeX_Floor9:
			case SectorFaceIdentifier.Wall_NegativeX_Ceiling9:
				return Direction.NegativeX;

			case SectorFaceIdentifier.Wall_PositiveX_QA:
			case SectorFaceIdentifier.Wall_PositiveX_Floor2:
			case SectorFaceIdentifier.Wall_PositiveX_Middle:
			case SectorFaceIdentifier.Wall_PositiveX_WS:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling2:
			case SectorFaceIdentifier.Wall_PositiveX_Floor3:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling3:
			case SectorFaceIdentifier.Wall_PositiveX_Floor4:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling4:
			case SectorFaceIdentifier.Wall_PositiveX_Floor5:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling5:
			case SectorFaceIdentifier.Wall_PositiveX_Floor6:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling6:
			case SectorFaceIdentifier.Wall_PositiveX_Floor7:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling7:
			case SectorFaceIdentifier.Wall_PositiveX_Floor8:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling8:
			case SectorFaceIdentifier.Wall_PositiveX_Floor9:
			case SectorFaceIdentifier.Wall_PositiveX_Ceiling9:
				return Direction.PositiveX;

			case SectorFaceIdentifier.Wall_Diagonal_QA:
			case SectorFaceIdentifier.Wall_Diagonal_Floor2:
			case SectorFaceIdentifier.Wall_Diagonal_Middle:
			case SectorFaceIdentifier.Wall_Diagonal_WS:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling2:
			case SectorFaceIdentifier.Wall_Diagonal_Floor3:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling3:
			case SectorFaceIdentifier.Wall_Diagonal_Floor4:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling4:
			case SectorFaceIdentifier.Wall_Diagonal_Floor5:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling5:
			case SectorFaceIdentifier.Wall_Diagonal_Floor6:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling6:
			case SectorFaceIdentifier.Wall_Diagonal_Floor7:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling7:
			case SectorFaceIdentifier.Wall_Diagonal_Floor8:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling8:
			case SectorFaceIdentifier.Wall_Diagonal_Floor9:
			case SectorFaceIdentifier.Wall_Diagonal_Ceiling9:
				return Direction.Diagonal;

			default:
				return Direction.None;
		}
	}

	public static IEnumerable<SectorFaceIdentifier> GetWalls()
		=> Enum.GetValues<SectorFaceIdentifier>().Where(face => face.IsWall());

	public static bool IsWall(this SectorFaceIdentifier face)
		=> face is not SectorFaceIdentifier.Floor and not SectorFaceIdentifier.Floor_Triangle2 and not SectorFaceIdentifier.Ceiling and not SectorFaceIdentifier.Ceiling_Triangle2;

	public static bool IsNonWall(this SectorFaceIdentifier face)
		=> face is >= SectorFaceIdentifier.Floor and <= SectorFaceIdentifier.Ceiling_Triangle2;

	public static bool IsNonDiagonalWall(this SectorFaceIdentifier face)
		=> face.IsWall() && face.GetDirection() is not Direction.Diagonal;

	public static bool IsPositiveX(this SectorFaceIdentifier face)
		=> face.GetDirection() is Direction.PositiveX;

	public static bool IsNegativeX(this SectorFaceIdentifier face)
		=> face.GetDirection() is Direction.NegativeX;

	public static bool IsPositiveZ(this SectorFaceIdentifier face)
		=> face.GetDirection() is Direction.PositiveZ;

	public static bool IsNegativeZ(this SectorFaceIdentifier face)
		=> face.GetDirection() is Direction.NegativeZ;

	public static bool IsDiagonal(this SectorFaceIdentifier face)
		=> face.GetDirection() is Direction.Diagonal;

	public static bool IsFloorWall(this SectorFaceIdentifier face)
		=> face <= SectorFaceIdentifier.Wall_Diagonal_Floor2 || face.IsExtraFloorSplit();

	public static bool IsCeilingWall(this SectorFaceIdentifier face)
		=> face is >= SectorFaceIdentifier.Wall_PositiveZ_WS and <= SectorFaceIdentifier.Wall_Diagonal_Ceiling2 || face.IsExtraCeilingSplit();

	public static bool IsMiddleWall(this SectorFaceIdentifier face)
		=> face is >= SectorFaceIdentifier.Wall_PositiveZ_Middle and <= SectorFaceIdentifier.Wall_Diagonal_Middle;

	public static bool IsFloor(this SectorFaceIdentifier face)
		=> face is SectorFaceIdentifier.Floor or SectorFaceIdentifier.Floor_Triangle2;

	public static bool IsCeiling(this SectorFaceIdentifier face)
		=> face is SectorFaceIdentifier.Ceiling or SectorFaceIdentifier.Ceiling_Triangle2;

	public static bool IsExtraFloorSplit(this SectorFaceIdentifier face)
		=> face.GetVertical()?.IsExtraFloorSplit() == true;

	public static bool IsExtraCeilingSplit(this SectorFaceIdentifier face)
		=> face.GetVertical()?.IsExtraCeilingSplit() == true;

	public static bool IsSpecificFloorSplit(this SectorFaceIdentifier face, Direction direction)
		=> face.IsExtraFloorSplit() && face.GetDirection() == direction;

	public static bool IsSpecificCeilingSplit(this SectorFaceIdentifier face, Direction direction)
		=> face.IsExtraCeilingSplit() && face.GetDirection() == direction;

	public static SectorFaceIdentifier GetQaFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => SectorFaceIdentifier.Wall_PositiveZ_QA,
			Direction.NegativeZ => SectorFaceIdentifier.Wall_NegativeZ_QA,
			Direction.NegativeX => SectorFaceIdentifier.Wall_NegativeX_QA,
			Direction.PositiveX => SectorFaceIdentifier.Wall_PositiveX_QA,
			Direction.Diagonal => SectorFaceIdentifier.Wall_Diagonal_QA,
			_ => throw new ArgumentException()
		};
	}

	public static SectorFaceIdentifier GetWsFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => SectorFaceIdentifier.Wall_PositiveZ_WS,
			Direction.NegativeZ => SectorFaceIdentifier.Wall_NegativeZ_WS,
			Direction.NegativeX => SectorFaceIdentifier.Wall_NegativeX_WS,
			Direction.PositiveX => SectorFaceIdentifier.Wall_PositiveX_WS,
			Direction.Diagonal => SectorFaceIdentifier.Wall_Diagonal_WS,
			_ => throw new ArgumentException()
		};
	}

	public static SectorFaceIdentifier GetMiddleFace(Direction direction)
	{
		return direction switch
		{
			Direction.PositiveZ => SectorFaceIdentifier.Wall_PositiveZ_Middle,
			Direction.NegativeZ => SectorFaceIdentifier.Wall_NegativeZ_Middle,
			Direction.NegativeX => SectorFaceIdentifier.Wall_NegativeX_Middle,
			Direction.PositiveX => SectorFaceIdentifier.Wall_PositiveX_Middle,
			Direction.Diagonal => SectorFaceIdentifier.Wall_Diagonal_Middle,
			_ => throw new ArgumentException()
		};
	}

	public static SectorFaceIdentifier GetExtraFloorSplitFace(Direction direction, int splitIndex)
	{
		return direction switch
		{
			Direction.PositiveZ => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_PositiveZ_Floor2,
				1 => SectorFaceIdentifier.Wall_PositiveZ_Floor3,
				2 => SectorFaceIdentifier.Wall_PositiveZ_Floor4,
				3 => SectorFaceIdentifier.Wall_PositiveZ_Floor5,
				4 => SectorFaceIdentifier.Wall_PositiveZ_Floor6,
				5 => SectorFaceIdentifier.Wall_PositiveZ_Floor7,
				6 => SectorFaceIdentifier.Wall_PositiveZ_Floor8,
				7 => SectorFaceIdentifier.Wall_PositiveZ_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.PositiveX => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_PositiveX_Floor2,
				1 => SectorFaceIdentifier.Wall_PositiveX_Floor3,
				2 => SectorFaceIdentifier.Wall_PositiveX_Floor4,
				3 => SectorFaceIdentifier.Wall_PositiveX_Floor5,
				4 => SectorFaceIdentifier.Wall_PositiveX_Floor6,
				5 => SectorFaceIdentifier.Wall_PositiveX_Floor7,
				6 => SectorFaceIdentifier.Wall_PositiveX_Floor8,
				7 => SectorFaceIdentifier.Wall_PositiveX_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeZ => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_NegativeZ_Floor2,
				1 => SectorFaceIdentifier.Wall_NegativeZ_Floor3,
				2 => SectorFaceIdentifier.Wall_NegativeZ_Floor4,
				3 => SectorFaceIdentifier.Wall_NegativeZ_Floor5,
				4 => SectorFaceIdentifier.Wall_NegativeZ_Floor6,
				5 => SectorFaceIdentifier.Wall_NegativeZ_Floor7,
				6 => SectorFaceIdentifier.Wall_NegativeZ_Floor8,
				7 => SectorFaceIdentifier.Wall_NegativeZ_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeX => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_NegativeX_Floor2,
				1 => SectorFaceIdentifier.Wall_NegativeX_Floor3,
				2 => SectorFaceIdentifier.Wall_NegativeX_Floor4,
				3 => SectorFaceIdentifier.Wall_NegativeX_Floor5,
				4 => SectorFaceIdentifier.Wall_NegativeX_Floor6,
				5 => SectorFaceIdentifier.Wall_NegativeX_Floor7,
				6 => SectorFaceIdentifier.Wall_NegativeX_Floor8,
				7 => SectorFaceIdentifier.Wall_NegativeX_Floor9,
				_ => throw new ArgumentException()
			},
			Direction.Diagonal => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_Diagonal_Floor2,
				1 => SectorFaceIdentifier.Wall_Diagonal_Floor3,
				2 => SectorFaceIdentifier.Wall_Diagonal_Floor4,
				3 => SectorFaceIdentifier.Wall_Diagonal_Floor5,
				4 => SectorFaceIdentifier.Wall_Diagonal_Floor6,
				5 => SectorFaceIdentifier.Wall_Diagonal_Floor7,
				6 => SectorFaceIdentifier.Wall_Diagonal_Floor8,
				7 => SectorFaceIdentifier.Wall_Diagonal_Floor9,
				_ => throw new ArgumentException()
			},
			_ => throw new ArgumentException(),
		};
	}

	public static SectorFaceIdentifier GetExtraCeilingSplitFace(Direction direction, int splitIndex)
	{
		return direction switch
		{
			Direction.PositiveZ => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling2,
				1 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling3,
				2 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling4,
				3 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling5,
				4 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling6,
				5 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling7,
				6 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling8,
				7 => SectorFaceIdentifier.Wall_PositiveZ_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.PositiveX => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_PositiveX_Ceiling2,
				1 => SectorFaceIdentifier.Wall_PositiveX_Ceiling3,
				2 => SectorFaceIdentifier.Wall_PositiveX_Ceiling4,
				3 => SectorFaceIdentifier.Wall_PositiveX_Ceiling5,
				4 => SectorFaceIdentifier.Wall_PositiveX_Ceiling6,
				5 => SectorFaceIdentifier.Wall_PositiveX_Ceiling7,
				6 => SectorFaceIdentifier.Wall_PositiveX_Ceiling8,
				7 => SectorFaceIdentifier.Wall_PositiveX_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeZ => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling2,
				1 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling3,
				2 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling4,
				3 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling5,
				4 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling6,
				5 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling7,
				6 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling8,
				7 => SectorFaceIdentifier.Wall_NegativeZ_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.NegativeX => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_NegativeX_Ceiling2,
				1 => SectorFaceIdentifier.Wall_NegativeX_Ceiling3,
				2 => SectorFaceIdentifier.Wall_NegativeX_Ceiling4,
				3 => SectorFaceIdentifier.Wall_NegativeX_Ceiling5,
				4 => SectorFaceIdentifier.Wall_NegativeX_Ceiling6,
				5 => SectorFaceIdentifier.Wall_NegativeX_Ceiling7,
				6 => SectorFaceIdentifier.Wall_NegativeX_Ceiling8,
				7 => SectorFaceIdentifier.Wall_NegativeX_Ceiling9,
				_ => throw new ArgumentException()
			},
			Direction.Diagonal => splitIndex switch
			{
				0 => SectorFaceIdentifier.Wall_Diagonal_Ceiling2,
				1 => SectorFaceIdentifier.Wall_Diagonal_Ceiling3,
				2 => SectorFaceIdentifier.Wall_Diagonal_Ceiling4,
				3 => SectorFaceIdentifier.Wall_Diagonal_Ceiling5,
				4 => SectorFaceIdentifier.Wall_Diagonal_Ceiling6,
				5 => SectorFaceIdentifier.Wall_Diagonal_Ceiling7,
				6 => SectorFaceIdentifier.Wall_Diagonal_Ceiling8,
				7 => SectorFaceIdentifier.Wall_Diagonal_Ceiling9,
				_ => throw new ArgumentException()
			},
			_ => throw new ArgumentException(),
		};
	}
}
