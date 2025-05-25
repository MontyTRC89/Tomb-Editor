using System;
using System.Linq;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;

namespace TombLib.LevelData.SectorServices;

public static class SurfaceToQuadConverter
{
	public static bool ConvertAreaToQuads(Room room, RectangleInt2 area, SectorVerticalPart vertical, int snapHeight)
	{
		bool success = false;
		bool isFloor = vertical.IsOnFloor();

		// De-triangulate the area by making all sectors in the area have quad surfaces
		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				Sector sector = room.GetSectorTry(x, z);

				if (sector is null || sector.IsFullWall)
					continue;

				ref SectorSurface surface = ref isFloor ? ref sector.Floor : ref sector.Ceiling;

				if (surface.IsQuad)
					continue;

				if (isFloor)
					DeTriangulateFloor(ref surface);
				else
					DeTriangulateCeiling(ref surface);

				sector.FixHeights(vertical, snapHeight);
				success = true;
			}
		}

		return success;
	}

	#region Floor

	private static void DeTriangulateFloor(ref SectorSurface surface)
	{
		int[] heights = new int[] { surface.XnZp, surface.XpZp, surface.XpZn, surface.XnZn };

		int minCornerHeight = surface.Min;
		int minCornerCount = heights.Count(height => height == minCornerHeight);

		if (minCornerCount == 1)
			HandleSingleCornerFloorCase(ref surface, minCornerHeight);
		else if (minCornerCount == 2)
			HandleTwoCornerFloorCase(ref surface, minCornerHeight);
		else if (minCornerCount == 3)
			SetAllCorners(ref surface, minCornerHeight); // Set all corners to min height to create a flat floor
	}

	private static void HandleSingleCornerFloorCase(ref SectorSurface surface, int minCornerHeight)
	{
		if (minCornerHeight == surface.XnZp)
		{
			// Slope from NW (lowest) to diagonal
			int xSlope = Math.Min(Math.Max(surface.XnZp - surface.XpZp, surface.XnZn - surface.XpZn), Math.Min(surface.XpZp - surface.XnZp, surface.XpZn - surface.XnZn));
			int zSlope = Math.Min(Math.Max(surface.XnZp - surface.XnZn, surface.XpZp - surface.XpZn), Math.Min(surface.XnZn - surface.XnZp, surface.XpZn - surface.XpZp));

			surface.XpZp = minCornerHeight - xSlope;
			surface.XpZn = minCornerHeight - xSlope - zSlope;
			surface.XnZn = minCornerHeight - zSlope;
		}
		else if (minCornerHeight == surface.XpZp)
		{
			// Slope from NE (lowest) to diagonal
			int xSlope = Math.Min(Math.Max(surface.XpZp - surface.XnZp, surface.XpZn - surface.XnZn), Math.Min(surface.XnZp - surface.XpZp, surface.XnZn - surface.XpZn));
			int zSlope = Math.Min(Math.Max(surface.XpZp - surface.XpZn, surface.XnZp - surface.XnZn), Math.Min(surface.XpZn - surface.XpZp, surface.XnZn - surface.XnZp));

			surface.XnZp = minCornerHeight - xSlope;
			surface.XpZn = minCornerHeight - zSlope;
			surface.XnZn = minCornerHeight - xSlope - zSlope;
		}
		else if (minCornerHeight == surface.XpZn)
		{
			// Slope from SE (lowest) to diagonal
			int xSlope = Math.Min(Math.Max(surface.XpZn - surface.XnZn, surface.XpZp - surface.XnZp), Math.Min(surface.XnZn - surface.XpZn, surface.XnZp - surface.XpZp));
			int zSlope = Math.Min(Math.Max(surface.XpZn - surface.XpZp, surface.XnZn - surface.XnZp), Math.Min(surface.XpZp - surface.XpZn, surface.XnZp - surface.XnZn));

			surface.XnZp = minCornerHeight - xSlope - zSlope;
			surface.XpZp = minCornerHeight - zSlope;
			surface.XnZn = minCornerHeight - xSlope;
		}
		else if (minCornerHeight == surface.XnZn)
		{
			// Slope from SW (lowest) to diagonal
			int xSlope = Math.Min(Math.Max(surface.XnZn - surface.XpZn, surface.XnZp - surface.XpZp), Math.Min(surface.XpZn - surface.XnZn, surface.XpZp - surface.XnZp));
			int zSlope = Math.Min(Math.Max(surface.XnZn - surface.XnZp, surface.XpZn - surface.XpZp), Math.Min(surface.XnZp - surface.XnZn, surface.XpZp - surface.XpZn));

			surface.XnZp = minCornerHeight - zSlope;
			surface.XpZp = minCornerHeight - xSlope - zSlope;
			surface.XpZn = minCornerHeight - xSlope;
		}
	}

	private static void HandleTwoCornerFloorCase(ref SectorSurface surface, int minCornerHeight)
	{
		// 2 corners at min height - check if they're adjacent or diagonal
		bool adjacentXn = surface.XnZp == minCornerHeight && surface.XnZn == minCornerHeight;
		bool adjacentXp = surface.XpZp == minCornerHeight && surface.XpZn == minCornerHeight;
		bool adjacentZp = surface.XnZp == minCornerHeight && surface.XpZp == minCornerHeight;
		bool adjacentZn = surface.XnZn == minCornerHeight && surface.XpZn == minCornerHeight;

		if (adjacentXn)
		{
			// Left edge is low
			int xSlope = Math.Max(minCornerHeight - surface.XpZp, minCornerHeight - surface.XpZn); // Use larger slope
			surface.XpZp = minCornerHeight - xSlope;
			surface.XpZn = minCornerHeight - xSlope;
		}
		else if (adjacentXp)
		{
			// Right edge is low
			int xSlope = Math.Max(minCornerHeight - surface.XnZp, minCornerHeight - surface.XnZn); // Use larger slope
			surface.XnZp = minCornerHeight - xSlope;
			surface.XnZn = minCornerHeight - xSlope;
		}
		else if (adjacentZp)
		{
			// Top edge is low
			int zSlope = Math.Max(minCornerHeight - surface.XnZn, minCornerHeight - surface.XpZn); // Use larger slope
			surface.XnZn = minCornerHeight - zSlope;
			surface.XpZn = minCornerHeight - zSlope;
		}
		else if (adjacentZn)
		{
			// Bottom edge is low
			int zSlope = Math.Max(minCornerHeight - surface.XnZp, minCornerHeight - surface.XpZp); // Use larger slope
			surface.XnZp = minCornerHeight - zSlope;
			surface.XpZp = minCornerHeight - zSlope;
		}
		else
		{
			// Diagonal corners at min height
			if (surface.XnZp == minCornerHeight && surface.XpZn == minCornerHeight)
			{
				// NW and SE corners are low
				int minOfHigherCorners = Math.Min(surface.XpZp, surface.XnZn);
				SetAllCorners(ref surface, minOfHigherCorners);
			}
			else if (surface.XpZp == minCornerHeight && surface.XnZn == minCornerHeight)
			{
				// NE and SW corners are low
				int minOfHigherCorners = Math.Min(surface.XnZp, surface.XpZn);
				SetAllCorners(ref surface, minOfHigherCorners);
			}
		}
	}

	#endregion Floor

	#region Ceiling

	private static void DeTriangulateCeiling(ref SectorSurface surface)
	{
		int[] heights = new int[] { surface.XnZp, surface.XpZp, surface.XpZn, surface.XnZn };

		int maxCornerHeight = surface.Max;
		int maxCornerCount = heights.Count(height => height == maxCornerHeight);

		if (maxCornerCount == 1)
			HandleSingleCornerCeilingCase(ref surface, maxCornerHeight);
		else if (maxCornerCount == 2)
			HandleTwoCornerCeilingCase(ref surface, maxCornerHeight);
		else if (maxCornerCount == 3)
			SetAllCorners(ref surface, maxCornerHeight); // Set all corners to max height to create a flat floor
	}

	private static void HandleSingleCornerCeilingCase(ref SectorSurface surface, int maxCornerHeight)
	{
		if (maxCornerHeight == surface.XnZp)
		{
			// Slope from NW (highest) to diagonal
			int xSlope = Math.Max(Math.Min(surface.XnZp - surface.XpZp, surface.XnZn - surface.XpZn), Math.Max(surface.XpZp - surface.XnZp, surface.XpZn - surface.XnZn));
			int zSlope = Math.Max(Math.Min(surface.XnZp - surface.XnZn, surface.XpZp - surface.XpZn), Math.Max(surface.XnZn - surface.XnZp, surface.XpZn - surface.XpZp));

			surface.XpZp = maxCornerHeight - xSlope;
			surface.XpZn = maxCornerHeight - xSlope - zSlope;
			surface.XnZn = maxCornerHeight - zSlope;
		}
		else if (maxCornerHeight == surface.XpZp)
		{
			// Slope from NE (highest) to diagonal
			int xSlope = Math.Max(Math.Min(surface.XpZp - surface.XnZp, surface.XpZn - surface.XnZn), Math.Max(surface.XnZp - surface.XpZp, surface.XnZn - surface.XpZn));
			int zSlope = Math.Max(Math.Min(surface.XpZp - surface.XpZn, surface.XnZp - surface.XnZn), Math.Max(surface.XpZn - surface.XpZp, surface.XnZn - surface.XnZp));

			surface.XnZp = maxCornerHeight - xSlope;
			surface.XpZn = maxCornerHeight - zSlope;
			surface.XnZn = maxCornerHeight - xSlope - zSlope;
		}
		else if (maxCornerHeight == surface.XpZn)
		{
			// Slope from SE (highest) to diagonal
			int xSlope = Math.Max(Math.Min(surface.XpZn - surface.XnZn, surface.XpZp - surface.XnZp), Math.Max(surface.XnZn - surface.XpZn, surface.XnZp - surface.XpZp));
			int zSlope = Math.Max(Math.Min(surface.XpZn - surface.XpZp, surface.XnZn - surface.XnZp), Math.Max(surface.XpZp - surface.XpZn, surface.XnZp - surface.XnZn));

			surface.XnZp = maxCornerHeight - xSlope - zSlope;
			surface.XpZp = maxCornerHeight - zSlope;
			surface.XnZn = maxCornerHeight - xSlope;
		}
		else if (maxCornerHeight == surface.XnZn)
		{
			// Slope from SW (highest) to diagonal
			int xSlope = Math.Max(Math.Min(surface.XnZn - surface.XpZn, surface.XnZp - surface.XpZp), Math.Max(surface.XpZn - surface.XnZn, surface.XpZp - surface.XnZp));
			int zSlope = Math.Max(Math.Min(surface.XnZn - surface.XnZp, surface.XpZn - surface.XpZp), Math.Max(surface.XnZp - surface.XnZn, surface.XpZp - surface.XpZn));

			surface.XnZp = maxCornerHeight - zSlope;
			surface.XpZp = maxCornerHeight - xSlope - zSlope;
			surface.XpZn = maxCornerHeight - xSlope;
		}
	}

	private static void HandleTwoCornerCeilingCase(ref SectorSurface surface, int maxCornerHeight)
	{
		// 2 corners at max height - check if they're adjacent or diagonal
		bool adjacentXn = surface.XnZp == maxCornerHeight && surface.XnZn == maxCornerHeight;
		bool adjacentXp = surface.XpZp == maxCornerHeight && surface.XpZn == maxCornerHeight;
		bool adjacentZp = surface.XnZp == maxCornerHeight && surface.XpZp == maxCornerHeight;
		bool adjacentZn = surface.XnZn == maxCornerHeight && surface.XpZn == maxCornerHeight;

		if (adjacentXn)
		{
			// Left edge is high
			int xSlope = Math.Min(maxCornerHeight - surface.XpZp, maxCornerHeight - surface.XpZn); // Use smaller slope
			surface.XpZp = maxCornerHeight - xSlope;
			surface.XpZn = maxCornerHeight - xSlope;
		}
		else if (adjacentXp)
		{
			// Right edge is high
			int xSlope = Math.Min(maxCornerHeight - surface.XnZp, maxCornerHeight - surface.XnZn); // Use smaller slope
			surface.XnZp = maxCornerHeight - xSlope;
			surface.XnZn = maxCornerHeight - xSlope;
		}
		else if (adjacentZp)
		{
			// Top edge is high
			int zSlope = Math.Min(maxCornerHeight - surface.XnZn, maxCornerHeight - surface.XpZn); // Use smaller slope
			surface.XnZn = maxCornerHeight - zSlope;
			surface.XpZn = maxCornerHeight - zSlope;
		}
		else if (adjacentZn)
		{
			// Bottom edge is high
			int zSlope = Math.Min(maxCornerHeight - surface.XnZp, maxCornerHeight - surface.XpZp); // Use smaller slope
			surface.XnZp = maxCornerHeight - zSlope;
			surface.XpZp = maxCornerHeight - zSlope;
		}
		else
		{
			// Diagonal corners at max height
			if (surface.XnZp == maxCornerHeight && surface.XpZn == maxCornerHeight)
			{
				// NW and SE corners are high
				int maxOfLowerCorners = Math.Max(surface.XpZp, surface.XnZn);
				SetAllCorners(ref surface, maxOfLowerCorners);
			}
			else if (surface.XpZp == maxCornerHeight && surface.XnZn == maxCornerHeight)
			{
				// NE and SW corners are high
				int maxOfLowerCorners = Math.Max(surface.XnZp, surface.XpZn);
				SetAllCorners(ref surface, maxOfLowerCorners);
			}
		}
	}

	#endregion Ceiling

	private static void SetAllCorners(ref SectorSurface surface, int height)
		=> surface.XnZp = surface.XpZp = surface.XpZn = surface.XnZn = height;
}
