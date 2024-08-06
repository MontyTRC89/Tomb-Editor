using System;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

public class Subdivision : ICloneable
{
	public int XnZp;
	public int XpZp;
	public int XpZn;
	public int XnZn;

	public int Min
		=> Math.Min(Math.Min(XnZp, XpZp), Math.Min(XpZn, XnZn));

	public int Max
		=> Math.Max(Math.Max(XnZp, XpZp), Math.Max(XpZn, XnZn));

	public Subdivision()
	{ }

	public Subdivision(int uniformEdgeY)
		=> XnZp = XpZp = XpZn = XnZn = uniformEdgeY;

	public Subdivision(int xnzp, int xpzp, int xpzn, int xnzn)
	{
		XnZp = xnzp;
		XpZp = xpzp;
		XpZn = xpzn;
		XnZn = xnzn;
	}

	public int GetEdge(BlockEdge edge) => edge switch
	{
		BlockEdge.XnZp => XnZp,
		BlockEdge.XpZp => XpZp,
		BlockEdge.XpZn => XpZn,
		BlockEdge.XnZn => XnZn,
		_ => throw new ArgumentException()
	};

	public void SetEdge(BlockEdge edge, int value)
	{
		switch (edge)
		{
			case BlockEdge.XnZp:
				XnZp = value;
				break;

			case BlockEdge.XpZp:
				XpZp = value;
				break;

			case BlockEdge.XpZn:
				XpZn = value;
				break;

			case BlockEdge.XnZn:
				XnZn = value;
				break;

			default:
				throw new ArgumentException();
		}
	}

	public object Clone()
		=> new Subdivision(XnZp, XpZp, XpZn, XnZn);
}
