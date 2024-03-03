using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

/// <summary>
/// Flattens the selected sectors to the average height of each individual sector.
/// </summary>
internal sealed class AverageSectorsCommand(INotifyPropertyChanged caller, Editor editor, int increments, BlockVertical vertical, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
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
				Block b = room.Blocks[x, z];
				int sum = 0;

				for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					sum += b.GetHeight(vertical, edge);

				sum /= increments;

				for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
					b.SetHeight(vertical, edge, sum / 4 * increments);
			}
		}

		CommitSmartBuildGeometry(room, area);
	}
}
