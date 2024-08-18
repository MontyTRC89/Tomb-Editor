namespace TombLib.LevelData.SectorEnums;

public enum SectorEdge : byte
{
	/// <summary>
	/// Top-left corner of the sector (when viewed on the sector mini-map).
	/// <para>X Negative (left), Z Positive (top).</para>
	/// </summary>
	XnZp,

	/// <summary>
	/// Top-right corner of the sector (when viewed on the sector mini-map).
	/// <para>X Positive (right), Z Positive (top).</para>
	/// </summary>
	XpZp,

	/// <summary>
	/// Bottom-right corner of the sector (when viewed on the sector mini-map).
	/// <para>X Positive (right), Z Negative (bottom).</para>
	/// </summary>
	XpZn,

	/// <summary>
	/// Bottom-left corner of the sector (when viewed on the sector mini-map).
	/// <para>X Negative (left), Z Negative (bottom).</para>
	/// </summary>
	XnZn,

	Count
}
