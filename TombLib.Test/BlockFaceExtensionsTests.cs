using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombLib.Test;

[TestClass]
public class BlockFaceExtensionsTests
{
	[TestMethod]
	public void GetVertical()
	{
		// Floor

		Assert.AreEqual(BlockVertical.Floor, BlockFace.Wall_PositiveZ_QA.GetVertical());
		Assert.AreEqual(BlockVertical.Floor, BlockFace.Wall_NegativeZ_QA.GetVertical());
		Assert.AreEqual(BlockVertical.Floor, BlockFace.Wall_NegativeX_QA.GetVertical());
		Assert.AreEqual(BlockVertical.Floor, BlockFace.Wall_PositiveX_QA.GetVertical());
		Assert.AreEqual(BlockVertical.Floor, BlockFace.Wall_Diagonal_QA.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision2")))
			Assert.AreEqual(BlockVertical.FloorSubdivision2, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision3")))
			Assert.AreEqual(BlockVertical.FloorSubdivision3, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision4")))
			Assert.AreEqual(BlockVertical.FloorSubdivision4, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision5")))
			Assert.AreEqual(BlockVertical.FloorSubdivision5, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision6")))
			Assert.AreEqual(BlockVertical.FloorSubdivision6, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision7")))
			Assert.AreEqual(BlockVertical.FloorSubdivision7, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision8")))
			Assert.AreEqual(BlockVertical.FloorSubdivision8, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision9")))
			Assert.AreEqual(BlockVertical.FloorSubdivision9, face.GetVertical());

		// Ceiling

		Assert.AreEqual(BlockVertical.Ceiling, BlockFace.Wall_PositiveZ_WS.GetVertical());
		Assert.AreEqual(BlockVertical.Ceiling, BlockFace.Wall_NegativeZ_WS.GetVertical());
		Assert.AreEqual(BlockVertical.Ceiling, BlockFace.Wall_NegativeX_WS.GetVertical());
		Assert.AreEqual(BlockVertical.Ceiling, BlockFace.Wall_PositiveX_WS.GetVertical());
		Assert.AreEqual(BlockVertical.Ceiling, BlockFace.Wall_Diagonal_WS.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision2")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision2, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision3")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision3, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision4")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision4, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision5")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision5, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision6")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision6, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision7")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision7, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision8")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision8, face.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision9")))
			Assert.AreEqual(BlockVertical.CeilingSubdivision9, face.GetVertical());

		// null results

		Assert.IsNull(BlockFace.Floor.GetVertical());
		Assert.IsNull(BlockFace.Floor_Triangle2.GetVertical());

		Assert.IsNull(BlockFace.Ceiling.GetVertical());
		Assert.IsNull(BlockFace.Ceiling_Triangle2.GetVertical());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Middle")))
			Assert.IsNull(face.GetVertical());

		Assert.IsNull(BlockFace.Count.GetVertical());
	}

	[TestMethod]
	public void GetFaceType()
	{
		// Floor

		Assert.AreEqual(BlockFaceType.Floor, BlockFace.Wall_PositiveZ_QA.GetFaceType());
		Assert.AreEqual(BlockFaceType.Floor, BlockFace.Wall_NegativeZ_QA.GetFaceType());
		Assert.AreEqual(BlockFaceType.Floor, BlockFace.Wall_NegativeX_QA.GetFaceType());
		Assert.AreEqual(BlockFaceType.Floor, BlockFace.Wall_PositiveX_QA.GetFaceType());
		Assert.AreEqual(BlockFaceType.Floor, BlockFace.Wall_Diagonal_QA.GetFaceType());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.AreEqual(BlockFaceType.Floor, face.GetFaceType());

		// Ceiling

		Assert.AreEqual(BlockFaceType.Ceiling, BlockFace.Wall_PositiveZ_WS.GetFaceType());
		Assert.AreEqual(BlockFaceType.Ceiling, BlockFace.Wall_NegativeZ_WS.GetFaceType());
		Assert.AreEqual(BlockFaceType.Ceiling, BlockFace.Wall_NegativeX_WS.GetFaceType());
		Assert.AreEqual(BlockFaceType.Ceiling, BlockFace.Wall_PositiveX_WS.GetFaceType());
		Assert.AreEqual(BlockFaceType.Ceiling, BlockFace.Wall_Diagonal_WS.GetFaceType());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.AreEqual(BlockFaceType.Ceiling, face.GetFaceType());

		// Wall

		Assert.AreEqual(BlockFaceType.Wall, BlockFace.Wall_PositiveZ_Middle.GetFaceType());
		Assert.AreEqual(BlockFaceType.Wall, BlockFace.Wall_NegativeZ_Middle.GetFaceType());
		Assert.AreEqual(BlockFaceType.Wall, BlockFace.Wall_NegativeX_Middle.GetFaceType());
		Assert.AreEqual(BlockFaceType.Wall, BlockFace.Wall_PositiveX_Middle.GetFaceType());
		Assert.AreEqual(BlockFaceType.Wall, BlockFace.Wall_Diagonal_Middle.GetFaceType());

		// Exceptions

		Assert.ThrowsException<ArgumentException>(() => BlockFace.Floor.GetFaceType());
		Assert.ThrowsException<ArgumentException>(() => BlockFace.Floor_Triangle2.GetFaceType());

		Assert.ThrowsException<ArgumentException>(() => BlockFace.Ceiling.GetFaceType());
		Assert.ThrowsException<ArgumentException>(() => BlockFace.Ceiling_Triangle2.GetFaceType());

		Assert.ThrowsException<ArgumentException>(() => BlockFace.Count.GetFaceType());
	}

	[TestMethod]
	public void GetDirection()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveZ")))
			Assert.AreEqual(Direction.PositiveZ, face.GetDirection());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeZ")))
			Assert.AreEqual(Direction.NegativeZ, face.GetDirection());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeX")))
			Assert.AreEqual(Direction.NegativeX, face.GetDirection());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveX")))
			Assert.AreEqual(Direction.PositiveX, face.GetDirection());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Diagonal")))
			Assert.AreEqual(Direction.Diagonal, face.GetDirection());

		Assert.AreEqual(Direction.None, BlockFace.Floor.GetDirection());
		Assert.AreEqual(Direction.None, BlockFace.Floor_Triangle2.GetDirection());

		Assert.AreEqual(Direction.None, BlockFace.Ceiling.GetDirection());
		Assert.AreEqual(Direction.None, BlockFace.Ceiling_Triangle2.GetDirection());

		Assert.AreEqual(Direction.None, BlockFace.Count.GetDirection());
	}

	[TestMethod]
	public void IsWall()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsTrue(face.IsWall());

		Assert.IsFalse(BlockFace.Floor.IsWall());
		Assert.IsFalse(BlockFace.Floor_Triangle2.IsWall());

		Assert.IsFalse(BlockFace.Ceiling.IsWall());
		Assert.IsFalse(BlockFace.Ceiling_Triangle2.IsWall());
	}

	[TestMethod]
	public void IsNonWall()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsFalse(face.IsNonWall());

		Assert.IsTrue(BlockFace.Floor.IsNonWall());
		Assert.IsTrue(BlockFace.Floor_Triangle2.IsNonWall());

		Assert.IsTrue(BlockFace.Ceiling.IsNonWall());
		Assert.IsTrue(BlockFace.Ceiling_Triangle2.IsNonWall());
	}

