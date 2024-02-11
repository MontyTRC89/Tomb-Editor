using NLog;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

public abstract class RoomGeometryCommand : UnconditionalEditorCommand
{
	protected RoomGeometryCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	protected void CommitSmartBuildGeometry(Room room, RectangleInt2 area)
	{
		var watch = Stopwatch.StartNew();
		room.SmartBuildGeometry(area, Editor.Configuration.Rendering3D_HighQualityLightPreview);
		watch.Stop();

		Logger.Debug($"Edit geometry time: {watch.ElapsedMilliseconds} ms");
		Editor.RoomGeometryChange(room);
	}

	protected void SetSurfaceWithoutUpdate(Room room, RectangleInt2 area, bool ceiling)
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

	protected void ToggleBlockFlag(Room room, RectangleInt2 area, BlockFlags flag)
	{
		var roomsToUpdate = new List<Room> { room };

		// Collect all affected rooms for undo
		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, Editor.Configuration.UI_ProbeAttributesThroughPortals);

				if (!roomsToUpdate.Contains(currentBlock.Room))
					roomsToUpdate.Add(currentBlock.Room);
			}
		}

		// Do undo
		Editor.UndoManager.PushGeometryChanged(roomsToUpdate);

		if (Editor.Configuration.UI_SetAttributesAtOnce)
		{
			// Set or unset flag, based on already existing flag prevalence in selected area
			int amount = (area.Width + 1) * (area.Height + 1);
			int prevalence = 0;

			for (int x = area.X0; x <= area.X1; x++)
			{
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, Editor.Configuration.UI_ProbeAttributesThroughPortals);

					if ((currentBlock.Block.Flags & flag) != BlockFlags.None)
						prevalence++;
				}
			}

			bool toggle = prevalence == 0 || prevalence <= (amount / 2);

			// Do actual flag editing
			for (int x = area.X0; x <= area.X1; x++)
			{
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, Editor.Configuration.UI_ProbeAttributesThroughPortals);

					if (toggle)
						currentBlock.Block.Flags |= flag;
					else
						currentBlock.Block.Flags &= ~flag;
				}
			}
		}
		else
		{
			// Do actual flag editing
			for (int x = area.X0; x <= area.X1; x++)
			{
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, Editor.Configuration.UI_ProbeAttributesThroughPortals);
					currentBlock.Block.Flags ^= flag;
				}
			}
		}

		foreach (Room currentRoom in roomsToUpdate)
			Editor.RoomSectorPropertiesChange(currentRoom);
	}
}
