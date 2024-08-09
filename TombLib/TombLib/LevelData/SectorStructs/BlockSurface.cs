using System;
using System.Numerics;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

[Serializable]
public struct BlockSurface
{
	public bool SplitDirectionToggled;
	public DiagonalSplit DiagonalSplit;
	public int XnZp;
	public int XpZp;
	public int XpZn;
	public int XnZn;

	public bool IsQuad => DiagonalSplit == DiagonalSplit.None && IsQuad2(XnZp, XpZp, XpZn, XnZn);

	public bool HasSlope => DiagonalSplit switch
	{
		DiagonalSplit.XnZp => Math.Abs(XnZp - Math.Min(XnZn, XpZp)) >= Clicks.ToWorld(3),
		DiagonalSplit.XpZp => Math.Abs(XpZp - Math.Min(XnZp, XpZn)) >= Clicks.ToWorld(3),
		DiagonalSplit.XpZn => Math.Abs(XpZn - Math.Min(XnZn, XpZp)) >= Clicks.ToWorld(3),
		DiagonalSplit.XnZn => Math.Abs(XnZn - Math.Min(XnZp, XpZn)) >= Clicks.ToWorld(3),
		_ => Max - Min >= Clicks.ToWorld(3),
	};

	public int IfQuadSlopeX => IsQuad ? XpZp - XnZp : 0;
	public int IfQuadSlopeZ => IsQuad ? XpZp - XpZn : 0;
	public int Max => Math.Max(Math.Max(XnZp, XpZp), Math.Max(XpZn, XnZn));
	public int Min => Math.Min(Math.Min(XnZp, XpZp), Math.Min(XpZn, XnZn));

