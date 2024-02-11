using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class ToggleForceFloorSolidCommand : RoomGeometryCommand
{
	public ToggleForceFloorSolidCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
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
				room.Blocks[x, z].ForceFloorSolid = !room.Blocks[x, z].ForceFloorSolid;
		}

		CommitSmartBuildGeometry(room, area);
		Editor.RoomGeometryChange(room);
		Editor.RoomSectorPropertiesChange(room);
	}
}