	[TestMethod]
	public void IsNonDiagonalWall()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.AreEqual(!face.ToString().Contains("Diagonal"), face.IsNonDiagonalWall());
	}

	[TestMethod]
	public void IsPositiveX()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveX")))
			Assert.IsTrue(face.IsPositiveX());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("PositiveX")))
			Assert.IsFalse(face.IsPositiveX());
	}

	[TestMethod]
	public void IsNegativeX()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeX")))
			Assert.IsTrue(face.IsNegativeX());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("NegativeX")))
			Assert.IsFalse(face.IsNegativeX());
	}

	[TestMethod]
	public void IsPositiveZ()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveZ")))
			Assert.IsTrue(face.IsPositiveZ());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("PositiveZ")))
			Assert.IsFalse(face.IsPositiveZ());
	}

	[TestMethod]
	public void IsNegativeZ()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeZ")))
			Assert.IsTrue(face.IsNegativeZ());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("NegativeZ")))
			Assert.IsFalse(face.IsNegativeZ());
	}

	[TestMethod]
	public void IsDiagonal()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Diagonal")))
			Assert.IsTrue(face.IsDiagonal());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("Diagonal")))
			Assert.IsFalse(face.IsDiagonal());
	}

	[TestMethod]
	public void IsFloorWall()
	{
		Assert.IsTrue(BlockFace.Wall_PositiveZ_QA.IsFloorWall());
		Assert.IsTrue(BlockFace.Wall_NegativeZ_QA.IsFloorWall());
		Assert.IsTrue(BlockFace.Wall_NegativeX_QA.IsFloorWall());
		Assert.IsTrue(BlockFace.Wall_PositiveX_QA.IsFloorWall());
		Assert.IsTrue(BlockFace.Wall_Diagonal_QA.IsFloorWall());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsTrue(face.IsFloorWall());

		Assert.IsFalse(BlockFace.Wall_PositiveZ_WS.IsFloorWall());
		Assert.IsFalse(BlockFace.Wall_NegativeZ_WS.IsFloorWall());
		Assert.IsFalse(BlockFace.Wall_NegativeX_WS.IsFloorWall());
		Assert.IsFalse(BlockFace.Wall_PositiveX_WS.IsFloorWall());
		Assert.IsFalse(BlockFace.Wall_Diagonal_WS.IsFloorWall());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsFalse(face.IsFloorWall());

		Assert.IsFalse(BlockFace.Floor.IsFloorWall());
		Assert.IsFalse(BlockFace.Floor_Triangle2.IsFloorWall());

		Assert.IsFalse(BlockFace.Ceiling.IsFloorWall());
		Assert.IsFalse(BlockFace.Ceiling_Triangle2.IsFloorWall());
	}

	[TestMethod]
	public void IsCeilingWall()
	{
		Assert.IsTrue(BlockFace.Wall_PositiveZ_WS.IsCeilingWall());
		Assert.IsTrue(BlockFace.Wall_NegativeZ_WS.IsCeilingWall());
		Assert.IsTrue(BlockFace.Wall_NegativeX_WS.IsCeilingWall());
		Assert.IsTrue(BlockFace.Wall_PositiveX_WS.IsCeilingWall());
		Assert.IsTrue(BlockFace.Wall_Diagonal_WS.IsCeilingWall());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsTrue(face.IsCeilingWall());

		Assert.IsFalse(BlockFace.Wall_PositiveZ_QA.IsCeilingWall());
		Assert.IsFalse(BlockFace.Wall_NegativeZ_QA.IsCeilingWall());
		Assert.IsFalse(BlockFace.Wall_NegativeX_QA.IsCeilingWall());
		Assert.IsFalse(BlockFace.Wall_PositiveX_QA.IsCeilingWall());
		Assert.IsFalse(BlockFace.Wall_Diagonal_QA.IsCeilingWall());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsFalse(face.IsCeilingWall());

		Assert.IsFalse(BlockFace.Floor.IsCeilingWall());
		Assert.IsFalse(BlockFace.Floor_Triangle2.IsCeilingWall());

		Assert.IsFalse(BlockFace.Ceiling.IsCeilingWall());
		Assert.IsFalse(BlockFace.Ceiling_Triangle2.IsCeilingWall());
	}

	[TestMethod]
	public void IsMiddleWall()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Middle")))
			Assert.IsTrue(face.IsMiddleWall());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("Middle")))
			Assert.IsFalse(face.IsMiddleWall());
	}

	[TestMethod]
	public void IsFloor()
	{
		Assert.IsTrue(BlockFace.Floor.IsFloor());
		Assert.IsTrue(BlockFace.Floor_Triangle2.IsFloor());

		Assert.IsFalse(BlockFace.Ceiling.IsFloor());
		Assert.IsFalse(BlockFace.Ceiling_Triangle2.IsFloor());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsFalse(face.IsFloor());
	}

	[TestMethod]
	public void IsCeiling()
	{
		Assert.IsTrue(BlockFace.Ceiling.IsCeiling());
		Assert.IsTrue(BlockFace.Ceiling_Triangle2.IsCeiling());

		Assert.IsFalse(BlockFace.Floor.IsCeiling());
		Assert.IsFalse(BlockFace.Floor_Triangle2.IsCeiling());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Wall")))
			Assert.IsFalse(face.IsCeiling());
	}

	[TestMethod]
	public void IsExtraFloorSubdivision()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsTrue(face.IsExtraFloorSubdivision());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("FloorSubdivision")))
			Assert.IsFalse(face.IsExtraFloorSubdivision());
	}

	[TestMethod]
	public void IsExtraCeilingSubdivision()
	{
		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsTrue(face.IsExtraCeilingSubdivision());

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("CeilingSubdivision")))
			Assert.IsFalse(face.IsExtraCeilingSubdivision());
	}

	[TestMethod]
	public void IsSpecificFloorSubdivision()
	{
		// True

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveZ_FloorSubdivision")))
			Assert.IsTrue(face.IsSpecificFloorSubdivision(Direction.PositiveZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeZ_FloorSubdivision")))
			Assert.IsTrue(face.IsSpecificFloorSubdivision(Direction.NegativeZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeX_FloorSubdivision")))
			Assert.IsTrue(face.IsSpecificFloorSubdivision(Direction.NegativeX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveX_FloorSubdivision")))
			Assert.IsTrue(face.IsSpecificFloorSubdivision(Direction.PositiveX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Diagonal_FloorSubdivision")))
			Assert.IsTrue(face.IsSpecificFloorSubdivision(Direction.Diagonal));

		// False

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("PositiveZ_FloorSubdivision")))
			Assert.IsFalse(face.IsSpecificFloorSubdivision(Direction.PositiveZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("NegativeZ_FloorSubdivision")))
			Assert.IsFalse(face.IsSpecificFloorSubdivision(Direction.NegativeZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("NegativeX_FloorSubdivision")))
			Assert.IsFalse(face.IsSpecificFloorSubdivision(Direction.NegativeX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("PositiveX_FloorSubdivision")))
			Assert.IsFalse(face.IsSpecificFloorSubdivision(Direction.PositiveX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("Diagonal_FloorSubdivision")))
			Assert.IsFalse(face.IsSpecificFloorSubdivision(Direction.Diagonal));
	}

	[TestMethod]
	public void IsSpecificCeilingSubdivision()
	{
		// True

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveZ_CeilingSubdivision")))
			Assert.IsTrue(face.IsSpecificCeilingSubdivision(Direction.PositiveZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeZ_CeilingSubdivision")))
			Assert.IsTrue(face.IsSpecificCeilingSubdivision(Direction.NegativeZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("NegativeX_CeilingSubdivision")))
			Assert.IsTrue(face.IsSpecificCeilingSubdivision(Direction.NegativeX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("PositiveX_CeilingSubdivision")))
			Assert.IsTrue(face.IsSpecificCeilingSubdivision(Direction.PositiveX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => f.ToString().Contains("Diagonal_CeilingSubdivision")))
			Assert.IsTrue(face.IsSpecificCeilingSubdivision(Direction.Diagonal));

		// False

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("PositiveZ_CeilingSubdivision")))
			Assert.IsFalse(face.IsSpecificCeilingSubdivision(Direction.PositiveZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("NegativeZ_CeilingSubdivision")))
			Assert.IsFalse(face.IsSpecificCeilingSubdivision(Direction.NegativeZ));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("NegativeX_CeilingSubdivision")))
			Assert.IsFalse(face.IsSpecificCeilingSubdivision(Direction.NegativeX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("PositiveX_CeilingSubdivision")))
			Assert.IsFalse(face.IsSpecificCeilingSubdivision(Direction.PositiveX));

		foreach (BlockFace face in Enum.GetValues<BlockFace>().Where(f => !f.ToString().Contains("Diagonal_CeilingSubdivision")))
			Assert.IsFalse(face.IsSpecificCeilingSubdivision(Direction.Diagonal));
	}

	[TestMethod]
	public void GetExtraFloorSubdivisionFace()
	{
		// PositiveZ

		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision2, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 0));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision3, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 1));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision4, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 2));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision5, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 3));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision6, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 4));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision7, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 5));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision8, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 6));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_FloorSubdivision9, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, 7));

		// NegativeZ

		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision2, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 0));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision3, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 1));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision4, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 2));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision5, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 3));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision6, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 4));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision7, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 5));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision8, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 6));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_FloorSubdivision9, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, 7));

		// NegativeX

		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision2, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 0));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision3, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 1));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision4, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 2));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision5, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 3));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision6, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 4));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision7, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 5));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision8, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 6));
		Assert.AreEqual(BlockFace.Wall_NegativeX_FloorSubdivision9, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, 7));

		// PositiveX

		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision2, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 0));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision3, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 1));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision4, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 2));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision5, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 3));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision6, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 4));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision7, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 5));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision8, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 6));
		Assert.AreEqual(BlockFace.Wall_PositiveX_FloorSubdivision9, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, 7));

		// Diagonal

		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision2, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 0));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision3, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 1));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision4, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 2));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision5, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 3));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision6, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 4));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision7, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 5));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision8, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 6));
		Assert.AreEqual(BlockFace.Wall_Diagonal_FloorSubdivision9, BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.Diagonal, 7));

		// Exceptions

		Assert.ThrowsException<ArgumentException>(() => BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.None, 0));
	}

	[TestMethod]
	public void GetExtraCeilingSubdivisionFace()
	{
		// PositiveZ

		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision2, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 0));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision3, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 1));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision4, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 2));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision5, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 3));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision6, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 4));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision7, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 5));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision8, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 6));
		Assert.AreEqual(BlockFace.Wall_PositiveZ_CeilingSubdivision9, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, 7));

		// NegativeZ

		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision2, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 0));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision3, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 1));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision4, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 2));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision5, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 3));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision6, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 4));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision7, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 5));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision8, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 6));
		Assert.AreEqual(BlockFace.Wall_NegativeZ_CeilingSubdivision9, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, 7));

		// NegativeX

		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision2, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 0));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision3, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 1));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision4, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 2));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision5, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 3));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision6, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 4));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision7, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 5));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision8, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 6));
		Assert.AreEqual(BlockFace.Wall_NegativeX_CeilingSubdivision9, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, 7));

		// PositiveX

		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision2, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 0));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision3, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 1));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision4, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 2));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision5, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 3));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision6, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 4));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision7, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 5));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision8, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 6));
		Assert.AreEqual(BlockFace.Wall_PositiveX_CeilingSubdivision9, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, 7));

		// Diagonal

		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision2, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 0));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision3, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 1));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision4, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 2));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision5, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 3));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision6, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 4));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision7, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 5));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision8, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 6));
		Assert.AreEqual(BlockFace.Wall_Diagonal_CeilingSubdivision9, BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.Diagonal, 7));

		// Exceptions

		Assert.ThrowsException<ArgumentException>(() => BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.None, 0));
	}
}
