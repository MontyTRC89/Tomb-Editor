namespace TombLib.LevelData;

/// <summary>
/// Represents a wall start or end point from top-down view.
/// </summary>
public struct WallEnd
{
	/// <summary>
	/// X coordinate of the wall point.
	/// </summary>
	public int X;

	/// <summary>
	/// Z coordinate of the wall point.
	/// </summary>
	public int Z;

	/// <summary>
	/// Minimum Y coordinate (height) of the wall point.
	/// </summary>
	public int MinY;

	/// <summary>
	/// Maximum Y coordinate (height) of the wall point.
	/// </summary>
	public int MaxY;
}
