using System;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// A horizontal split through a sector.
/// </summary>
public class SectorSplit : ICloneable
{
	/// <summary>
	/// Top-left corner of the split.
	/// </summary>
	public int XnZp;

	/// <summary>
	/// Top-right corner of the split.
	/// </summary>
	public int XpZp;

	/// <summary>
	/// Bottom-right corner of the split.
	/// </summary>
	public int XpZn;

	/// <summary>
	/// Bottom-left corner of the split.
	/// </summary>
	public int XnZn;

	/// <summary>
	/// The minimum value of all edges (lowest edge).
	/// </summary>
	public int Min
		=> Math.Min(Math.Min(XnZp, XpZp), Math.Min(XpZn, XnZn));

	/// <summary>
	/// The maximum value of all edges (highest edge).
	/// </summary>
	public int Max
		=> Math.Max(Math.Max(XnZp, XpZp), Math.Max(XpZn, XnZn));

	public SectorSplit()
	{ }

	public SectorSplit(int uniformEdgeY)
		=> XnZp = XpZp = XpZn = XnZn = uniformEdgeY;

	public SectorSplit(int xnzp, int xpzp, int xpzn, int xnzn)
	{
		XnZp = xnzp;
		XpZp = xpzp;
		XpZn = xpzn;
		XnZn = xnzn;
	}

	public int GetEdge(SectorEdge edge) => edge switch
	{
		SectorEdge.XnZp => XnZp,
		SectorEdge.XpZp => XpZp,
		SectorEdge.XpZn => XpZn,
		SectorEdge.XnZn => XnZn,
		_ => throw new ArgumentException()
	};

	public void SetEdge(SectorEdge edge, int value)
	{
		switch (edge)
		{
			case SectorEdge.XnZp:
				XnZp = value;
				break;

			case SectorEdge.XpZp:
				XpZp = value;
				break;

			case SectorEdge.XpZn:
				XpZn = value;
				break;

			case SectorEdge.XnZn:
				XnZn = value;
				break;

			default:
				throw new ArgumentException();
		}
	}

	public object Clone()
		=> new SectorSplit(XnZp, XpZp, XpZn, XnZn);
}
