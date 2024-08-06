namespace TombLib.LevelData.SectorEnums;

public enum BlockEdge : byte
{
	/// <summary> Index of edges on the negative X and positive Z direction </summary>
	XnZp,

	/// <summary> Index of edges on the positive X and positive Z direction </summary>
	XpZp,

	/// <summary> Index of edges on the positive X and negative Z direction </summary>
	XpZn,

	/// <summary> Index of edges on the negative X and negative Z direction </summary>
	XnZn,

	Count
}
