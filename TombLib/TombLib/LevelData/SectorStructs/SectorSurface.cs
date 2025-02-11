using System;
using System.Numerics;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// Floor or ceiling surface of a sector.
/// </summary>
public struct SectorSurface
{
	public bool SplitDirectionToggled;
	public DiagonalSplit DiagonalSplit;

	/// <summary>
	/// Top-left corner of the split (when viewed on the sector mini-map).
	/// <para>X Negative (left), Z Positive (top).</para>
	/// </summary>
	public int XnZp;

	/// <summary>
	/// Top-right corner of the split (when viewed on the sector mini-map).
	/// <para>X Positive (right), Z Positive (top).</para>
	/// </summary>
	public int XpZp;

	/// <summary>
	/// Bottom-right corner of the split (when viewed on the sector mini-map).
	/// <para>X Positive (right), Z Negative (bottom).</para>
	/// </summary>
	public int XpZn;

	/// <summary>
	/// Bottom-left corner of the split (when viewed on the sector mini-map).
	/// <para>X Negative (left), Z Negative (bottom).</para>
	/// </summary>
	public int XnZn;

	/// <summary>
	/// Whether the surface is steep enough to be a slope (3 clicks or more).
	/// </summary>
	public readonly bool HasSlope(bool isUsingClicks = false) => DiagonalSplit switch
	{
		DiagonalSplit.XnZp => Math.Abs(XnZp - Math.Min(XnZn, XpZp)) > (isUsingClicks ? 2 : Clicks.ToWorld(2)),
		DiagonalSplit.XpZp => Math.Abs(XpZp - Math.Min(XnZp, XpZn)) > (isUsingClicks ? 2 : Clicks.ToWorld(2)),
		DiagonalSplit.XpZn => Math.Abs(XpZn - Math.Min(XnZn, XpZp)) > (isUsingClicks ? 2 : Clicks.ToWorld(2)),
		DiagonalSplit.XnZn => Math.Abs(XnZn - Math.Min(XnZp, XpZn)) > (isUsingClicks ? 2 : Clicks.ToWorld(2)),
		_ => Max - Min > (isUsingClicks ? 2 : Clicks.ToWorld(2)),
	};

	/// <summary>
	/// Whether the surface is a quad that doesn't "fold" diagonally.
	/// </summary>
	public readonly bool IsQuad
		=> DiagonalSplit == DiagonalSplit.None && IsQuad2(XnZp, XpZp, XpZn, XnZn);

	public readonly int IfQuadSlopeX => IsQuad ? XpZp - XnZp : 0;
	public readonly int IfQuadSlopeZ => IsQuad ? XpZp - XpZn : 0;

	/// <summary>
	/// The minimum value of all edges (lowest edge).
	/// </summary>
	public readonly int Min
		=> Math.Min(Math.Min(XnZp, XpZp), Math.Min(XpZn, XnZn));

	/// <summary>
	/// The maximum value of all edges (highest edge).
	/// </summary>
	public readonly int Max
		=> Math.Max(Math.Max(XnZp, XpZp), Math.Max(XpZn, XnZn));

