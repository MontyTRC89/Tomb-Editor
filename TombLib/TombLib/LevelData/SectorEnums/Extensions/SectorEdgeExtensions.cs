namespace TombLib.LevelData.SectorEnums.Extensions;

public static class SectorEdgeExtensions
{
	public static int DirectionX(this SectorEdge edge)
		=> edge is SectorEdge.XpZn or SectorEdge.XpZp ? 1 : 0;

	public static int DirectionZ(this SectorEdge edge)
		=> edge is SectorEdge.XnZp or SectorEdge.XpZp ? 1 : 0;
}
