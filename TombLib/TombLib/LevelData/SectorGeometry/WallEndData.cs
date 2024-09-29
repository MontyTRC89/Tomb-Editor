namespace TombLib.LevelData.SectorGeometry;

/// <summary>
/// Represents a wall start or end point from top-down view.
/// </summary>
public readonly struct WallEndData
{
	/// <summary>
	/// X coordinate of the wall point.
	/// </summary>
	public readonly int X;

	/// <summary>
	/// Z coordinate of the wall point.
	/// </summary>
	public readonly int Z;

	/// <summary>
	/// Minimum Y coordinate (height) of the wall point.
	/// </summary>
	public readonly int MinY;

	/// <summary>
	/// Maximum Y coordinate (height) of the wall point.
	/// </summary>
	public readonly int MaxY;

	public WallEndData(int x, int z, int minY, int maxY)
	{
		X = x;
		Z = z;
		MinY = minY;
		MaxY = maxY;
	}
}
