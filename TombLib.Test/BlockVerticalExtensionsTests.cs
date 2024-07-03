using TombLib.LevelData;

namespace TombLib.Test;

[TestClass]
public class BlockVerticalExtensionsTests
{
	[TestMethod]
	public void IsOnFloor()
	{
		Assert.IsTrue(BlockVertical.Floor.IsOnFloor());
		Assert.IsFalse(BlockVertical.Ceiling.IsOnFloor());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsTrue(vertical.IsOnFloor());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsFalse(vertical.IsOnFloor());
	}

	[TestMethod]
	public void IsOnCeiling()
	{
		Assert.IsTrue(BlockVertical.Ceiling.IsOnCeiling());
		Assert.IsFalse(BlockVertical.Floor.IsOnCeiling());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsTrue(vertical.IsOnCeiling());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsFalse(vertical.IsOnCeiling());
	}

	[TestMethod]
	public void IsExtraFloorSubdivision()
	{
		Assert.IsFalse(BlockVertical.Floor.IsExtraFloorSubdivision());
		Assert.IsFalse(BlockVertical.Ceiling.IsExtraFloorSubdivision());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsTrue(vertical.IsExtraFloorSubdivision());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsFalse(vertical.IsExtraFloorSubdivision());
	}

	[TestMethod]
	public void IsExtraCeilingSubdivision()
	{
		Assert.IsFalse(BlockVertical.Floor.IsExtraCeilingSubdivision());
		Assert.IsFalse(BlockVertical.Ceiling.IsExtraCeilingSubdivision());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("CeilingSubdivision")))
			Assert.IsTrue(vertical.IsExtraCeilingSubdivision());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("FloorSubdivision")))
			Assert.IsFalse(vertical.IsExtraCeilingSubdivision());
	}

	[TestMethod]
	public void IsExtraSubdivision()
	{
		Assert.IsFalse(BlockVertical.Floor.IsExtraSubdivision());
		Assert.IsFalse(BlockVertical.Ceiling.IsExtraSubdivision());

		foreach (BlockVertical vertical in Enum.GetValues<BlockVertical>().Where(f => f.ToString().Contains("Subdivision")))
			Assert.IsTrue(vertical.IsExtraSubdivision());
	}

	[TestMethod]
	public void GetExtraFloorSubdivision()
	{
		Assert.AreEqual(BlockVertical.FloorSubdivision2, BlockVerticalExtensions.GetExtraFloorSubdivision(0));
		Assert.AreEqual(BlockVertical.FloorSubdivision3, BlockVerticalExtensions.GetExtraFloorSubdivision(1));
		Assert.AreEqual(BlockVertical.FloorSubdivision4, BlockVerticalExtensions.GetExtraFloorSubdivision(2));
		Assert.AreEqual(BlockVertical.FloorSubdivision5, BlockVerticalExtensions.GetExtraFloorSubdivision(3));
		Assert.AreEqual(BlockVertical.FloorSubdivision6, BlockVerticalExtensions.GetExtraFloorSubdivision(4));
		Assert.AreEqual(BlockVertical.FloorSubdivision7, BlockVerticalExtensions.GetExtraFloorSubdivision(5));
		Assert.AreEqual(BlockVertical.FloorSubdivision8, BlockVerticalExtensions.GetExtraFloorSubdivision(6));
		Assert.AreEqual(BlockVertical.FloorSubdivision9, BlockVerticalExtensions.GetExtraFloorSubdivision(7));
	}

	[TestMethod]
	public void GetExtraCeilingSubdivision()
	{
		Assert.AreEqual(BlockVertical.CeilingSubdivision2, BlockVerticalExtensions.GetExtraCeilingSubdivision(0));
		Assert.AreEqual(BlockVertical.CeilingSubdivision3, BlockVerticalExtensions.GetExtraCeilingSubdivision(1));
		Assert.AreEqual(BlockVertical.CeilingSubdivision4, BlockVerticalExtensions.GetExtraCeilingSubdivision(2));
		Assert.AreEqual(BlockVertical.CeilingSubdivision5, BlockVerticalExtensions.GetExtraCeilingSubdivision(3));
		Assert.AreEqual(BlockVertical.CeilingSubdivision6, BlockVerticalExtensions.GetExtraCeilingSubdivision(4));
		Assert.AreEqual(BlockVertical.CeilingSubdivision7, BlockVerticalExtensions.GetExtraCeilingSubdivision(5));
		Assert.AreEqual(BlockVertical.CeilingSubdivision8, BlockVerticalExtensions.GetExtraCeilingSubdivision(6));
		Assert.AreEqual(BlockVertical.CeilingSubdivision9, BlockVerticalExtensions.GetExtraCeilingSubdivision(7));
	}

	[TestMethod]
	public void GetExtraSubdivisionIndex()
	{
		Assert.AreEqual(0, BlockVertical.FloorSubdivision2.GetExtraSubdivisionIndex());
		Assert.AreEqual(1, BlockVertical.FloorSubdivision3.GetExtraSubdivisionIndex());
		Assert.AreEqual(2, BlockVertical.FloorSubdivision4.GetExtraSubdivisionIndex());
		Assert.AreEqual(3, BlockVertical.FloorSubdivision5.GetExtraSubdivisionIndex());
		Assert.AreEqual(4, BlockVertical.FloorSubdivision6.GetExtraSubdivisionIndex());
		Assert.AreEqual(5, BlockVertical.FloorSubdivision7.GetExtraSubdivisionIndex());
		Assert.AreEqual(6, BlockVertical.FloorSubdivision8.GetExtraSubdivisionIndex());
		Assert.AreEqual(7, BlockVertical.FloorSubdivision9.GetExtraSubdivisionIndex());

		Assert.AreEqual(0, BlockVertical.CeilingSubdivision2.GetExtraSubdivisionIndex());
		Assert.AreEqual(1, BlockVertical.CeilingSubdivision3.GetExtraSubdivisionIndex());
		Assert.AreEqual(2, BlockVertical.CeilingSubdivision4.GetExtraSubdivisionIndex());
		Assert.AreEqual(3, BlockVertical.CeilingSubdivision5.GetExtraSubdivisionIndex());
		Assert.AreEqual(4, BlockVertical.CeilingSubdivision6.GetExtraSubdivisionIndex());
		Assert.AreEqual(5, BlockVertical.CeilingSubdivision7.GetExtraSubdivisionIndex());
		Assert.AreEqual(6, BlockVertical.CeilingSubdivision8.GetExtraSubdivisionIndex());
		Assert.AreEqual(7, BlockVertical.CeilingSubdivision9.GetExtraSubdivisionIndex());
	}
}
