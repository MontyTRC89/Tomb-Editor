namespace TombLib.LevelData.SectorEnums.Extensions;

public static class BlockVerticalExtensions
{
	public static bool IsOnFloor(this BlockVertical vertical)
		=> vertical is BlockVertical.Floor || vertical.IsExtraFloorSubdivision();

	public static bool IsOnCeiling(this BlockVertical vertical)
		=> vertical is BlockVertical.Ceiling || vertical.IsExtraCeilingSubdivision();

	public static bool IsExtraFloorSubdivision(this BlockVertical vertical) => vertical
		is BlockVertical.FloorSubdivision2
		or BlockVertical.FloorSubdivision3
		or BlockVertical.FloorSubdivision4
		or BlockVertical.FloorSubdivision5
		or BlockVertical.FloorSubdivision6
		or BlockVertical.FloorSubdivision7
		or BlockVertical.FloorSubdivision8
		or BlockVertical.FloorSubdivision9;

	public static bool IsExtraCeilingSubdivision(this BlockVertical vertical) => vertical
		is BlockVertical.CeilingSubdivision2
		or BlockVertical.CeilingSubdivision3
		or BlockVertical.CeilingSubdivision4
		or BlockVertical.CeilingSubdivision5
		or BlockVertical.CeilingSubdivision6
		or BlockVertical.CeilingSubdivision7
		or BlockVertical.CeilingSubdivision8
		or BlockVertical.CeilingSubdivision9;

	public static bool IsExtraSubdivision(this BlockVertical vertical)
		=> vertical.IsExtraFloorSubdivision() || vertical.IsExtraCeilingSubdivision();

	public static BlockVertical GetExtraFloorSubdivision(int subdivisionIndex)
		=> (BlockVertical)((int)BlockVertical.FloorSubdivision2 + (subdivisionIndex * 2));

	public static BlockVertical GetExtraCeilingSubdivision(int subdivisionIndex)
		=> (BlockVertical)((int)BlockVertical.CeilingSubdivision2 + (subdivisionIndex * 2));

	public static int GetExtraSubdivisionIndex(this BlockVertical vertical)
		=> ((int)vertical / 2) - 1;
}
