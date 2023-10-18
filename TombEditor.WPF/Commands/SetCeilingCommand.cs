using NLog;
using TombEditor.WPF.Extensions;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal class SetCeilingCommand : UnconditionalEditorCommand
{
	public SetCeilingCommand(Editor editor, Logger? logger = null) : base(editor, logger)
	{ }

	public override void Execute(object? parameter)
	{
		if (!Editor.IsValidRoomAndSectorSelection)
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		room.SetSurfaceWithoutUpdate(area, true);
		room.CommitSmartBuildGeometry(area);
		Editor.RoomSectorPropertiesChange(room);
	}
}
