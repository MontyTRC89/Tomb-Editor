namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// A pair of a room and a sector, with the sector's position in the room.
/// </summary>
public struct RoomSectorPair
{
	public Room Room { get; set; }
	public Sector Sector { get; set; }
	public VectorInt2 SectorPosition { get; set; }
}
