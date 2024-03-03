using NLog;
using System;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

/// <summary>
/// Generates a smooth noise on the selected area.
/// </summary>
internal sealed class SmoothRandomCommand(INotifyPropertyChanged caller, Editor editor, float strengthDirection, BlockVertical vertical, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		float[,] changes = new float[area.Width + 2, area.Height + 2];
		var rng = new Random();

		for (int x = 1; x <= area.Width; x++)
		{
			for (int z = 1; z <= area.Height; z++)
				changes[x, z] = (float)rng.NextDouble() * strengthDirection;
		}

		for (int x = 0; x <= area.Width; x++)
		{
			for (int z = 0; z <= area.Height; z++)
			{
				for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
				{
					room.ChangeBlockHeight(area.X0 + x, area.Y0 + z, vertical, edge,
						(int)Math.Round(changes[x + edge.DirectionX(), z + edge.DirectionZ()]) * Editor.IncrementReference);
				}
			}
		}

		CommitSmartBuildGeometry(room, area);
	}
}