	public bool SplitDirectionIsXEqualsZ
	{
		readonly get
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

	/// <summary>
	/// Checks for DiagonalSplit and takes priority.
	/// </summary>
	public readonly bool SplitDirectionIsXEqualsZWithDiagonalSplit => DiagonalSplit switch
	{
		DiagonalSplit.XnZn or DiagonalSplit.XpZp => false,
		DiagonalSplit.XpZn or DiagonalSplit.XnZp => true,
		DiagonalSplit.None => SplitDirectionIsXEqualsZ,
		_ => throw new ApplicationException("\"DiagonalSplit\" in unknown state."),
	};

	public readonly int GetHeight(SectorEdge edge) => edge switch
	{
		SectorEdge.XnZp => XnZp,
		SectorEdge.XpZp => XpZp,
		SectorEdge.XpZn => XpZn,
		SectorEdge.XnZn => XnZn,
		_ => throw new ArgumentOutOfRangeException(),
	};

	/// <summary>
	/// Returns the height of the 4 edges if the sector is split.
	/// </summary>
	public readonly int GetActualMin(SectorEdge edge)
	{
		switch (DiagonalSplit)
		{
			case DiagonalSplit.None:
				return GetHeight(edge);

			case DiagonalSplit.XnZn:
				if (edge is SectorEdge.XnZp or SectorEdge.XpZn)
					return Math.Min(GetHeight(edge), XpZp);
				return GetHeight(edge);

			case DiagonalSplit.XpZp:
				if (edge is SectorEdge.XnZp or SectorEdge.XpZn)
					return Math.Min(GetHeight(edge), XnZn);
				return GetHeight(edge);

			case DiagonalSplit.XpZn:
				if (edge is SectorEdge.XpZp or SectorEdge.XnZn)
					return Math.Min(GetHeight(edge), XnZp);
				return GetHeight(edge);

			case DiagonalSplit.XnZp:
				if (edge is SectorEdge.XpZp or SectorEdge.XnZn)
					return Math.Min(GetHeight(edge), XpZn);
				return GetHeight(edge);

			default:
				throw new ApplicationException("\"splitType\" in unknown state.");
		}
	}

	/// <summary>
	/// Returns the height of the 4 edges if the sector is split.
	/// </summary>
	public readonly int GetActualMax(SectorEdge edge)
	{
		switch (DiagonalSplit)
		{
			case DiagonalSplit.None:
				return GetHeight(edge);

			case DiagonalSplit.XnZn:
				if (edge is SectorEdge.XnZp or SectorEdge.XpZn)
					return Math.Max(GetHeight(edge), XpZp);
				return GetHeight(edge);

			case DiagonalSplit.XpZp:
				if (edge is SectorEdge.XnZp or SectorEdge.XpZn)
					return Math.Max(GetHeight(edge), XnZn);
				return GetHeight(edge);

			case DiagonalSplit.XpZn:
				if (edge is SectorEdge.XpZp or SectorEdge.XnZn)
					return Math.Max(GetHeight(edge), XnZp);
				return GetHeight(edge);

			case DiagonalSplit.XnZp:
				if (edge is SectorEdge.XpZp or SectorEdge.XnZn)
					return Math.Max(GetHeight(edge), XpZn);
				return GetHeight(edge);

			default:
				throw new ApplicationException("\"splitType\" in unknown state.");
		}
	}

	public void SetHeight(SectorEdge edge, int value)
	{
		switch (edge)
		{
			case SectorEdge.XnZp:
				XnZp = value;
				return;

			case SectorEdge.XpZp:
				XpZp = value;
				return;

			case SectorEdge.XpZn:
				XpZn = value;
				return;

			case SectorEdge.XnZn:
				XnZn = value;
				return;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void SetHeight(int value)
	{
		SetHeight(SectorEdge.XnZn, value);
		SetHeight(SectorEdge.XnZp, value);
		SetHeight(SectorEdge.XpZn, value);
		SetHeight(SectorEdge.XpZp, value);
	}

	public static bool IsQuad2(int hXnZp, int hXpZp, int hXpZn, int hXnZn)
		=> hXpZp - hXpZn == hXnZp - hXnZn
		&& hXpZp - hXnZp == hXpZn - hXnZn;

	public static SectorSurface operator +(SectorSurface first, SectorSurface second)
		=> new() { XpZp = first.XpZp + second.XpZp, XpZn = first.XpZn + second.XpZn, XnZp = first.XnZp + second.XnZp, XnZn = first.XnZn + second.XnZn };

	public static SectorSurface operator -(SectorSurface first, SectorSurface second)
		=> new() { XpZp = first.XpZp - second.XpZp, XpZn = first.XpZn - second.XpZn, XnZp = first.XnZp - second.XnZp, XnZn = first.XnZn - second.XnZn };

	/// <summary>
	/// Converts the coordinates of the sector from world units to click units.
	/// </summary>
	/// <returns>A new <see cref="SectorSurface"/> instance with the coordinates converted to click units.</returns>
	public readonly SectorSurface WorldToClicks() => new()
	{
		SplitDirectionToggled = SplitDirectionToggled,
		DiagonalSplit = DiagonalSplit,

		XnZp = Clicks.FromWorld(XnZp),
		XpZp = Clicks.FromWorld(XpZp),
		XpZn = Clicks.FromWorld(XpZn),
		XnZn = Clicks.FromWorld(XnZn),
	};
}
