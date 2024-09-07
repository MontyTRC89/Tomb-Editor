namespace TombLib.LevelData.SectorEnums;

/// <summary>
/// Whether the sector can be walked on, is a wall or a room border.
/// </summary>
public enum SectorType : byte
{
	/// <summary>
	/// Player can walk on this sector.
	/// </summary>
	Floor,

	/// <summary>
	/// Wall sector (green square on the sector mini-map).
	/// </summary>
	Wall,

	/// <summary>
	/// Room border sector (gray square on the sector mini-map).
	/// </summary>
	BorderWall
}
