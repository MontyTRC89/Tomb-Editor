using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class SetSurfaceCommand : RoomGeometryCommand
{
	private readonly bool _ceiling;

	public SetSurfaceCommand(INotifyPropertyChanged caller, Editor editor, bool ceiling, Logger? logger = null) : base(caller, editor, logger)
		=> _ceiling = ceiling;

	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		SetSurfaceWithoutUpdate(room, area, _ceiling);
		CommitSmartBuildGeometry(room, area);
		Editor.RoomSectorPropertiesChange(room);
	}
}
