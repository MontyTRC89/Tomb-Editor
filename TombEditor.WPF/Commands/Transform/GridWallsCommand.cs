using NLog;
using System;
using System.ComponentModel;
using System.Linq;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class GridWallsCommand(INotifyPropertyChanged caller, Editor editor, Room room, RectangleInt2 area, bool fiveDivisions, bool squares, bool fromUI = true, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (fromUI && !CheckForRoomAndBlockSelection())
			return;

		if (squares)
			GridWallsSquares(room, area, fiveDivisions, fromUI);
		else
			GridWalls(room, area, fiveDivisions);
	}

	private void GridWalls(Room room, RectangleInt2 area, bool fiveDivisions = false, bool fromUI = true)
	{
		// Don't undo if action is called implicitly (e.g. new room/level creation)
		if (fromUI)
			Editor.UndoManager.PushGeometryChanged(room);

		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				Block block = room.Blocks[x, z];

				if (block.IsAnyWall)
				{
					// Figure out corner heights
					int?[] floorHeights = new int?[(int)BlockEdge.Count];
					int?[] ceilingHeights = new int?[(int)BlockEdge.Count];

					for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					{
						int testX = x + edge.DirectionX(), testZ = z + edge.DirectionZ();
						floorHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Floor).Cast<int?>().Max();
						ceilingHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Ceiling).Cast<int?>().Min();

						floorHeights[(int)edge] /= Editor.IncrementReference;
						ceilingHeights[(int)edge] /= Editor.IncrementReference;
					}

					if (!floorHeights.Any(floorHeight => floorHeight.HasValue) || !ceilingHeights.Any(floorHeight => floorHeight.HasValue))
						continue; // We can only do it if there is information available

					block.ExtraFloorSubdivisions.Clear();
					block.ExtraCeilingSubdivisions.Clear();

					for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					{
						// Skip opposite diagonal step corner
						switch (block.Floor.DiagonalSplit)
						{
							case DiagonalSplit.XnZn:
								if (edge == BlockEdge.XpZp)
									continue;
								break;

							case DiagonalSplit.XnZp:
								if (edge == BlockEdge.XpZn)
									continue;
								break;

							case DiagonalSplit.XpZn:
								if (edge == BlockEdge.XnZp)
									continue;
								break;

							case DiagonalSplit.XpZp:
								if (edge == BlockEdge.XnZn)
									continue;
								break;
						}

						// Use the closest available vertical area information and divide it equally
						int floor = floorHeights[(int)edge]
							?? floorHeights[((int)edge + 1) % 4]
							?? floorHeights[((int)edge + 3) % 4]
							?? floorHeights[((int)edge + 2) % 4].Value;

						int ceiling = ceilingHeights[(int)edge]
							?? ceilingHeights[((int)edge + 1) % 4]
							?? ceilingHeights[((int)edge + 3) % 4]
							?? ceilingHeights[((int)edge + 2) % 4].Value;

						// TODO: Add support for more subdivisions

						int edHeight = (int)Math.Round(fiveDivisions ? ((floor * 4.0f) + (ceiling * 1.0f)) / 5.0f : floor),
							qaHeight = (int)Math.Round(fiveDivisions ? ((floor * 3.0f) + (ceiling * 2.0f)) / 5.0f : ((floor * 2.0f) + (ceiling * 1.0f)) / 3.0f),
							wsHeight = (int)Math.Round(fiveDivisions ? ((floor * 2.0f) + (ceiling * 3.0f)) / 5.0f : ((floor * 1.0f) + (ceiling * 2.0f)) / 3.0f),
							rfHeight = (int)Math.Round(fiveDivisions ? ((floor * 1.0f) + (ceiling * 4.0f)) / 5.0f : ceiling);

						edHeight *= Editor.IncrementReference;
						qaHeight *= Editor.IncrementReference;
						wsHeight *= Editor.IncrementReference;
						rfHeight *= Editor.IncrementReference;

						block.SetHeight(BlockVertical.FloorSubdivision2, edge, edHeight);
						block.Floor.SetHeight(edge, qaHeight);
						block.Ceiling.SetHeight(edge, wsHeight);
						block.SetHeight(BlockVertical.CeilingSubdivision2, edge, rfHeight);
					}
				}
			}
		}

		// Explicitly build geometry if action is called from user interface.
		// Otherwise (e.g. new room or level creation), do it implicitly, without calling global editor events.

		if (fromUI)
			CommitSmartBuildGeometry(room, area);
		else
			room.BuildGeometry();
	}

	private void GridWallsSquares(Room room, RectangleInt2 area, bool fiveDivisions = false, bool fromUI = true)
	{
		// Don't undo if action is called implicitly (e.g. new room/level creation)
		if (fromUI)
			Editor.UndoManager.PushGeometryChanged(room);

		int minFloor = int.MaxValue;
		int maxCeiling = int.MinValue;

		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				Block block = room.Blocks[x, z];

				if (block.IsAnyWall)
				{
					// Figure out corner heights
					int?[] floorHeights = new int?[(int)BlockEdge.Count];
					int?[] ceilingHeights = new int?[(int)BlockEdge.Count];

					for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					{
						int testX = x + edge.DirectionX(), testZ = z + edge.DirectionZ();
						floorHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Floor).Cast<int?>().Max();
						ceilingHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Ceiling).Cast<int?>().Min();

						floorHeights[(int)edge] /= Editor.IncrementReference;
						ceilingHeights[(int)edge] /= Editor.IncrementReference;
					}

					if (!floorHeights.Any(floorHeight => floorHeight.HasValue) || !ceilingHeights.Any(floorHeight => floorHeight.HasValue))
						continue; // We can only do it if there is information available

					for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					{
						// Skip opposite diagonal step corner
						switch (block.Floor.DiagonalSplit)
						{
							case DiagonalSplit.XnZn:
								if (edge == BlockEdge.XpZp)
									continue;
								break;

							case DiagonalSplit.XnZp:
								if (edge == BlockEdge.XpZn)
									continue;
								break;

							case DiagonalSplit.XpZn:
								if (edge == BlockEdge.XnZp)
									continue;
								break;

							case DiagonalSplit.XpZp:
								if (edge == BlockEdge.XnZn)
									continue;
								break;
						}

						// Use the closest available vertical area information and divide it equally
						int floor = floorHeights[(int)edge]
							?? floorHeights[((int)edge + 1) % 4]
							?? floorHeights[((int)edge + 3) % 4]
							?? floorHeights[((int)edge + 2) % 4].Value;

						int ceiling = ceilingHeights[(int)edge]
							?? ceilingHeights[((int)edge + 1) % 4]
							?? ceilingHeights[((int)edge + 3) % 4]
							?? ceilingHeights[((int)edge + 2) % 4].Value;

						if (floor <= minFloor)
							minFloor = floor;

						if (ceiling >= maxCeiling)
							maxCeiling = ceiling;
					}
				}
			}
		}

		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				Block block = room.Blocks[x, z];

				if (block.IsAnyWall)
				{
					for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					{
						// Skip opposite diagonal step corner
						switch (block.Floor.DiagonalSplit)
						{
							case DiagonalSplit.XnZn:
								if (edge == BlockEdge.XpZp)
									continue;
								break;

							case DiagonalSplit.XnZp:
								if (edge == BlockEdge.XpZn)
									continue;
								break;

							case DiagonalSplit.XpZn:
								if (edge == BlockEdge.XnZp)
									continue;
								break;

							case DiagonalSplit.XpZp:
								if (edge == BlockEdge.XnZn)
									continue;
								break;
						}

						// TODO: Add support for subdivisions

						int edHeight = (int)Math.Round(fiveDivisions ? ((minFloor * 4.0f) + (maxCeiling * 1.0f)) / 5.0f : minFloor),
							qaHeight = (int)Math.Round(fiveDivisions ? ((minFloor * 3.0f) + (maxCeiling * 2.0f)) / 5.0f : ((minFloor * 2.0f) + (maxCeiling * 1.0f)) / 3.0f),
							wsHeight = (int)Math.Round(fiveDivisions ? ((minFloor * 2.0f) + (maxCeiling * 3.0f)) / 5.0f : ((minFloor * 1.0f) + (maxCeiling * 2.0f)) / 3.0f),
							rfHeight = (int)Math.Round(fiveDivisions ? ((minFloor * 1.0f) + (maxCeiling * 4.0f)) / 5.0f : maxCeiling);

						edHeight *= Editor.IncrementReference;
						qaHeight *= Editor.IncrementReference;
						wsHeight *= Editor.IncrementReference;
						rfHeight *= Editor.IncrementReference;

						block.SetHeight(BlockVertical.FloorSubdivision2, edge, edHeight);
						block.Floor.SetHeight(edge, qaHeight);
						block.Ceiling.SetHeight(edge, wsHeight);
						block.SetHeight(BlockVertical.CeilingSubdivision2, edge, rfHeight);
					}
				}
			}
		}

		// Explicitly build geometry if action is called from user interface.
		// Otherwise (e.g. new room or level creation), do it implicitly, without calling global editor events.

		if (fromUI)
			CommitSmartBuildGeometry(room, area);
		else
			room.BuildGeometry();
	}
}
