using NLog;
using System.Collections.Generic;
using System.ComponentModel;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class AddGhostBlocksCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null)
	: UnconditionalEditorCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		var ghostList = new List<GhostBlockInstance>();

		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				if (!room.Blocks[x, z].HasGhostBlock && !room.Blocks[x, z].IsAnyWall)
				{
					var ghost = new GhostBlockInstance() { SectorPosition = new VectorInt2(x, z) };
					room.AddObject(Editor.Level, ghost);
					Editor.ObjectChange(ghost, ObjectChangeType.Add);
					ghostList.Add(ghost);
				}
			}
		}

		if (ghostList.Count == 0)
		{
			Editor.SendMessage("No ghost blocks were added. You already have it in specified area.", PopupType.Warning);
			return;
		}

		Editor.RoomSectorPropertiesChange(room);
		Editor.UndoManager.PushGhostBlockCreated(ghostList); // Undo
	}
}
