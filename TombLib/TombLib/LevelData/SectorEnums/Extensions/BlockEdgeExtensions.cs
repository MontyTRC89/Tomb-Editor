namespace TombLib.LevelData.SectorEnums.Extensions;

public static class BlockEdgeExtensions
{
	public static int DirectionX(this BlockEdge edge)
		=> edge is BlockEdge.XpZn or BlockEdge.XpZp ? 1 : 0;

	public static int DirectionZ(this BlockEdge edge)
		=> edge is BlockEdge.XnZp or BlockEdge.XpZp ? 1 : 0;
}
