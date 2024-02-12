using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class SetSurfaceCommand(INotifyPropertyChanged caller, Editor editor, bool ceiling, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		SetSurfaceWithoutUpdate(room, area, ceiling);
		CommitSmartBuildGeometry(room, area);
		Editor.RoomSectorPropertiesChange(room);
	}

	private void SetSurfaceWithoutUpdate(Room room, RectangleInt2 area, bool ceiling)
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
}
