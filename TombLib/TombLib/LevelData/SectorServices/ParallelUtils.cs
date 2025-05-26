using System;
using System.Threading.Tasks;

namespace TombLib.LevelData.SectorServices;

/// <summary>
/// Provides utility methods for parallel processing of rectangular areas.
/// </summary>
public static class ParallelUtils
{
	/// <summary>
	/// Performs an action on each coordinate in a rectangular area in parallel, with optional filtering conditions.
	/// </summary>
	/// <param name="area">The rectangular area on which to perform the action.</param>
	/// <param name="action">The action to perform on each coordinate. Takes x and z coordinates and returns a boolean indicating success.</param>
	/// <param name="validXCondition">Optional predicate to determine if an x-coordinate should be processed. If <see langword="null" />, all x-coordinates are processed.</param>
	/// <param name="validZCondition">Optional predicate to determine if a z-coordinate should be processed. If <see langword="null" />, all z-coordinates are processed.</param>
	/// <returns><see langword="true" /> if the action returned <see langword="true" /> for at least one coordinate, otherwise <see langword="false" />.</returns>
	public static bool PerformActionOnArea(RectangleInt2 area, Func<int, int, bool> action,
		Func<int, bool> validXCondition = null, Func<int, bool> validZCondition = null)
	{
		bool success = false;
		object lockObj = new();

		Parallel.For(area.Y0, area.Y1 + 1, z =>
		{
			if (validZCondition is not null && !validZCondition(z))
				return;

			for (int x = area.X0; x <= area.X1; x++)
			{
				if (validXCondition is not null && !validXCondition(x))
					continue;

				bool result = action(x, z);

				if (success || !result)
					continue;

				lock (lockObj)
					success = true;
			}
		});

		return success;
	}

	/// <inheritdoc cref="PerformActionOnArea(RectangleInt2, Func{int, int, bool}, Func{int, bool}, Func{int, bool})" />
	public static bool PerformActionOnArea(RectangleInt2 area, Func<int, int, bool> action)
	{
		bool success = false;
		object lockObj = new();

		Parallel.For(area.Y0, area.Y1 + 1, z =>
		{
			for (int x = area.X0; x <= area.X1; x++)
			{
				bool result = action(x, z);

				if (success || !result)
					continue;

				lock (lockObj)
					success = true;
			}
		});

		return success;
	}
}
