using NLog;
using System.Collections.Generic;
using System.Diagnostics;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Extensions;

public static class RoomExtensions
{
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
	private static readonly Editor editor = Editor.Instance;

	public static void SetSurfaceWithoutUpdate(this Room room, RectangleInt2 area, bool ceiling)
	{
		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				if (room.Blocks[x, z].Type == BlockType.BorderWall)
					continue;

				room.Blocks[x, z].Type = BlockType.Floor;

				if (ceiling)
					room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
				else
					room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
			}
		}
	}

	public static void CommitSmartBuildGeometry(this Room room, RectangleInt2 area)
	{
		var watch = Stopwatch.StartNew();
		room.SmartBuildGeometry(area, editor.Configuration.Rendering3D_HighQualityLightPreview);
		watch.Stop();

		logger.Debug($"Edit geometry time: {watch.ElapsedMilliseconds} ms");
		editor.RoomGeometryChange(room);
	}

	public static void ToggleBlockFlag(this Room room, RectangleInt2 area, BlockFlags flag)
	{
		var roomsToUpdate = new List<Room> { room };

		// Collect all affected rooms for undo
		for (int x = area.X0; x <= area.X1; x++)
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, editor.Configuration.UI_ProbeAttributesThroughPortals);

				if (!roomsToUpdate.Contains(currentBlock.Room))
					roomsToUpdate.Add(currentBlock.Room);
			}

		// Do undo
		editor.UndoManager.PushGeometryChanged(roomsToUpdate);

		if (editor.Configuration.UI_SetAttributesAtOnce)
		{
			// Set or unset flag, based on already existing flag prevalence in selected area
			int amount = (area.Width + 1) * (area.Height + 1);
			int prevalence = 0;

			for (int x = area.X0; x <= area.X1; x++)
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, editor.Configuration.UI_ProbeAttributesThroughPortals);

					if ((currentBlock.Block.Flags & flag) != BlockFlags.None)
						prevalence++;
				}

			bool toggle = prevalence == 0 || prevalence <= (amount / 2);

			// Do actual flag editing
			for (int x = area.X0; x <= area.X1; x++)
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, editor.Configuration.UI_ProbeAttributesThroughPortals);

					if (toggle)
						currentBlock.Block.Flags |= flag;
					else
						currentBlock.Block.Flags &= ~flag;
				}
		}
		else
		{
			// Do actual flag editing
			for (int x = area.X0; x <= area.X1; x++)
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, editor.Configuration.UI_ProbeAttributesThroughPortals);
					currentBlock.Block.Flags ^= flag;
				}
		}

		foreach (Room currentRoom in roomsToUpdate)
			editor.RoomSectorPropertiesChange(currentRoom);
	}
}
