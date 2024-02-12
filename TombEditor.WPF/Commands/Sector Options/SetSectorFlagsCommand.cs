using NLog;
using System.Collections.Generic;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class SetSectorFlagsCommand(INotifyPropertyChanged caller, Editor editor, BlockFlags flags, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		// Check if flag is supported in the current engine version
		switch (flags)
		{
			case BlockFlags.Beetle:
			case BlockFlags.Monkey:
			case BlockFlags.TriggerTriggerer:
				if (!VersionCheck(Editor.Level.Settings.GameVersion >= TRVersion.Game.TR3, "This flag"))
					return;
				break;

			case BlockFlags.ClimbNegativeX:
			case BlockFlags.ClimbNegativeZ:
			case BlockFlags.ClimbPositiveX:
			case BlockFlags.ClimbPositiveZ:
				if (!VersionCheck(Editor.Level.Settings.GameVersion >= TRVersion.Game.TR2, "Climbing"))
					return;
				break;
		}

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		ToggleBlockFlag(room, area, flags);
	}

	private void ToggleBlockFlag(Room room, RectangleInt2 area, BlockFlags flag)
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
