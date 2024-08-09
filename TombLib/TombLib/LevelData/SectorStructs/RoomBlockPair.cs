namespace TombLib.LevelData.SectorStructs;

public struct RoomBlockPair
{
	public Room Room { get; set; }
	public Block Block { get; set; }
	public VectorInt2 Pos { get; set; }
}
