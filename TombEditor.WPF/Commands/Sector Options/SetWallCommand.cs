using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class SetWallCommand : RoomGeometryCommand
{
	public SetWallCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				if (room.Blocks[x, z].Type == BlockType.BorderWall)
					continue;

				room.Blocks[x, z].Type = BlockType.Wall;
				room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
				room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
			}
		}

		CommitSmartBuildGeometry(room, area);
		Editor.RoomSectorPropertiesChange(room);
	}
}
