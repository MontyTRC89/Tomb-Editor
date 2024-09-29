namespace TombLib.LevelData.SectorEnums.Extensions;

public static class SectorVerticalPartExtensions
{
	public static bool IsOnFloor(this SectorVerticalPart vertical)
		=> vertical is SectorVerticalPart.QA || vertical.IsExtraFloorSplit();

	public static bool IsOnCeiling(this SectorVerticalPart vertical)
		=> vertical is SectorVerticalPart.WS || vertical.IsExtraCeilingSplit();

	public static bool IsExtraFloorSplit(this SectorVerticalPart vertical) => vertical
		is SectorVerticalPart.Floor2
		or SectorVerticalPart.Floor3
		or SectorVerticalPart.Floor4
		or SectorVerticalPart.Floor5
		or SectorVerticalPart.Floor6
		or SectorVerticalPart.Floor7
		or SectorVerticalPart.Floor8
		or SectorVerticalPart.Floor9;

	public static bool IsExtraCeilingSplit(this SectorVerticalPart vertical) => vertical
		is SectorVerticalPart.Ceiling2
		or SectorVerticalPart.Ceiling3
		or SectorVerticalPart.Ceiling4
		or SectorVerticalPart.Ceiling5
		or SectorVerticalPart.Ceiling6
		or SectorVerticalPart.Ceiling7
		or SectorVerticalPart.Ceiling8
		or SectorVerticalPart.Ceiling9;

	public static bool IsExtraSplit(this SectorVerticalPart vertical)
		=> vertical.IsExtraFloorSplit() || vertical.IsExtraCeilingSplit();

	public static SectorVerticalPart GetExtraFloorSplit(int splitIndex)
		=> (SectorVerticalPart)((int)SectorVerticalPart.Floor2 + (splitIndex * 2));

	public static SectorVerticalPart GetExtraCeilingSplit(int splitIndex)
		=> (SectorVerticalPart)((int)SectorVerticalPart.Ceiling2 + (splitIndex * 2));

	public static int GetExtraSplitIndex(this SectorVerticalPart vertical)
		=> ((int)vertical / 2) - 1;
}
