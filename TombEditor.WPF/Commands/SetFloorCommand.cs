using NLog;
using TombEditor.WPF.Extensions;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

public class SetFloorCommand : UnconditionalEditorCommand
{
	public SetFloorCommand(Editor editor, Logger? logger = null) : base(editor, logger)
	{ }

	public override void Execute(object? parameter)
	{
		if (!Editor.IsValidRoomAndSectorSelection)
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		room.SetSurfaceWithoutUpdate(area, false);
		room.CommitSmartBuildGeometry(area);
		Editor.RoomSectorPropertiesChange(room);
	}
}
