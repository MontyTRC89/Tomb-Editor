using NLog;
using System;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

/// <summary>
/// Generates a sharp noise on the selected area.
/// </summary>
internal sealed class SharpRandomCommand(INotifyPropertyChanged caller, Editor editor, float strengthDirection, BlockVertical vertical, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		var rng = new Random();

		for (int x = 0; x <= area.Width; x++)
		{
			for (int z = 0; z <= area.Height; z++)
			{
				for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
				{
					room.ChangeBlockHeight(area.X0 + x, area.Y0 + z, vertical, edge,
						(int)Math.Round((float)rng.NextDouble() * strengthDirection) * Editor.IncrementReference);
				}
			}
		}

		CommitSmartBuildGeometry(room, area);
	}
}
