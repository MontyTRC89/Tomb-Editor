namespace TombLib.LevelData.Geometry;

/// <summary>
/// Represents a wall split, such as QA, WS, ED, RF or any other extra subdivision of a wall.
/// </summary>
public struct WallSplit
{
	/// <summary>
	/// Y coordinate of the start of the wall split.
	/// </summary>
	public int StartY;

	/// <summary>
	/// Y coordinate of the end of the wall split.
	/// </summary>
	public int EndY;

	public WallSplit(int startY, int endY)
	{
		StartY = startY;
		EndY = endY;
	}
}
