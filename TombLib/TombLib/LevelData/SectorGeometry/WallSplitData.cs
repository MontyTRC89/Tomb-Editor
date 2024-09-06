namespace TombLib.LevelData.SectorGeometry;

/// <summary>
/// Represents a wall split, such as QA, WS, ED, RF or any other extra splits of a wall.
/// </summary>
public readonly struct WallSplitData
{
	/// <summary>
	/// Y coordinate of the start of the wall split.
	/// </summary>
	public readonly int StartY;

	/// <summary>
	/// Y coordinate of the end of the wall split.
	/// </summary>
	public readonly int EndY;

	public WallSplitData(int startY, int endY)
	{
		StartY = startY;
		EndY = endY;
	}
}
