using NLog;
using TombEditor.WPF.Extensions;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal class SetWallCommand : UnconditionalEditorCommand
{
	public SetWallCommand(Editor editor, Logger? logger = null) : base(editor, logger)
	{ }

	public override void Execute(object? parameter)
	{
		if (!Editor.IsValidRoomAndSectorSelection)
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

		room.CommitSmartBuildGeometry(area);
		Editor.RoomSectorPropertiesChange(room);
	}
}