	public int GetHeight(BlockEdge edge)
	{
		switch (edge)
		{
			case BlockEdge.XnZp:
				return XnZp;

			case BlockEdge.XpZp:
				return XpZp;

			case BlockEdge.XpZn:
				return XpZn;

			case BlockEdge.XnZn:
				return XnZn;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void SetHeight(BlockEdge edge, int value)
	{
		switch (edge)
		{
			case BlockEdge.XnZp:
				XnZp = value;
				return;

			case BlockEdge.XpZp:
				XpZp = value;
				return;

			case BlockEdge.XpZn:
				XpZn = value;
				return;

			case BlockEdge.XnZn:
				XnZn = value;
				return;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void SetHeight(int value)
	{
		SetHeight(BlockEdge.XnZn, value);
		SetHeight(BlockEdge.XnZp, value);
		SetHeight(BlockEdge.XpZn, value);
		SetHeight(BlockEdge.XpZp, value);
	}

	public static bool IsQuad2(int hXnZp, int hXpZp, int hXpZn, int hXnZn)
	{
		return hXpZp - hXpZn == hXnZp - hXnZn &&
			hXpZp - hXnZp == hXpZn - hXnZn;
	}

	public bool SplitDirectionIsXEqualsZ
	{
		get
		{
			var p1 = new Vector3(0, XnZp, 0);
			var p2 = new Vector3(1, XpZp, 0);
			var p3 = new Vector3(1, XpZn, 1);
			var p4 = new Vector3(0, XnZn, 1);

			var plane = Plane.CreateFromVertices(p1, p2, p4);
			if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
				return !SplitDirectionToggled;

			plane = Plane.CreateFromVertices(p1, p2, p3);
			if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
				return SplitDirectionToggled;

			plane = Plane.CreateFromVertices(p2, p3, p4);
			if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
				return !SplitDirectionToggled;

			plane = Plane.CreateFromVertices(p3, p4, p1);
			if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
				return SplitDirectionToggled;

			// Otherwise
			int min = Math.Min(Math.Min(Math.Min(XnZp, XpZp), XpZn), XnZn);
			int max = Math.Max(Math.Max(Math.Max(XnZp, XpZp), XpZn), XnZn);

			if (XnZp == XpZn && XpZp == XnZn && XpZp != XpZn)
				return SplitDirectionToggled;

			if (min == XnZp && min == XpZn)
				return SplitDirectionToggled;
			if (min == XpZp && min == XnZn)
				return !SplitDirectionToggled;

			if (min == XnZp && max == XpZn)
				return !SplitDirectionToggled;
			if (min == XpZp && max == XnZn)
				return SplitDirectionToggled;
			if (min == XpZn && max == XnZp)
				return !SplitDirectionToggled;
			if (min == XnZn && max == XpZp)
				return SplitDirectionToggled;

			return !SplitDirectionToggled;
		}
		set
		{
			if (value != SplitDirectionIsXEqualsZ)
				SplitDirectionToggled = !SplitDirectionToggled;
		}
	}

	/// <summary>Checks for DiagonalSplit and takes priority</summary>
	public bool SplitDirectionIsXEqualsZWithDiagonalSplit
	{
		get
		{
			switch (DiagonalSplit)
			{
				case DiagonalSplit.XnZn:
				case DiagonalSplit.XpZp:
					return false;

				case DiagonalSplit.XpZn:
				case DiagonalSplit.XnZp:
					return true;

				case DiagonalSplit.None:
					return SplitDirectionIsXEqualsZ;

				default:
					throw new ApplicationException("\"DiagonalSplit\" in unknown state.");
			}
		}
	}

	/// <summary>Returns the height of the 4 edges if the sector is split</summary>
	public int GetActualMax(BlockEdge edge)
	{
		switch (DiagonalSplit)
		{
			case DiagonalSplit.None:
				return GetHeight(edge);

			case DiagonalSplit.XnZn:
				if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
					return Math.Max(GetHeight(edge), XpZp);
				return GetHeight(edge);

			case DiagonalSplit.XpZp:
				if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
					return Math.Max(GetHeight(edge), XnZn);
				return GetHeight(edge);

			case DiagonalSplit.XpZn:
				if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
					return Math.Max(GetHeight(edge), XnZp);
				return GetHeight(edge);

			case DiagonalSplit.XnZp:
				if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
					return Math.Max(GetHeight(edge), XpZn);
				return GetHeight(edge);

			default:
				throw new ApplicationException("\"splitType\" in unknown state.");
		}
	}

	/// <summary>Returns the height of the 4 edges if the sector is split</summary>
	public int GetActualMin(BlockEdge edge)
	{
		switch (DiagonalSplit)
		{
			case DiagonalSplit.None:
				return GetHeight(edge);

			case DiagonalSplit.XnZn:
				if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
					return Math.Min(GetHeight(edge), XpZp);
				return GetHeight(edge);

			case DiagonalSplit.XpZp:
				if (edge == BlockEdge.XnZp || edge == BlockEdge.XpZn)
					return Math.Min(GetHeight(edge), XnZn);
				return GetHeight(edge);

			case DiagonalSplit.XpZn:
				if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
					return Math.Min(GetHeight(edge), XnZp);
				return GetHeight(edge);

			case DiagonalSplit.XnZp:
				if (edge == BlockEdge.XpZp || edge == BlockEdge.XnZn)
					return Math.Min(GetHeight(edge), XpZn);
				return GetHeight(edge);

			default:
				throw new ApplicationException("\"splitType\" in unknown state.");
		}
	}

	public static BlockSurface operator +(BlockSurface first, BlockSurface second)
		=> new BlockSurface() { XpZp = first.XpZp + second.XpZp, XpZn = first.XpZn + second.XpZn, XnZp = first.XnZp + second.XnZp, XnZn = first.XnZn + second.XnZn };
	public static BlockSurface operator -(BlockSurface first, BlockSurface second)
		=> new BlockSurface() { XpZp = first.XpZp - second.XpZp, XpZn = first.XpZn - second.XpZn, XnZp = first.XnZp - second.XnZp, XnZn = first.XnZn - second.XnZn };
}
